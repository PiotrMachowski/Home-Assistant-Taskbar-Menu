using System;
using System.Linq;
using System.Windows;
using Home_Assistant_Taskbar_Menu.Connection;
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
            bool enableLogging = e.Args.Length == 0;
            Storage.InitConfigDirectory(enableLogging);
            Configuration configuration = Storage.RestoreConfiguration();
            ViewConfiguration viewConfiguration = Storage.RestoreViewConfiguration();
            if (configuration != null && e.Args.Length > 0)
            {
                if (e.Args[0] == "call_service" && e.Args.Length > 2)
                {
                    var service = e.Args[1];
                    var serviceData = string.Join(" ", e.Args.Skip(2).ToList());
                    CallService(configuration, service, serviceData);
                    Shutdown();
                    return;
                }
            }

            StartUi(viewConfiguration, configuration);
        }

        private static void StartUi(ViewConfiguration viewConfiguration, Configuration configuration)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(viewConfiguration.GetProperty(ViewConfiguration.ThemeKey) == ViewConfiguration.LightTheme
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

        private static void CallService(Configuration configuration, string service, string data)
        {
            var restClient = new HomeAssistantRestClient(configuration);
            var serviceSplit = service.Split('.');
            if (serviceSplit.Length == 2)
            {
                restClient.CallService(serviceSplit[0], serviceSplit[1], data);
            }
        }
    }
}