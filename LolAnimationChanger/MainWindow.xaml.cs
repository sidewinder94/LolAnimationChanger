using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GoogleAnalyticsTracker.Core.TrackerParameters;
using LolAnimationChanger.Annotations;
using LolAnimationChanger.Data;
using LolAnimationChanger.Properties;
using LolAnimationChanger.Resources;
using LolAnimationChanger.Resources.Lang;
using Microsoft.Win32;
using Newtonsoft.Json;
using Utils.Misc;
using Utils.Text;
using Utils.Reflection;

namespace LolAnimationChanger
{
    //Selected Theme : \Riot Games\League of Legends\RADS\projects\lol_air_client\releases\0.0.1.139\deploy\theme.properties
    //Available Themes : \Riot Games\League of Legends\RADS\projects\lol_air_client\releases\0.0.1.139\deploy\mod\lgn\themes
    //Default Path : C:\Riot Games
    //If Istalled Found in : HKLM/SOFTWARE/Riot Games/League of Legends

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private Double _downloadProgress;
        private long _bytesPerSecond;
        private Boolean _downloadEnabled = true;
        private Visibility _downloadVisibility = Visibility.Collapsed;
        private String _downloadSpeed;
        private DateTime _lastUpdate;
        private long _lastBytes;
        private Boolean _displayUnkown;
        private Boolean _forceExtraction;
        private string _searchText;

        public String SearchText
        {
            get { return DownloadScreen != null ? DownloadScreen.ToString() : ""; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                CollectionViewSource.GetDefaultView(LoginScreensList.ItemsSource).Refresh();
            }
        }

        public String TitleString
        {
            get
            {
                if (IsNotAdmin)
                {
                    return Strings.MainTitle;
                }
                return String.Format("{0} ({1})", Strings.MainTitle,
                    Strings.Admin);
            }
        }

