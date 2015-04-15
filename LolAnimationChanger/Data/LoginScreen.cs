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
        public String Name;
        public String NameFr;
        public String Filename;
        public String SHA1;

        public Boolean IsExtracted
        {
            get
            {
                return Directory.Exists(String.Format("{0}{1}{2}",
                    Configuration.GamePath,
                    Properties.Resources.ThemeDirPath,
                    Filename.Replace(".zip", "")));
            }
        }

        public Boolean IsDownloaded
        {
            get
            {
                return CheckIntegrity();
            }
        }

        public void Download(DownloadProgressChangedEventHandler progressHandler = null, AsyncCompletedEventHandler completedHandler = null)
        {
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
            try
            {
                ZipFile.ExtractToDirectory(String.Format("{0}{1}", BasePath, Filename),
                    String.Format("{0}{1}", Configuration.GamePath,
                        Properties.Resources.ThemeDirPath));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public Boolean Apply()
        {
            var themeSettingsFilePath = String.Format("{0}{1}", Configuration.GamePath,
                Properties.Resources.ThemeConfigFile);

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
                String config = File.ReadAllText(themeSettingsFilePath)
                    .RegExpReplace(@"(themeConfig=)(?:.*)", String.Format(@"$1{0}", Filename.Replace(".zip", "")));

                File.WriteAllText(themeSettingsFilePath, config);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(String.Format(Strings.ApplySettingsError,
                                              Configuration.GamePath,
                                              Properties.Resources.ThemeConfigFile),
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
                    ? NameFr
                    : Name;
        }

        #endregion
    }
}