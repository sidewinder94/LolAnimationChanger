using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using LolAnimationChanger.Resources.Lang;

namespace LolAnimationChanger.Data
{
    public class LoginScreen
    {
        private static String _basePath = @"downloads\";
        public String Name;
        public String NameFr;
        public String Filename;
        public String SHA1;

        public void Download(DownloadProgressChangedEventHandler progressHandler = null, AsyncCompletedEventHandler completedHandler = null)
        {
            if (!Directory.Exists(_basePath))
            {
                try
                {
                    Directory.CreateDirectory(_basePath);
                }
                catch (Exception)
                {
                    MessageBox.Show(Strings.CantCreateDLDir, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            using (var wc = new WebClient())
            {
                if (progressHandler != null) wc.DownloadProgressChanged += progressHandler;
                if (completedHandler != null) wc.DownloadFileCompleted += completedHandler;
                wc.DownloadFileAsync(new Uri(Properties.Resources.RootAddress + Filename), _basePath + Filename);
            }
        }


        public Boolean CheckIntegrity()
        {
            try
            {
                var hasher = SHA1Managed.Create();
                var stream = File.Open(_basePath + Filename, FileMode.Open, FileAccess.Read);
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
            return CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCultureIgnoreCase) ? NameFr : Name;
        }

        #endregion
    }
}