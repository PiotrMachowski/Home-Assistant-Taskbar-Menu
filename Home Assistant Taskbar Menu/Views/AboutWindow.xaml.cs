using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}