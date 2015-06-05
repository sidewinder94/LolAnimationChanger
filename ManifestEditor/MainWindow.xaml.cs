using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ManifestEditor.Annotations;
using ManifestEditor.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using Utils.Text;
using Path = System.IO.Path;

namespace ManifestEditor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private String _manifestFile;
        private LoginScreen _selectedLoginScreen;


        public IEnumerable<LoginScreen> LoginScreens { get; set; }

        public LoginScreen SelectedLoginScreen
        {
            get { return _selectedLoginScreen; }
            set
            {
                _selectedLoginScreen = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            LoadManifest();
            LoadLoginScreens();
            InitializeComponent();
        }

        private void SaveManifest()
        {
            var json = JsonConvert.SerializeObject(LoginScreens);

            File.WriteAllText(_manifestFile, json, Encoding.GetEncoding("windows-1252"));
        }

        private void LoadLoginScreens()
        {
            var json = File.ReadAllText(_manifestFile, Encoding.GetEncoding("windows-1252"));

            var loginList = JsonConvert.DeserializeObject<List<LoginScreen>>(json);

            var dir = Path.GetDirectoryName(_manifestFile);

            var files = Directory.EnumerateFiles(dir, "*.zip", SearchOption.AllDirectories);

            loginList.AddRange(from file in files
                               where loginList.All(l => l.Filename != Path.GetFileName(file))
                               select new LoginScreen
                               {
                                   Filename = Path.GetFileName(file),
                                   Name = Path.GetFileNameWithoutExtension(file),
                                   SHA1 = ComputeSHA1(file)
                               });

            LoginScreens = loginList.OrderBy(l => l.ToString());
        }

        private void LoadManifest()
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = "Manifest File|manifest.json",
                Multiselect = false
            };

            ofd.ShowDialog();
            _manifestFile = ofd.FileName;
        }

        private static String ComputeSHA1(String path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            FileStream stream = null;
            try
            {
                var hasher = SHA1Managed.Create();
                stream = File.Open(path, FileMode.Open, FileAccess.Read);
                stream.Position = 0;
                StringBuilder sb = new StringBuilder();
                foreach (var b in hasher.ComputeHash(stream))
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return e.Message;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveManifest();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
