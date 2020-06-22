using System.Collections.Generic;
using System.Windows;
using HADotNet.Core.Models;
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

        public ViewConfigurationDialog(List<StateObject> stateObjects)
        {
            InitializeComponent();
            stateObjects.ForEach(s => EntityIdComboBox.Items.Add(s.EntityId));
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
                ? ViewConfiguration.Entity(EntityIdComboBox.SelectedItem.ToString(), NameTextBox.Text)
                : ViewConfiguration.Folder(NameTextBox.Text);
            DialogResult = true;
        }

        private void ValidateData(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = !(EntityIdComboBox.SelectedIndex < 0 && _isEntity ||
                                     string.IsNullOrEmpty(NameTextBox.Text) && !_isEntity);
        }
    }
}