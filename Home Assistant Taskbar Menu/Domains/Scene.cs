using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Scene : Domain
    {
        public const string Name = "scene";
        
        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var menuItem = new MenuItem
            {
                Header = GetName(stateObject, name),
                ToolTip = stateObject.EntityId
            };
            menuItem.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "turn_on", stateObject);
            };
            return menuItem;
        }
    }
}