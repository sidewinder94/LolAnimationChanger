using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LolAnimationChanger.Annotations;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LolAnimationChanger.Resources
{
    public class Configuration
    {
        public static void Save(String path = "config.json")
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(GetInstance()._dataHolder));
        }

        public static void Load(String path = "config.json")
        {
            try
            {
                GetInstance()._dataHolder = JsonConvert.DeserializeObject<DataHolder>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        #region Singleton Implementation
        private DataHolder _dataHolder;

        private static Configuration GetInstance()
        {
            return Configuration.Holder.Config;
        }



        private Configuration()
        {

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
            public Boolean PathSet = false;

            public DataHolder()
            {

            }
        }





    }
}
