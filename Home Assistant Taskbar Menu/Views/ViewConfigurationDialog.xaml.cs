using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu
{
    /// <summary>
    /// Interaction logic for ViewConfigurationDialog.xaml
    /// </summary>
    public partial class ViewConfigurationDialog : Window
    {
        private readonly bool _isEntity;

        public ViewConfiguration ViewConfiguration { get; set; }

        public ViewConfigurationDialog(List<Entity> stateObjects)
        {
            InitializeComponent();
            stateObjects.ForEach(s => EntityIdComboBox.Items.Add(s));
            NameTextBox.ToolTip = "Leave empty to use name retrieved from Home Assistant";
            _isEntity = true;
        }

        public ViewConfigurationDialog()
        {
            InitializeComponent();
            RowEntityId1.Height = new GridLength(0);
            RowEntityId2.Height = new GridLength(0);
            Height = 130;
            _isEntity = false;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            ViewConfiguration = _isEntity
                ? ViewConfiguration.Entity(((Entity) EntityIdComboBox.SelectedItem).EntityId,
                    string.IsNullOrEmpty(NameTextBox.Text) ? null : NameTextBox.Text)
                : ViewConfiguration.Folder(NameTextBox.Text);
            DialogResult = true;
        }

        private void ValidateData(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = !(EntityIdComboBox.SelectedIndex < 0 && _isEntity ||
                                     string.IsNullOrEmpty(NameTextBox.Text) && !_isEntity);
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