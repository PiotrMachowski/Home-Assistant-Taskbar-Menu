using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            NameAndVersionLabel.Content = ResourceProvider.NameAndVersion();
            CopyrightInfoLabel.Content = ResourceProvider.CopyrightInfo();
            RepoHyperlink.NavigateUri = new Uri(ResourceProvider.RepoUri());
        }

        private void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}