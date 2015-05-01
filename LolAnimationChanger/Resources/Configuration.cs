using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using LolAnimationChanger.Resources.Lang;
using Microsoft.Win32;
using Newtonsoft.Json;
using Utils.Text;

namespace LolAnimationChanger.Resources
{
    public class Configuration
    {
        public static void Save(String path = "config.json")
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(GetInstance()._dataHolder));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(Strings.SaveConfigError,
                                Strings.Error,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        public static void Load(String path = "config.json")
        {
            try
            {
                GetInstance()._dataHolder = JsonConvert.DeserializeObject<DataHolder>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                GetInstance()._dataHolder = new DataHolder();
                String found =
                    (String)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Riot Games\League of Legends", "Path",
                        null);

                GamePath = found ?? @"C:\Riot Games\League of Legends\";

                if (found != null) PathSet = true;

                if (Directory.Exists(GamePath + @"RADS")) PathSet = true;
            }
        }

        public static String GamePath
        {
            get { return GetInstance()._dataHolder.GamePath; }
            set { GetInstance()._dataHolder.GamePath = value; }
        }

        public static Boolean PathSet
        {
            get { return GetInstance()._dataHolder.PathSet; }
            set { GetInstance()._dataHolder.PathSet = value; }
        }

        public static String ThemeConfigFile
        {
            get { return String.Format(Properties.Resources.ThemeConfigFile, GetLauncherVersion()); }
        }

        public static String ThemeDirPath
        {
            get
            {
                return String.Format(Properties.Resources.ThemeDirPath, GetLauncherVersion());
            }
        }



        private String _launcherVersion;
        private static String GetLauncherVersion()
        {
            if (!GetInstance()._launcherVersion.IsEmpty()) return GetInstance()._launcherVersion;

            var dirs = Directory.EnumerateDirectories(String.Format("{0}{1}", GamePath, Properties.Resources.ReleasesPath));
            GetInstance()._launcherVersion = dirs.Select(d =>
            {
                Version v;
                Version.TryParse(d.RegExpReplace(@".*\\", ""), out v);
                return v;
            }).OrderByDescending(v => v).First().ToString();
            return GetInstance()._launcherVersion;
        }

        #region Singleton Implementation
        private DataHolder _dataHolder;

        private static Configuration GetInstance()
        {
            return Holder.Config;
        }



        private Configuration()
        {
            _dataHolder = new DataHolder();
        }

        private static class Holder
        {
            internal static readonly Configuration Config = new Configuration();
        }
        #endregion

        [Serializable]
        //Public is needed for serialization to JSON and since it's a holder class, we have no need for the members of the outer class
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private class DataHolder
        {

            public String GamePath = "";
            public Boolean PathSet;
        }
    }
}
