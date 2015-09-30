using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using LolAnimationChanger.Annotations;
using LolAnimationChanger.Resources;
using LolAnimationChanger.Resources.Lang;
using Utils.Text;

namespace LolAnimationChanger.Data
{
    //Never Instantiated explicitely, but instantiated by JSON Deserialization
    [UsedImplicitly]
    public class LoginScreen
    {
        private const String BasePath = @"downloads\";
        public String Name { get; set; }
        public String NameFr { get; set; }
        public String Filename { get; set; }
        public String SHA1 { get; set; }
        public const String RequiredResources = ",lolBrand,parchment";
        public Boolean ToExport { get; set; }

        private Boolean _isDownloaded = false;

        public Boolean IsExtracted
        {
            get
            {
                if (Filename == null && SHA1 == null) return true;

                return Directory.Exists(String.Format("{0}{1}{2}",
                    Configuration.GamePath,
                    Configuration.ThemeDirPath,
                    Filename.Replace(".zip", "")));
            }
        }

        public Boolean IsDownloaded
        {
            get
            {
                if (_isDownloaded) return _isDownloaded;

                _isDownloaded = CheckIntegrity();

                return _isDownloaded;
            }
            private set { _isDownloaded = value; }
        }

        public void Download(DownloadProgressChangedEventHandler progressHandler = null, AsyncCompletedEventHandler completedHandler = null)
        {
            IsDownloaded = false;
            if (!Directory.Exists(BasePath))
            {
                try
                {
                    Directory.CreateDirectory(BasePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    MessageBox.Show(Strings.CantCreateDLDir, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            using (var wc = new WebClient())
            {
                if (progressHandler != null) wc.DownloadProgressChanged += progressHandler;
                if (completedHandler != null) wc.DownloadFileCompleted += completedHandler;
                wc.DownloadFileAsync(new Uri(Properties.Resources.RootAddress + Filename), BasePath + Filename);
            }
        }

        public Boolean CheckIntegrity()
        {
            if (!File.Exists(BasePath + Filename)) return false;
            FileStream stream = null;
            try
            {
                var hasher = SHA1Managed.Create();
                stream = File.Open(BasePath + Filename, FileMode.Open, FileAccess.Read);
                stream.Position = 0;
                StringBuilder sb = new StringBuilder();
                foreach (var b in hasher.ComputeHash(stream))
                {
                    sb.Append(b.ToString("X2"));
                }

                return SHA1.Equals(sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        public Boolean Extract()
        {
            var dirName = String.Format("{0}{1}{2}", Configuration.GamePath, Configuration.ThemeDirPath,
                    Filename.Replace(".zip", ""));
            try
            {

                if (Directory.Exists(dirName))
                {
                    //If by chance there is already a backu folder we delete it
                    if (Directory.Exists(String.Format("{0}.bak", dirName)))
                    {
                        Directory.Delete(String.Format("{0}.bak", dirName), true);
                    }
                    //We backup the current folder to a .bak folder
                    Directory.Move(dirName,
                        String.Format("{0}.bak", dirName));
                }
                ZipFile.ExtractToDirectory(String.Format("{0}{1}", BasePath, Filename),
                    String.Format("{0}{1}", Configuration.GamePath,
                        Configuration.ThemeDirPath));

                return true;
            }
            catch (Exception e)
            {
                //If something failed, we retore the backup folder
                if (Directory.Exists(String.Format("{0}.bak", dirName)))
                {
                    Directory.Move(String.Format("{0}.bak", dirName), dirName);
                }
                Console.WriteLine(e);

                return false;
            }
            finally
            {
                //we cleanup after ourselves, and selete the backup folder in case it's still here (meaning all went well)
                if (Directory.Exists(String.Format("{0}.bak", dirName)))
                {
                    Directory.Delete(String.Format("{0}.bak", dirName), true);
                }
            }
        }

        public Boolean Apply()
        {
            var themeSettingsFilePath = String.Format("{0}{1}", Configuration.GamePath,
                Configuration.ThemeConfigFile);

            if (!File.Exists(String.Format("{0}.bak", themeSettingsFilePath)))
            {
                try
                {
                    File.Copy(themeSettingsFilePath, String.Format("{0}.bak", themeSettingsFilePath));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var result = MessageBox.Show(Strings.SettingsBackupError, Strings.Error, MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (result == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }

            try
            {

                // ReSharper disable once PossibleNullReferenceException
                var themeDirName = (Filename == null && SHA1 == null) ? Name : Filename.Replace(".zip", "");

                String config = File.ReadAllText(themeSettingsFilePath)
                    .RegExpReplace(@"(themeConfig=)(?:.*)", String.Format(@"$1{0}{1}", themeDirName, RequiredResources));

                File.WriteAllText(themeSettingsFilePath, config);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(String.Format(Strings.ApplySettingsError,
                                              Configuration.GamePath,
                                              Configuration.ThemeConfigFile),
                                Strings.Error,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }

        }

        #region Overrides of Object

        /// <summary>
        /// Retourne une chaîne qui représente l'objet actuel.
        /// </summary>
        /// <returns>
        /// Chaîne qui représente l'objet en cours.
        /// </returns>
        public override string ToString()
        {
            if (NameFr.IsEmpty()) return Name;

            return CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCultureIgnoreCase)
                    ? NameFr//.ReEncodeString("iso-8859-1", "utf-8")
                    : Name;
        }

        #endregion
    }
}