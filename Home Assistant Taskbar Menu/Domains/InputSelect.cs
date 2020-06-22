using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class InputSelect : Domain
    {
        public const string Name = "input_select";

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var root = new MenuItem
            {
                Header = GetName(stateObject, name),
                ToolTip = stateObject.EntityId
            };
            var options = (stateObject.GetAttributeValue<JArray>("options") ?? new JArray()).Select(i => (string) i).ToList();
            foreach (var option in options)
            {
                var optionItem = new MenuItem {Header = option, IsChecked = stateObject.State == option};
                optionItem.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "select_option", stateObject,
                            Tuple.Create<string, object>("option", option));
                };
                root.Items.Add(optionItem);
            }

            return root;
        }
    }
}