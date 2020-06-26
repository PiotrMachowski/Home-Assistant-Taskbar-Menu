using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Home_Assistant_Taskbar_Menu.Entities;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Views
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private readonly List<Entity> _entities;

        public SearchWindow(string s, List<Entity> entities)
        {
            _entities = new List<Entity>(entities);
            InitializeComponent();
            HaClientContext.AddStateChangeListener(this, updatedEntity =>
            {
                var index = _entities.FindIndex(e => e.EntityId == updatedEntity.EntityId);
                if (index >= 0)
                {
                    _entities[index] = updatedEntity;
                    Dispatcher.Invoke(() => UpdateFoundList(null, null));
                }
            });
            if (s.Length == 1)
            {
                SearchBox.Text = s;
            }

            SearchBox.CaretIndex = int.MaxValue;
            SearchBox.Focus();
        }

        private void UpdateFoundList(object sender, TextChangedEventArgs ee)
        {
            var text = SearchBox.Text.ToLower();
            FoundList.Items.Clear();
            var list = _entities.ToList().Where(e => EntityMatches(e, text)).ToList();
            for (var i = 0; i < list.Count && i < 10; i++)
            {
                var entity = list[i];
                var cm = Convert(entity);
                FoundList.Items.Add(cm);
            }
        }

        private static bool EntityMatches(Entity e, string text)
        {
            return e.EntityId.ToLower().Contains(text) || e.GetName().ToLower().Contains(text);
        }

        private ListBoxItem Convert(Entity entity)
        {
            MenuItem item = entity.ToMenuItemSafe(Dispatcher, null);
            item.Visibility = Visibility.Hidden;
            PackIcon icon = new PackIcon()
            {
                Kind = PackIconKind.Tick,
                Height = double.NaN,
                Width = double.NaN,
                Visibility = entity.IsOn() ? Visibility.Visible : Visibility.Hidden,
                Padding = new Thickness(5)
            };
            Grid.SetColumn(icon, 0);
            Label label = new Label
            {
                Content = entity.ToString().Replace("_", "__"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 1);
            Grid grid = new Grid
            {
                Width = double.NaN,
                IsEnabled = entity.IsAvailable(),
                ContextMenu = new ContextMenu()
            };
            grid.Children.Add(icon);
            grid.Children.Add(label);
            grid.Children.Add(item);
            grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(30)});
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.MouseDown += (sender, args) =>
            {
                if (grid.ContextMenu.Items.Count == 0)
                    item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                else
                    grid.ContextMenu.IsOpen = true;
                args.Handled = true;
            };
            List<object> os = item.Items.Cast<object>().ToList();
            os.ForEach(o =>
            {
                item.Items.Remove(o);
                grid.ContextMenu.Items.Add(o);
            });
            return new ListBoxItem
            {
                Padding = new Thickness(0),
                Content = grid,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            HaClientContext.RemoveStateChangeListener(this);
            Close();
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void EscapeListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HaClientContext.RemoveStateChangeListener(this);
                Close();
            }
        }
    }
}