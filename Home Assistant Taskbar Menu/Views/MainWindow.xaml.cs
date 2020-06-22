using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HADotNet.Core.Models;
using Home_Assistant_Taskbar_Menu.Connection;
using Home_Assistant_Taskbar_Menu.Domains;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewConfiguration _viewConfiguration;
        private readonly List<Control> _defaultMenuItems;
        private readonly List<StateObject> _stateObjects;

        public ObservableCollection<Control> Menu { get; set; }

        public MainWindow(Configuration configuration, ViewConfiguration viewConfiguration)
        {
            _viewConfiguration = viewConfiguration;
            _defaultMenuItems = CreateDefaultMenuItems(configuration.Url);
            Menu = new ObservableCollection<Control>();
            _stateObjects = new List<StateObject>();
            InitializeComponent();
            TaskbarMenuRoot.ItemsSource = Menu;
            check(configuration);
        }

        private List<Control> CreateDefaultMenuItems(string configurationUrl)
        {
            var url = configurationUrl.Replace("wss://", "https://")
                .Replace("ws://", "http://")
                .Replace("/api/websocket", "");
            var editView = new MenuItem {Header = "Edit view configuration"};
            editView.Click += (sender, args) =>
            {
                var viewConfigurationWindow = new ViewConfigurationWindow(_stateObjects, _viewConfiguration);
                var response = viewConfigurationWindow.ShowDialog();
                if (response == true)
                {
                    _viewConfiguration = viewConfigurationWindow.ViewConfiguration;
                    UpdateTree();
                }
            };
            var haItem = new MenuItem {Header = "Open HA in Browser"};
            haItem.Click += (sender, args) => { System.Diagnostics.Process.Start(url); };

            var exitItem = new MenuItem {Header = "Exit"};
            exitItem.Click += (sender, args) => { Close(); };

            return new List<Control>
            {
                new Separator(),
                editView,
                haItem,
                exitItem
            };
        }

        private void check(Configuration configuration)
        {
            Task.Run(() => { CheckIt(configuration).Wait(); });
        }

        private async Task CheckIt(Configuration configuration)
        {
            HomeAssistantWebsocketsClient homeAssistantWebsocketClient =
                new HomeAssistantWebsocketsClient(configuration);
            HaClientContext.HomeAssistantWebsocketClient = homeAssistantWebsocketClient;
            await HaClientContext.Start();
            HaClientContext.AddStateChangeListener(UpdateState);
            await Task.Delay(1000);
            await HaClientContext.GetStates(s =>
            {
                UpdateStateObjects(s);
                Console.WriteLine($"RECEIVED STATES: {s.Count}");
                s.ForEach(c => { Console.WriteLine($"   {c.EntityId}: {c.State}"); });
                Dispatcher.Invoke(UpdateTree);
            });
        }

        private List<Control> CreateStructure(List<StateObject> stateObjects, ViewConfiguration viewConfiguration)
        {
            return viewConfiguration.Children.Count == 0
                ? ConvertToMenuItems(stateObjects).Select(e => (Control) e.ItemsControl).ToList()
                : viewConfiguration.Children.Select(c => MapToControl(stateObjects, c)).ToList();
        }

        private Control MapToControl(List<StateObject> stateObjects, ViewConfiguration viewConfiguration)
        {
            switch (viewConfiguration.NodeType)
            {
                case ViewConfiguration.Type.Separator:
                    return new Separator();
                case ViewConfiguration.Type.Entity:
                    var stateObject = stateObjects.Find(e => e.EntityId.Equals(viewConfiguration.EntityId));
                    return stateObject == null
                        ? new MenuItem {Header = viewConfiguration.EntityId}
                        : ConvertToMenuItems(new List<StateObject> {stateObject}, viewConfiguration.Name)[0]
                            .ItemsControl;
                case ViewConfiguration.Type.Folder:
                    var node = new MenuItem
                    {
                        Header = viewConfiguration.Name,
                    };
                    viewConfiguration.Children.Select(c => MapToControl(stateObjects, c)).ToList()
                        .ForEach(c => node.Items.Add(c));
                    return node;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateStateObjects(List<StateObject> state)
        {
            _stateObjects.Clear();
            _stateObjects.AddRange(state.Where(Domain.IsSupported));
        }

        private List<EntityMenuItem> ConvertToMenuItems(List<StateObject> stateObjects, string name = null)
        {
            List<EntityMenuItem> items = new List<EntityMenuItem>();
            stateObjects.ForEach(s => Domain.ConvertToMenuItem(s, name, items.Add, Dispatcher));
            return items;
        }

        private void UpdateState(StateObject changedState)
        {
            Console.WriteLine($"STATE UPDATED: {changedState.EntityId} => {changedState.State}");
            if (_viewConfiguration.ContainsEntity(changedState) ||
                _viewConfiguration.Children.Count == 0 && Domain.IsSupported(changedState))
            {
                var ind = _stateObjects.FindIndex(s => s.EntityId == changedState.EntityId);
                if (ind >= 0)
                {
                    _stateObjects[ind] = changedState;
                }

                Dispatcher.Invoke(() =>
                {
                    Console.WriteLine("UPDATING");
                    UpdateTree();
                });
            }
        }

        private void UpdateTree()
        {
            Menu.Clear();
            CreateStructure(_stateObjects, _viewConfiguration).ForEach(Menu.Add);
            _defaultMenuItems.ForEach(Menu.Add);
        }
    }

    public class EntityMenuItem
    {
        public StateObject StateObject { get; set; }
        public ItemsControl ItemsControl { get; set; }

        public EntityMenuItem(ItemsControl itemsControl, StateObject stateObject)
        {
            ItemsControl = itemsControl;
            StateObject = stateObject;
        }

        public void Update(EntityMenuItem newValue)
        {
            ItemsControl = newValue.ItemsControl;
            StateObject = newValue.StateObject;
        }
    }
}