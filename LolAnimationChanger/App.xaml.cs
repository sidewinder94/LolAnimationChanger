using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LolAnimationChanger.Resources;
using Microsoft.Win32;

namespace LolAnimationChanger
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Configuration.Load();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Configuration.Save();
        }
    }
}
