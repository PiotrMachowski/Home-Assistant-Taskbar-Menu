using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CefSharp;
using CefSharp.Wpf;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu.Views
{
    /// <summary>
    /// Interaction logic for BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {

        private IntPtr _windowHandle;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).Handle;
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        private System.Timers.Timer focusLossTimer = new System.Timers.Timer();


        private string Url { get; }

        public BrowserWindow(Configuration configuration)
        {
            CefSettings cefSharpSettings = new CefSettings {LogSeverity = LogSeverity.Disable};
            Cef.Initialize(cefSharpSettings);
            ShowActivated = true;
            Url = configuration.HttpUrl();
            InitializeComponent();
            Browser.Address = Url;

            var requestContextSettings = new RequestContextSettings
                {CachePath = Storage.BrowserCachePath};
            Browser.RequestContext = new RequestContext(requestContextSettings);
            var position = Storage.RestorePosition();
            if (position.HasValue)
            {
                Left = position.Value.x;
                Top = position.Value.y;
                Width = position.Value.width;
                Height = position.Value.height;
            }

            
            focusLossTimer.Elapsed += (s, e) => {
            if(GetForegroundWindow() != _windowHandle && Visibility == Visibility.Visible)
                {
                    Debug.WriteLine("Hiding browser window due to focus loss");
                    Application.Current.Dispatcher.Invoke(new Action(() => { Hide(); }));
                }
            }; 
            focusLossTimer.Interval = 500;
            focusLossTimer.Enabled = true;           
            
        }

        private void MinimizeButton(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Hide();
            focusLossTimer.Stop();
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BrowserButton(object sender, RoutedEventArgs e)
        {
            Process.Start(Browser.Address);
        }

        private void RefreshButton(object sender, RoutedEventArgs e)
        {
            Browser.WebBrowser.Reload();
        }

        private void BrowserWindow_OnClosed(object sender, EventArgs e)
        {
            Storage.SavePosition((Left, Top, Width, Height));
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            focusLossTimer.Start();
        }
    }
}