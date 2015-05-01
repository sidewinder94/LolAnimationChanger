using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace LolAnimationChanger
{
    /// <summary>
    /// Logique d'interaction pour About.xaml
    /// </summary>
    public partial class About
    {
        public About()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
