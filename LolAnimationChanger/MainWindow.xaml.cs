using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using LolAnimationChanger.Annotations;
using LolAnimationChanger.Data;
using LolAnimationChanger.Resources;
using LolAnimationChanger.Resources.Lang;
using Microsoft.Win32;
using Newtonsoft.Json;
using Utils.Text;
using Utils.Misc;

namespace LolAnimationChanger
{
    //Selected Theme : \Riot Games\League of Legends\RADS\projects\lol_air_client\releases\0.0.1.139\deploy\theme.properties
    //Available Themes : \Riot Games\League of Legends\RADS\projects\lol_air_client\releases\0.0.1.139\deploy\mod\lgn\themes
    //Default Path : C:\Riot Games
    //If Istalled Found in : HKLM/SOFTWARE/Riot Games/League of Legends

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Double _downloadProgress = 0.0d;
        private long _bytesPerSecond;
        private Boolean _downloadEnabled = true;
        private Visibility _downloadVisibility = Visibility.Collapsed;
        private String _downloadSpeed;
        private DateTime _lastUpdate;
        private long _lastBytes = 0;

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

        public List<LoginScreen> LoginScreens { get; set; }
        public LoginScreen DownloadScreen { get; set; }

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

        public MainWindow()
        {
            DownloadLoginScreenList();
            CheckConfiguration();

            InitializeComponent();
        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadEnabled = false;
            if (DownloadScreen != null)
            {
                DownloadScreen.Download(DownloadProgressHandler, DownloadCompletedHandler);
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
            if (_bytesPerSecond != 0) DownloadSpeed += String.Format(" {0}/s", Misc.HumanReadableByteCount(_bytesPerSecond));

        }

        private void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs args)
        {
            if (DownloadScreen.CheckIntegrity())
            {
                DownloadEnabled = true;
                MessageBox.Show(Strings.DownloadSucceeded, Strings.DownloadSuccess,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var result = MessageBox.Show(Strings.DownloadFailed, Strings.DownloadError, MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes) return;
                DownloadProgress = 0.0d;
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
                var json = wc.DownloadString(Properties.Resources.RootAddress + Properties.Resources.ManifestName);
                LoginScreens = JsonConvert.DeserializeObject<List<LoginScreen>>(json).OrderBy(e => e.ToString()).ToList();
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
                Configuration.GamePath = path;
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
        #endregion
    }
}
