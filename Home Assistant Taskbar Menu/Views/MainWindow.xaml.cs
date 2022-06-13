using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Home_Assistant_Taskbar_Menu.Connection;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;
using Home_Assistant_Taskbar_Menu.Views;
using MaterialDesignThemes.Wpf;
using Icon = System.Drawing.Icon;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BrowserWindow _browserWindow;
        private AboutWindow _aboutWindow;
        private SearchWindow _searchWindow;
        private ViewConfigurationWindow _viewConfigurationWindow;

        private ViewConfiguration _viewConfiguration;
        private readonly List<FrameworkElement> _defaultMenuItems;
        private readonly List<Entity> _stateObjects;

        public ObservableCollection<UIElement> Menu { get; set; }

        #region Hotkeys
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint KEY_H = 0x48;
        private const uint KEY_E = 0x45;

        private IntPtr _windowHandle;
        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_WIN | MOD_SHIFT, KEY_H); // WIN + SHIFT + H
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_WIN | MOD_SHIFT, KEY_E); // WIN + SHIFT + E
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == KEY_H)
                            {
                                ToggleBrowser(null, null);
                            } else if (vkey == KEY_E) {
                                UIElement_OnKeyDown(null, null);
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
        #endregion

        public MainWindow(Configuration configuration, ViewConfiguration viewConfiguration)
        {
            var latestVersion = ResourceProvider.LatestVersion();
            _browserWindow = new BrowserWindow(configuration);
            _viewConfiguration = viewConfiguration;
            _stateObjects = new List<Entity>();
            Menu = new ObservableCollection<UIElement>();
            InitializeComponent();
            _defaultMenuItems = CreateDefaultMenuItems(configuration, latestVersion);
            TaskbarMenuRoot.ItemsSource = Menu;
            Task.Run(() => { InitConnection(configuration).Wait(); });
        }

        private List<FrameworkElement> CreateDefaultMenuItems(Configuration configuration,
            (string version, string url) latestVersion)
        {
            var showUpdate = !ResourceProvider.IsUpToDate(latestVersion);
            Grid grid = new Grid() {MinWidth = 220};
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            if (showUpdate)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
                ShowNotification("Home Assistant Taskbar Menu", "New version of application is available");
            }

            CreateMenuIcon(grid, PackIconKind.Settings, "Edit Application Settings", () =>
            {
                _viewConfigurationWindow?.Close();
                _viewConfigurationWindow = new ViewConfigurationWindow(_stateObjects, _viewConfiguration);
                var response = _viewConfigurationWindow.ShowDialog();
                if (response == true)
                {
                    _viewConfiguration = _viewConfigurationWindow.ViewConfiguration;
                    UpdateTree();
                }
            });
            CreateMenuIcon(grid, PackIconKind.HomeAssistant, "Open Home Assistant",
                () => ToggleBrowser(null, null));
            CreateMenuIcon(grid, PackIconKind.OpenInBrowser, "Open Home Assistant in Browser",
                () => Process.Start(configuration.HttpUrl()));
            CreateMenuIcon(grid, PackIconKind.About, "About HA Taskbar Menu", () =>
            {
                _aboutWindow?.Close();
                _aboutWindow = new AboutWindow();
                _aboutWindow.ShowDialog();
            });
            if (showUpdate)
            {
                CreateMenuIcon(grid, PackIconKind.Update, "Update HA Taskbar Menu",
                    () => Process.Start(latestVersion.url));
            }

            CreateMenuIcon(grid, PackIconKind.Close, "Exit", () => Application.Current.Shutdown());
            return new List<FrameworkElement> {new Separator(), grid};
        }

        private static void CreateMenuIcon(Grid grid, PackIconKind kind, string tooltip, Action clickAction)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            PackIcon icon = new PackIcon
            {
                Kind = kind,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = double.NaN,
                Height = double.NaN,
                Margin = new Thickness(3),
                Foreground = new SolidColorBrush(theme.ToolTipBackground)
            };
            Button button = new Button()
            {
                Width = double.NaN,
                Height = double.NaN,
                Content = icon,
                ToolTip = tooltip,
                Padding = new Thickness(0),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent
            };
            button.PreviewMouseDown += (sender, args) => { clickAction.Invoke(); };
            Grid.SetColumn(button, grid.Children.Count * 2);
            grid.Children.Add(button);
        }


        private void HandleNewEntitiesList(List<Entity> entitiesList)
        {
            UpdateMyStateObjects(entitiesList);
            ConsoleWriter.WriteLine($"RECEIVED SUPPORTED STATES: {entitiesList.Count}", ConsoleColor.Green);
            entitiesList.ForEach(c => { ConsoleWriter.WriteLine($"   {c.EntityId}: {c.State}", ConsoleColor.Gray); });
            Dispatcher.Invoke(() => UpdateTree());
        }

        private async Task InitConnection(Configuration configuration)
        {
            HaClientContext.Initialize(configuration);
            await HaClientContext.Start();
            HaClientContext.AddStateChangeListener(this, UpdateState);
            HaClientContext.AddEntitiesListListener(HandleNewEntitiesList);
            HaClientContext.AddAuthenticationStateListener(auth => Dispatcher.Invoke(() => UpdateTree(auth)));
            HaClientContext.AddNotificationListener(HandleNotification);
        }

        private List<Control> CreateStructure(List<Entity> stateObjects, ViewConfiguration viewConfiguration)
        {
            return viewConfiguration.Children.Count == 0
                ? stateObjects.Where(e => e.Domain() != AutomationEntity.DomainName && e.Domain() != ScriptEntity.DomainName)
                    .Select(e => (Control) e.ToMenuItemSafe(Dispatcher, null))
                    .Take(100)
                    .ToList()
                : viewConfiguration.Children.Select(c => MapToControl(stateObjects, c)).ToList();
        }

        private Control MapToControl(List<Entity> stateObjects, ViewConfiguration viewConfiguration)
        {
            switch (viewConfiguration.NodeType)
            {
                case ViewConfiguration.Type.Separator:
                    return new Separator();
                case ViewConfiguration.Type.Entity:
                    var stateObject = stateObjects.Find(e => e.EntityId.Equals(viewConfiguration.EntityId));
                    return stateObject == null
                        ? new MenuItem {Header = viewConfiguration.EntityId}
                        : stateObject.ToMenuItemSafe(Dispatcher, viewConfiguration.Name);
                case ViewConfiguration.Type.Folder:
                    var node = new MenuItem
                    {
                        Header = viewConfiguration.Name
                    };
                    viewConfiguration.Children.Select(c => MapToControl(stateObjects, c)).ToList()
                        .ForEach(c => node.Items.Add(c));
                    return node;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateMyStateObjects(List<Entity> state)
        {
            _stateObjects.Clear();
            _stateObjects.AddRange(state);
        }

        private void UpdateState(Entity changedState)
        {
            ConsoleWriter.WriteLine($"STATE UPDATED: {changedState.EntityId} => {changedState.State}",
                ConsoleColor.Green);
            if (_viewConfiguration.ContainsEntity(changedState) ||
                _viewConfiguration.Children.Count == 0)
            {
                var ind = _stateObjects.FindIndex(s => s.EntityId == changedState.EntityId);
                if (ind >= 0)
                {
                    _stateObjects[ind] = changedState;
                }

                Dispatcher.Invoke(() => UpdateTree());
            }
        }

        private void UpdateTree(bool authenticated = true)
        {
            Menu.Clear();
            if (authenticated)
            {
                CreateStructure(_stateObjects, _viewConfiguration).ForEach(Menu.Add);
            }
            else
            {
                MenuItem reconnect = new MenuItem {Header = "Reconnect", IsEnabled = true};
                reconnect.Click += (sender, args) => { HaClientContext.Recreate(); };
                Menu.Add(reconnect);
            }

            _defaultMenuItems.ForEach(Menu.Add);
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            _searchWindow?.Close();
            _searchWindow = new SearchWindow(e, _stateObjects);
            _searchWindow.ShowDialog();
        }

        private void HandleNotification(NotificationEvent notification)
        {
            ConsoleWriter.WriteLine($"NOTIFICATION RECEIVED: {notification.Id}", ConsoleColor.Green);
            if (_viewConfiguration.GetProperty(ViewConfiguration.MirrorNotificationsKey) == true.ToString())
            {
                ShowNotification(notification.Title, notification.Message);
            }
        }

        private void ShowNotification(string title, string message)
        {
            TaskbarIcon.ShowBalloonTip(title, message, GetIcon(), true);
        }

        private Icon GetIcon()
        {
            using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Images/small.ico"))
                ?.Stream)
            {
                return new Icon(iconStream);
            }
        }

        private void ToggleBrowser(object sender, RoutedEventArgs e)
        {
            if(_browserWindow.Visibility != Visibility.Visible)
        {
            _browserWindow.Show();
            _browserWindow.Activate();
                Debug.WriteLine("Showing browser window.");
            }
            else
            {
                _browserWindow.Hide();
                Debug.WriteLine("Hiding browser window.");
            }

        }
    }
}