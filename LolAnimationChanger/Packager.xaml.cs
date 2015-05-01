using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using LolAnimationChanger.Annotations;
using LolAnimationChanger.Data;
using LolAnimationChanger.Resources;
using LolAnimationChanger.Resources.Lang;

namespace LolAnimationChanger
{
    /// <summary>
    /// Logique d'interaction pour Packager.xaml
    /// </summary>
    public partial class Packager : Window, INotifyPropertyChanged
    {

        private List<LoginScreen> _unkownLoginScreens = new List<LoginScreen>();

        public List<LoginScreen> UnkownLoginScreens
        {
            get
            {
                return _unkownLoginScreens;

            }
            set
            {
                _unkownLoginScreens = value;
                OnPropertyChanged();
            }
        }

        public Packager()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var path in UnkownLoginScreens.Where(l => l.ToExport)
                    .Select(uls => String.Format(@"{0}{1}{2}",
                        Configuration.GamePath,
                        Configuration.ThemeDirPath,
                        uls.Name)))
                {
                    //If the archive already exists => Delete
                    if (File.Exists(path + ".zip")) File.Delete(path + ".zip");


                    //Zip the theme directory
                    ZipFile.CreateFromDirectory(path, path + ".zip", CompressionLevel.Optimal, true);
                }

                Process.Start("explorer.exe",
                    String.Format("/root,{0}{1}", Configuration.GamePath,
                                                  Configuration.ThemeDirPath));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(
                    String.Format(Strings.PackingError, Configuration.GamePath, Configuration.ThemeDirPath),
                    Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
    }
}
