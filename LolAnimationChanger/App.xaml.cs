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

            Configuration.Load();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Configuration.Save();
        }
    }
}
