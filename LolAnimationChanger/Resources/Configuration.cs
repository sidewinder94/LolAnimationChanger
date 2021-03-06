﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using GoogleAnalyticsTracker.Simple;
using LolAnimationChanger.Data;
using LolAnimationChanger.Properties;
using LolAnimationChanger.Resources.Lang;
using Microsoft.Win32;
using Newtonsoft.Json;
using Utils.Text;

namespace LolAnimationChanger.Resources
{
    public class Configuration
    {
        public static readonly SimpleTracker Tracker = new SimpleTracker(Properties.Resources.GATrackingId, Properties.Resources.GATrackingDomain, new SimpleTrackerEnvironment()
        {
            OsPlatform = typeof(LoginScreen).Assembly.FullName,
            Hostname = UserID.ToString()
        })
        {
            UserAgent = Properties.Resources.UserAgent,
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            UseSsl = true
        };


        private static String @Path
        {
            get
            {
#if DEBUG
                return "config.json";
#else
                var appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\LeagueOfLegendsAnimationChanger";

                if (!Directory.Exists(appPath)) Directory.CreateDirectory(appPath);

                return appPath + @"\config.json";
#endif
            }
        }

        public static void Save(String path = null)
        {
            if (path == null) path = @Path;

            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Instance._dataHolder));
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

        public static void Load(String path = null)
        {
            if (path == null) path = @Path;

            try
            {
                Instance._dataHolder = JsonConvert.DeserializeObject<DataHolder>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Instance._dataHolder = new DataHolder();
                String found =
                    (String)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Riot Games\League of Legends", "Path",
                        null);

                GamePath = found ?? @"C:\Riot Games\League of Legends\";

                if (found != null) PathSet = true;

                if (Directory.Exists(GamePath + @"RADS")) PathSet = true;

                Instance._dataHolder.EnableTracking = true;
            }

            if (Instance._dataHolder.UserId == Guid.Empty)
            {
                Instance._dataHolder.UserId = Guid.NewGuid();
            }

        }

        public static Boolean EnableTracking
        {
            get { return Instance._dataHolder.EnableTracking; }
            set { Instance._dataHolder.EnableTracking = value; }
        }

        public static Guid UserID
        {
            get { return Instance._dataHolder.UserId; }
            set { Instance._dataHolder.UserId = value; }
        }

        public static String GamePath
        {
            get { return Instance._dataHolder.GamePath; }
            set { Instance._dataHolder.GamePath = value; }
        }

        public static Boolean PathSet
        {
            get { return Instance._dataHolder.PathSet; }
            set { Instance._dataHolder.PathSet = value; }
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

        public static String CurrentSelectedTheme
        {
            get
            {
                var themeSettingsFilePath = String.Format("{0}{1}", GamePath,
                ThemeConfigFile);
                return File.ReadAllText(themeSettingsFilePath).RegExpReplace(@"(?:.*\W*)(?:themeConfig=)(\w+),.*", "$1");
            }
        }


        private String _launcherVersion;
        private static String GetLauncherVersion()
        {
            if (!Instance._launcherVersion.IsEmpty()) return Instance._launcherVersion;

            var dirs = Directory.EnumerateDirectories(String.Format("{0}{1}", GamePath, Properties.Resources.ReleasesPath));
            Instance._launcherVersion = dirs.Select(d =>
            {
                Version v;
                Version.TryParse(d.RegExpReplace(@".*\\", ""), out v);
                return v;
            }).OrderByDescending(v => v).First().ToString();
            return Instance._launcherVersion;
        }

        #region Singleton Implementation
        private DataHolder _dataHolder;

        private static Configuration Instance
        {
            get { return Holder.Config; }
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
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public Guid UserId;
            [DefaultValue(true)]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public bool EnableTracking;
        }
    }
}