        public Boolean IsNotAdmin
        {
            get
            {
                WindowsIdentity identity = null;
                try
                {
                    identity = WindowsIdentity.GetCurrent();
                }
                catch (SecurityException)
                {
                    return true;
                }
                if (identity == null) return true;

                return !(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator));
            }
        }

        public Boolean DownloadEnabled
        {
            get
            {
                return _downloadEnabled;

            }
            set
            {
                _downloadEnabled = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LoginScreen> LoginScreens { get; set; }

        public LoginScreen DownloadScreen { get; set; }

        public LoginScreen SelectedTheme { get; set; }

        public ObservableCollection<LoginScreen> AvailableScreens
        {
            get
            {
                return GetAvailableLoginScreens();
            }
        }

        public String DownloadSpeed
        {
            get { return _downloadSpeed; }
            set
            {
                _downloadSpeed = value;
                OnPropertyChanged();
            }
        }

        public Visibility DownloadProgressVisibility
        {
            get { return _downloadVisibility; }
            set
            {
                _downloadVisibility = value;
                OnPropertyChanged();
            }
        }

        public Double DownloadProgress
        {
            get
            {
                return _downloadProgress;
            }
            set
            {
                _downloadProgress = value;
                OnPropertyChanged();
            }
        }

        public Boolean DisplayUnknown
        {
            get
            {
                return _displayUnkown;
            }
            set
            {
                _displayUnkown = value;
                OnPropertyChanged(x => x.AvailableScreens);
            }
        }

        public Boolean ForceExtraction
        {
            get
            {
                return _forceExtraction;
            }
            set
            {
                _forceExtraction = value;
                OnPropertyChanged(x => x.AvailableScreens);
                SelectCurrentTheme();
            }
        }

        public MainWindow()
        {

            CheckConfiguration();
            InitializeComponent();
            DownloadLoginScreenList();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadEnabled = false;
            if (DownloadScreen != null)
            {
                _lastBytes = 0;
                DownloadScreen.Download(DownloadProgressHandler, DownloadCompletedHandler);
            }
        }

        private ObservableCollection<LoginScreen> GetAvailableLoginScreens()
        {
            if (LoginScreens == null) return new ObservableCollection<LoginScreen>(Enumerable.Empty<LoginScreen>());

            IEnumerable<LoginScreen> result;
            if (ForceExtraction)
            {
                result = LoginScreens.Where(l => l.IsDownloaded);
            }
            else
            {
                result = LoginScreens.Where(l => l.IsDownloaded || l.IsExtracted);

                if (DisplayUnknown)
                {
                    var dirs = Directory.EnumerateDirectories(String.Format("{0}{1}", Configuration.GamePath,
                        Configuration.ThemeDirPath));
                    result = result.Concat(from dir in dirs
                                           where
                                               !LoginScreens.Any(
                                                   l => l.Filename.Equals(String.Format("{0}.zip",
                                                       dir.RegExpReplace(@"^.*\\", "")))) && !dir.Contains("parchment")
                                           select new LoginScreen()
                                           {
                                               Name = dir.RegExpReplace(@"^.*\\", ""),
                                           });
                }
            }
            return new ObservableCollection<LoginScreen>(result.OrderBy(l => l.ToString()));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTheme == null) return;

            if (!SelectedTheme.IsExtracted || ForceExtraction)
            {
                if (!SelectedTheme.Extract())
                {
                    MessageBox.Show(String.Format(Strings.ExtractionError,
                                                  Configuration.GamePath,
                                                  Configuration.ThemeDirPath),
                                    Strings.Error,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                }
            }

            if (SelectedTheme.Apply())
            {
                MessageBox.Show(Strings.ApplySuccess, Strings.Success, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(Strings.ApplyFailed, Strings.Error, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        #region Download Progress Handlers

        private void DownloadProgressHandler(object sender, DownloadProgressChangedEventArgs args)
        {
            DownloadProgressVisibility = Visibility.Visible;
            DownloadProgress = (args.BytesReceived * 100.0d) / (args.TotalBytesToReceive);
            if (_lastBytes == 0)
            {
                _lastUpdate = DateTime.Now;
                _lastBytes = args.BytesReceived;
                DownloadSpeed = String.Format("{0:0.00}%", Math.Round(DownloadProgress, 2));
                return;
            }

            var now = DateTime.Now;
            var timeSpan = now - _lastUpdate;

            if (timeSpan.Seconds != 0)
            {
                var bytesChange = args.BytesReceived - _lastBytes;
                _bytesPerSecond = bytesChange / timeSpan.Seconds;

                _lastBytes = args.BytesReceived;
                _lastUpdate = now;
            }

            DownloadSpeed = String.Format("{0:0.00}%", Math.Round(DownloadProgress, 2));
            if (_bytesPerSecond != 0) DownloadSpeed += String.Format(" {0}/s",
                Misc.HumanReadableByteCount(_bytesPerSecond));

        }

        private void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs args)
        {
            if (DownloadScreen.CheckIntegrity())
            {
                DownloadEnabled = true;
                OnPropertyChanged("AvailableScreens");
                MessageBox.Show(Strings.DownloadSucceeded, Strings.DownloadSuccess,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var result = MessageBox.Show(Strings.DownloadFailed, Strings.DownloadError, MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes)
                {
                    DownloadEnabled = true;
                    return;
                }
                DownloadProgress = 0.0d;
                _lastBytes = 0;
                DownloadScreen.Download(DownloadProgressHandler, DownloadCompletedHandler);
            }
        }

        #endregion

        #region Initialization methods

        private void DownloadLoginScreenList()
        {
            var wc = new WebClient();
            try
            {
                wc.DownloadStringCompleted += (o, e) =>
                {
                    LoginScreens = JsonConvert.DeserializeObject<IEnumerable<LoginScreen>>(e.Result).OrderBy(l => l.ToString());
                    OnPropertyChanged(x => x.LoginScreens);
                    OnPropertyChanged(x => x.AvailableScreens);
                    SelectCurrentTheme();
                    CollectionViewSource.GetDefaultView(LoginScreensList.ItemsSource).Filter = UserFilter;
                    if (Configuration.EnableTracking)
                    {
                        Configuration.Tracker.TrackAsync(
                            new EventTracking()
                            {
                                ClientId = Configuration.UserID.ToString(),
                                Action = "Downloaded Manifest",
                                DocumentTitle = Properties.Resources.ManifestName,
                                DocumentPath = Properties.Resources.ManifestName
                            });
                    }
                };
                wc.DownloadStringAsync(new Uri(Properties.Resources.RootAddress + Properties.Resources.ManifestName));

            }
            catch (Exception)
            {
                MessageBox.Show(Strings.ConnectionMessage,
                    Strings.ConnectionError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
            }
            finally
            {
                wc.Dispose();
            }

        }

        private static void CheckConfiguration()
        {
            if (Configuration.PathSet) return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".exe",
                DereferenceLinks = true,
                Filter = "League of Legends Launcher|lol.launcher.exe;lol.launcher.admin.exe",
                Multiselect = false,
                Title = Strings.FindLolLauncher
            };
            ofd.ShowDialog();
            String path = ofd.FileName;
            path = path.RegExpReplace(@"\\[^\\]+$", "");
            if (Directory.Exists(path + @"\RADS"))
            {
                Configuration.PathSet = true;
                Configuration.GamePath = path + @"\";
            }
            else
            {
                MessageBox.Show(Strings.BadFolder,
                    Strings.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private void OnPropertyChanged<T>(Expression<Func<MainWindow, T>> propertyPath)
        {
            var info = this.GetPropertyInfo(propertyPath);
            OnPropertyChanged(info.Name);
        }


        #endregion

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void PackNewThemesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = new List<LoginScreen>();
            var dirs = Directory.EnumerateDirectories(String.Format("{0}{1}", Configuration.GamePath,
                Configuration.ThemeDirPath));
            result.AddRange(from dir in dirs
                            where
                                !LoginScreens.Any(
                                    l => l.Filename.Equals(String.Format("{0}.zip", dir.RegExpReplace(@"^.*\\", "")))) && !dir.Contains("parchment")
                            select new LoginScreen()
                            {
                                Name = dir.RegExpReplace(@"^.*\\", ""),
                            });


            var packager = new Packager()
            {
                UnkownLoginScreens = result
            };
            packager.ShowDialog();
        }

        private void RunAsAdmin_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo(Process.GetCurrentProcess().ProcessName + ".exe")
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);
            Close();
        }


        private bool UserFilter(object item)
        {
            if (_searchText.IsEmpty()) return true;
            var i = (LoginScreen)item;
            return i.ToString().ToLower().Contains(_searchText.ToLower());
        }


        private void LoginScreensList_DropDownClosed(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                SearchText = "";
            }
        }

        private void SelectCurrentTheme()
        {
            var curr = AvailableScreens.FirstOrDefault(ls => ls.Filename.Contains(Configuration.CurrentSelectedTheme));
            if (curr != null)
            {
                SelectedTheme = curr;
                OnPropertyChanged(x => x.SelectedTheme);
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (AvailableScreens.Count >= 1)
            {
                var button = sender as Button;
                var loginScreen = button != null ? button.DataContext as LoginScreen : null;
                if (loginScreen != null) loginScreen.Delete();
                OnPropertyChanged(x => x.AvailableScreens);
            }
            else
            {
                MessageBox.Show(Strings.LastThemeDeletionError, Strings.Warning, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

        }
    }
}
