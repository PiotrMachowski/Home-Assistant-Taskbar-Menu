using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Home_Assistant_Taskbar_Menu.Utils;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private static readonly Brush InvalidColor = new SolidColorBrush(Color.FromArgb(52, 255, 0, 0));
        private static Brush _normalColor;

        public AuthWindow()
        {
            InitializeComponent();
            _normalColor = UrlTextBox.Background;
        }

        private void CheckButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(CheckConfig);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            Configuration configuration = new Configuration(GetUrl(), TokenTextBox.Text);
            Storage.Save(configuration);
            new MainWindow(configuration, Storage.RestoreViewConfiguration()).Show();
            Close();
        }

        private async Task CheckConfig()
        {
            string token = TokenTextBox.Text;
            var output = false;
            var received = 0;
            Exception exception = null;
            try
            {
                CheckButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                WebsocketClient client = new WebsocketClient(new Uri(GetUrl()));
                var authMessage = $"{{\"type\": \"auth\",\"access_token\": \"{token}\"}}";
                client.Start();
                client.MessageReceived.Subscribe(msg =>
                {
                    var type = (string) JObject.Parse(msg.Text)["type"];
                    if (type == "auth_required")
                    {
                        client.Send(authMessage);
                        return;
                    }

                    output = type == "auth_ok";
                    received = -10;
                    client.Stop(WebSocketCloseStatus.NormalClosure, "");
                    client.Dispose();
                });
                client.Send(authMessage);
                while (received < 10 && received >= 0)
                {
                    received++;
                    await Task.Delay(50);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (exception != null)
            {
                ConsoleWriter.WriteLine($"Error while connecting: {exception.Message}", ConsoleColor.Red);
            }
            else if (!output && received == 10)
            {
                ConsoleWriter.WriteLine("Unable to connect", ConsoleColor.Red);
            }
            else
            {
                ConsoleWriter.WriteLine("Invalid credentials", ConsoleColor.Red);
            }

            SaveButton.IsEnabled = output;
            UrlTextBox.Background = output ? _normalColor : InvalidColor;
            TokenTextBox.Background = output ? _normalColor : InvalidColor;
            CheckButton.IsEnabled = true;
        }

        private void DataChanged(object sender, TextChangedEventArgs e)
        {
            UrlTextBox.Background = _normalColor;
            TokenTextBox.Background = _normalColor;
            SaveButton.IsEnabled = false;
        }

        private string GetUrl()
        {
            return ((UrlTextBox?.Text ?? "")
                    .Replace("https://", "wss://")
                    .Replace("http://", "ws://")
                    + "/api/websocket").Replace("//api", "/api");
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