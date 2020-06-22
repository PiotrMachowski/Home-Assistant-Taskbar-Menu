using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Automation : Domain
    {
        public const string Name = "automation";

        private static readonly List<string> OffStates = new List<string> {States.Off, States.Unavailable};

        public static Func<string, bool> IsOn = s => !OffStates.Contains(s);

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var root = new MenuItem
            {
                Header = GetName(stateObject, name),
                IsChecked = IsOn(stateObject.State),
                ToolTip = stateObject.EntityId
            };
            var services = new List<Tuple<string, string>>
            {
                Tuple.Create("turn_on", "Turn On"),
                Tuple.Create("turn_off", "Turn Off"),
                Tuple.Create("trigger", "Trigger"),
            };
            foreach (var (service, serviceLabel) in services)
            {
                var optionItem = new MenuItem {Header = serviceLabel};
                optionItem.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, service, stateObject);
                };
                root.Items.Add(optionItem);
            }

            return root;
        }
    }
}