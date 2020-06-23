using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Home_Assistant_Taskbar_Menu.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            NameAndVersionLabel.Content =
                $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}";
            CopyrightInfoLabel.Content = Assembly.GetExecutingAssembly().CustomAttributes
                .Where(ca => ca.AttributeType?.FullName == "System.Reflection.AssemblyCopyrightAttribute")
                .Select(ca => ca.ConstructorArguments[0]).First().Value.ToString();
            RepoHyperlink.NavigateUri = new Uri(Assembly.GetExecutingAssembly().CustomAttributes
                .Where(ca => ca.AttributeType?.FullName == "System.Reflection.AssemblyDescriptionAttribute")
                .Select(ca => ca.ConstructorArguments[0]).First().Value.ToString());
        }

        private void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}