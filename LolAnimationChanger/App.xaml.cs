using System;
using System.Windows;

using LolAnimationChanger.Properties;
using LolAnimationChanger.Resources;

namespace LolAnimationChanger
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Settings.Default.UserID == Guid.Empty)
            {
                Settings.Default.UserID = Guid.NewGuid();
                Settings.Default.Save();
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            Configuration.Load();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Configuration.Save();
        }
    }
}
