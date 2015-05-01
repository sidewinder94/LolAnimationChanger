using System.Windows;
using LolAnimationChanger.Resources;

namespace LolAnimationChanger
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
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
