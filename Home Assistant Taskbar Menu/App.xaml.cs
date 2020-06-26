using System;
using System.Windows;
using Home_Assistant_Taskbar_Menu.Utils;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            Configuration configuration = Storage.RestoreConfiguration();
            ViewConfiguration viewConfiguration = Storage.RestoreViewConfiguration();

            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(viewConfiguration.Name == ViewConfiguration.LightTheme
                ? new MaterialDesignLightTheme()
                : (IBaseTheme) new MaterialDesignDarkTheme());
            paletteHelper.SetTheme(theme);

            if (configuration == null)
            {
                ConsoleWriter.WriteLine("NO CONFIGURATION", ConsoleColor.Red);
                new AuthWindow().Show();
            }
            else
            {
                ConsoleWriter.WriteLine($"configuration.Url = {configuration.Url}", ConsoleColor.Green);
                new MainWindow(configuration, viewConfiguration).Show();
            }
        }
    }
}