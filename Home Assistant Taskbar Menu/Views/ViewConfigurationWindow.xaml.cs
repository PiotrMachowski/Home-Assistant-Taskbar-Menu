﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for ViewConfigurationWindow.xaml
    /// </summary>
    public partial class ViewConfigurationWindow : Window
    {
        private readonly List<Entity> _stateObjects;
        public ViewConfiguration ViewConfiguration { get; set; }

        public ViewConfigurationWindow(List<Entity> stateObjects, ViewConfiguration viewConfiguration)
        {
            _stateObjects = stateObjects;
            InitializeComponent();
            ViewConfiguration = viewConfiguration;
            GenerateTree();
            SizeChanged += (o, args) =>
            {
                MaximizeMinimizeIcon.Kind = WindowState == WindowState.Maximized
                    ? PackIconKind.WindowMaximize
                    : PackIconKind.WindowRestore;
            };
            ChangeThemeButton.Content = $"Theme: {viewConfiguration.GetProperty(ViewConfiguration.ThemeKey)}";
            MirrorNotificationsButton.Content =
                $"Mirror notifications: {viewConfiguration.GetProperty(ViewConfiguration.MirrorNotificationsKey)}";
        }

        private void GenerateTree()
        {
            var addEntityMenuItem = new MenuItem {Header = "Add Entity"};
            var addNodeMenuItem = new MenuItem {Header = "Add Node"};
            var addSeparatorMenuItem = new MenuItem {Header = "Add Separator"};
            TreeView.ContextMenu = new ContextMenu {Items = {addEntityMenuItem, addNodeMenuItem, addSeparatorMenuItem}};
            addEntityMenuItem.Click += AddEntityClick;
            addNodeMenuItem.Click += AddNodeClick;
            addSeparatorMenuItem.Click += AddSeparatorClick;
            ViewConfiguration.Children.ForEach(c => Add(ViewConfiguration, c, TreeView));
        }

        private void AddEntityClick(object sender, RoutedEventArgs e)
        {
            var viewConfigurationDialog = new ViewConfigurationDialog(_stateObjects);
            var completed = viewConfigurationDialog.ShowDialog();
            if (completed == true)
            {
                ViewConfiguration.Children.Add(viewConfigurationDialog.ViewConfiguration);
                Add(ViewConfiguration, viewConfigurationDialog.ViewConfiguration, TreeView);
            }
        }

        private void AddNodeClick(object sender, RoutedEventArgs e)
        {
            var viewConfigurationDialog = new ViewConfigurationDialog();
            var completed = viewConfigurationDialog.ShowDialog();
            if (completed == true)
            {
                ViewConfiguration.Children.Add(viewConfigurationDialog.ViewConfiguration);
                Add(ViewConfiguration, viewConfigurationDialog.ViewConfiguration, TreeView);
            }
        }

        private void AddSeparatorClick(object sender, RoutedEventArgs e)
        {
            var separator = ViewConfiguration.Separator();
            ViewConfiguration.Children.Add(separator);
            Add(ViewConfiguration, separator, TreeView);
        }

        private void Add(ViewConfiguration rootView, ViewConfiguration viewConfiguration, ItemsControl root)
        {
            var deleteMenuItem = new MenuItem {Header = "Delete"};
            switch (viewConfiguration.NodeType)
            {
                case ViewConfiguration.Type.Separator:
                    var separatorRow = new TreeViewItem
                    {
                        Header = "------------------------------------------",
                        ContextMenu = new ContextMenu
                        {
                            Placement = PlacementMode.MousePoint,
                            Items = {deleteMenuItem}
                        }
                    };
                    deleteMenuItem.Click += (sender, args) =>
                    {
                        root.Items.Remove(separatorRow);
                        rootView.Children.Remove(viewConfiguration);
                    };
                    root.Items.Add(separatorRow);
                    break;
                case ViewConfiguration.Type.Entity:
                    var e = _stateObjects.Find(s => s.EntityId == viewConfiguration.EntityId);
                    var header = $"{viewConfiguration.Name} ({viewConfiguration.EntityId})";
                    if (string.IsNullOrEmpty(viewConfiguration.Name))
                    {
                        header = e?.ToString() ?? viewConfiguration.EntityId;
                    }

                    var entityRow = new TreeViewItem
                    {
                        Header = header,
                        ContextMenu = new ContextMenu
                        {
                            Placement = PlacementMode.MousePoint,
                            Items = {deleteMenuItem}
                        }
                    };
                    deleteMenuItem.Click += (sender, args) =>
                    {
                        root.Items.Remove(entityRow);
                        rootView.Children.Remove(viewConfiguration);
                    };
                    root.Items.Add(entityRow);
                    break;
                case ViewConfiguration.Type.Folder:
                    var addEntityMenuItem = new MenuItem {Header = "Add Entity"};
                    var addNodeMenuItem = new MenuItem {Header = "Add Node"};
                    var addSeparatorMenuItem = new MenuItem {Header = "Add Separator"};
                    var nodeRow = new TreeViewItem
                    {
                        Header = $"{viewConfiguration.Name}",
                        ContextMenu = new ContextMenu
                        {
                            Placement = PlacementMode.MousePoint,
                            Items =
                            {
                                addEntityMenuItem, addNodeMenuItem, addSeparatorMenuItem, deleteMenuItem
                            }
                        }
                    };
                    addEntityMenuItem.Click += (sender, args) =>
                    {
                        var viewConfigurationDialog = new ViewConfigurationDialog(_stateObjects);
                        var completed = viewConfigurationDialog
                            .ShowDialog();
                        if (completed == true)
                        {
                            viewConfiguration.Children.Add(viewConfigurationDialog.ViewConfiguration);
                            Add(viewConfiguration, viewConfigurationDialog.ViewConfiguration, nodeRow);
                        }
                    };
                    addNodeMenuItem.Click += (sender, args) =>
                    {
                        var viewConfigurationDialog = new ViewConfigurationDialog();
                        var completed = viewConfigurationDialog.ShowDialog();
                        if (completed == true)
                        {
                            viewConfiguration.Children.Add(viewConfigurationDialog.ViewConfiguration);
                            Add(viewConfiguration, viewConfigurationDialog.ViewConfiguration, nodeRow);
                        }
                    };
                    addSeparatorMenuItem.Click += (sender, args) =>
                    {
                        var separator = ViewConfiguration.Separator();
                        viewConfiguration.Children.Add(separator);
                        Add(viewConfiguration, separator, nodeRow);
                    };
                    deleteMenuItem.Click += (sender, args) =>
                    {
                        root.Items.Remove(nodeRow);
                        rootView.Children.Remove(viewConfiguration);
                    };
                    root.Items.Add(nodeRow);
                    viewConfiguration.Children.ForEach(c => Add(viewConfiguration, c, nodeRow));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            Storage.Save(ViewConfiguration);
            DialogResult = true;
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
            Close();
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ChangeThemeClick(object sender, RoutedEventArgs e)
        {
            ViewConfiguration.Properties[ViewConfiguration.ThemeKey] =
                ViewConfiguration.GetProperty(ViewConfiguration.ThemeKey) == ViewConfiguration.LightTheme
                    ? ViewConfiguration.DarkTheme
                    : ViewConfiguration.LightTheme;
            ChangeThemeButton.Content = $"Theme: {ViewConfiguration.GetProperty(ViewConfiguration.ThemeKey)}";
        }

        private void ChangeMirrorNotificationsClick(object sender, RoutedEventArgs e)
        {
            ViewConfiguration.Properties[ViewConfiguration.MirrorNotificationsKey] =
                ViewConfiguration.GetProperty(ViewConfiguration.MirrorNotificationsKey) == true.ToString()
                    ? false.ToString()
                    : true.ToString();
            MirrorNotificationsButton.Content = $"Mirror notifications: {ViewConfiguration.GetProperty(ViewConfiguration.MirrorNotificationsKey)}";
        }
    }
}