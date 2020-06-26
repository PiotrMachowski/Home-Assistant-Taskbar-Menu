using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Home_Assistant_Taskbar_Menu.Connection;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;
using Home_Assistant_Taskbar_Menu.Views;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AboutWindow _aboutWindow;
        private SearchWindow _searchWindow;
        private ViewConfigurationWindow _viewConfigurationWindow;

        private ViewConfiguration _viewConfiguration;
        private readonly List<Control> _defaultMenuItems;
        private readonly List<Entity> _stateObjects;

        public ObservableCollection<UIElement> Menu { get; set; }

        public MainWindow(Configuration configuration, ViewConfiguration viewConfiguration)
        {
            var latestVersion = ResourceProvider.LatestVersion();
            _viewConfiguration = viewConfiguration;
            _defaultMenuItems = CreateDefaultMenuItems(configuration.Url, latestVersion);
            Menu = new ObservableCollection<UIElement>();
            _stateObjects = new List<Entity>();
            InitializeComponent();
            TaskbarMenuRoot.ItemsSource = Menu;
            Task.Run(() => { InitConnection(configuration).Wait(); });
        }

        private List<Control> CreateDefaultMenuItems(string configurationUrl,
            (string version, string url) latestVersion)
        {
            var url = configurationUrl.Replace("wss://", "https://")
                .Replace("ws://", "http://")
                .Replace("/api/websocket", "");
            var editView = new MenuItem {Header = "Edit view configuration"};
            editView.Click += (sender, args) =>
            {
                _viewConfigurationWindow?.Close();
                _viewConfigurationWindow = new ViewConfigurationWindow(_stateObjects, _viewConfiguration);
                var response = _viewConfigurationWindow.ShowDialog();
                if (response == true)
                {
                    _viewConfiguration = _viewConfigurationWindow.ViewConfiguration;
                    UpdateTree();
                }
            };
            var haItem = new MenuItem {Header = "Open HA in Browser"};
            haItem.Click += (sender, args) => { Process.Start(url); };
            var aboutItem = new MenuItem {Header = "About HA Taskbar Menu"};
            aboutItem.Click += (sender, args) =>
            {
                _aboutWindow?.Close();
                _aboutWindow = new AboutWindow();
                _aboutWindow.ShowDialog();
            };

            var updateItem = new MenuItem {Header = "Update HA Taskbar Menu"};
            updateItem.Click += (sender, args) => { Process.Start(latestVersion.url); };

            var exitItem = new MenuItem {Header = "Exit"};
            exitItem.Click += (sender, args) => { Application.Current.Shutdown(); };

            var list = new List<Control> {new Separator(), editView, haItem, aboutItem};
            if (!ResourceProvider.IsUpToDate(latestVersion))
            {
                list.Add(updateItem);
            }

            list.Add(exitItem);
            return list;
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
            HaClientContext.HomeAssistantWebsocketClient = new HomeAssistantWebsocketsClient(configuration);
            await HaClientContext.Start();
            HaClientContext.AddStateChangeListener(this, UpdateState);
            HaClientContext.AddEntitiesListListener(HandleNewEntitiesList);
            HaClientContext.AddAuthenticationStateListener(auth => Dispatcher.Invoke(() => UpdateTree(auth)));
        }

        private List<Control> CreateStructure(List<Entity> stateObjects, ViewConfiguration viewConfiguration)
        {
            return viewConfiguration.Children.Count == 0
                ? stateObjects.Select(e => (Control) e.ToMenuItemSafe(Dispatcher, null)).ToList()
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
                Menu.Add(new MenuItem {Header = "Not connected", IsEnabled = false});
            }

            _defaultMenuItems.ForEach(Menu.Add);
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            _searchWindow?.Close();
            _searchWindow = new SearchWindow(e.Key.ToString(), _stateObjects);
            _searchWindow.ShowDialog();
        }
    }
}