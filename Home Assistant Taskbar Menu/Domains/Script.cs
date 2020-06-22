using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Script : Domain
    {
        public const string Name = "script";
        private static readonly List<string> OffStates = new List<string> { States.Off, States.Unavailable };
        public static Func<string, bool> IsOn = s => !OffStates.Contains(s);

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var menuItem = new MenuItem
            {
                Header = GetName(stateObject, name),
                IsChecked = IsOn(stateObject.State),
                ToolTip = stateObject.EntityId
            };
            menuItem.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "toggle", stateObject);
            };
            return menuItem;
        }
    }
}