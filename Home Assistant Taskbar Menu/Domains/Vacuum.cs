using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Vacuum : Domain
    {
        public const string Name = "vacuum";

        private static readonly List<string> OffStates = new List<string>
            {States.Off, States.Unavailable, States.Docked};

        public static Func<string, bool> IsOn = s => !OffStates.Contains(s);

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var root = new MenuItem
            {
                Header = GetName(stateObject, name),
                IsChecked = IsOn(stateObject.State),
                ToolTip = stateObject.EntityId
            };

            var supportedFeatures = stateObject.GetAttributeValue<long>("supported_features");
            var features = SupportedFeatures.Decode((int) supportedFeatures);
            var services = features.Where(SupportedFeatures.ToServiceMap.ContainsKey)
                .Select(f => SupportedFeatures.ToServiceMap[f]).ToList();
            foreach (var (service, serviceLabel) in services)
            {
                var serviceItem = new MenuItem {Header = serviceLabel};
                serviceItem.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, service, stateObject);
                };
                root.Items.Add(serviceItem);
            }

            return root;
        }


        private static class SupportedFeatures
        {
            public const int TurnOn = 1;
            public const int TurnOff = 2;
            public const int Pause = 4;
            public const int Stop = 8;
            public const int ReturnHome = 16;
            public const int FanSpeed = 32;
            public const int Battery = 64;
            public const int Status = 128;
            public const int SendCommand = 256;
            public const int Locate = 512;
            public const int CleanSpot = 1024;
            public const int Map = 2048;
            public const int State = 4096;
            public const int Start = 8192;

            public static List<int> All = new List<int>
            {
                TurnOn, TurnOff, Pause, Stop, ReturnHome, FanSpeed,
                Battery, Status, SendCommand, Locate, CleanSpot, Map,
                State, Start
            };

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }

            public static readonly Dictionary<int, Tuple<string, string>> ToServiceMap = new Dictionary<int, Tuple<string, string>>
            {
                {TurnOn, Tuple.Create("turn_on", "Turn On")},
                {TurnOff, Tuple.Create("turn_off", "Turn Off")},
                {Pause, Tuple.Create("pause", "Pause")},
                {Stop, Tuple.Create("stop", "Stop")},
                {ReturnHome, Tuple.Create("return_to_base", "Return To Base")},
                {Locate, Tuple.Create("locate", "Locate")},
                {CleanSpot, Tuple.Create("clean_spot", "Clean Spot")},
                {Start, Tuple.Create("start", "Start")}
            };
        }
    }
}