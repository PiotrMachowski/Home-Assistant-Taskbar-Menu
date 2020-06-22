using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Climate : Domain
    {
        public const string Name = "climate";

        private static readonly List<string> OffStates = new List<string> {States.Off, States.Unavailable};

        public static Func<string, bool> IsOn = s => !OffStates.Contains(s);

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var isOn = IsOn(stateObject.State);
            var menuItem = new MenuItem
            {
                Header = GetName(stateObject, name),
                IsChecked = isOn,
                ToolTip = stateObject.EntityId
            };
            menuItem.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, isOn ? "turn_off" : "turn_on", stateObject);
            };
            return menuItem;
        }

        private static class SupportedFeatures
        {
            public const int TargetTemperature = 1;
            public const int TargetTemperatureRange = 2;
            public const int TargetHumidity = 4;
            public const int FanMode = 8;
            public const int PresetMode = 16;
            public const int SwingMode = 32;
            public const int AuxHeat = 64;

            public static List<int> All = new List<int>
            {
                TargetTemperature, TargetTemperatureRange, TargetHumidity, FanMode, PresetMode, SwingMode, AuxHeat
            };

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }
        }
    }
}