using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Vacuum : Entity
    {
        public const string DomainName = "vacuum";

        private static readonly List<string> OffStatesList = new List<string> {States.Docked, States.Unavailable};

        public override string Domain()
        {
            return DomainName;
        }

        protected override List<string> OffStates()
        {
            return OffStatesList;
        }

        protected override List<int> AllSupportedFeatures()
        {
            return SupportedFeatures.All;
        }

        protected override Dictionary<int, (string service, string header)> FeatureToServiceMap()
        {
            return SupportedFeatures.ServiceMap;
        }

        public override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            var root = new MenuItem
            {
                Header = GetName(name),
                StaysOpenOnClick = true
            };
            if (IsOn())
            {
                root.Icon = new PackIcon { Kind = PackIconKind.Tick };
            }
            GetSupportedFeatures()
                .ForEach(supportedFeature => AddMenuItemIfSupported(dispatcher, root, supportedFeature));
            if (IsSupported(SupportedFeatures.FanSpeed))
            {
                var fanSpeedItem = new MenuItem {Header = "Fan Speed", StaysOpenOnClick = true};
                var currentFanSpeed = GetAttribute("fan_speed");
                GetListAttribute("fan_speed_list").ForEach(fanSpeed =>
                {
                    fanSpeedItem.Items.Add(CreateMenuItem(dispatcher, "set_fan_speed", fanSpeed, fanSpeed == currentFanSpeed,
                        data: Tuple.Create<string, object>("fan_speed", fanSpeed)));
                });
                root.Items.Add(fanSpeedItem);
            }

            return root;
        }

        private static class SupportedFeatures
        {
            private const int TurnOn = 1;
            private const int TurnOff = 2;
            private const int Pause = 4;
            private const int Stop = 8;
            private const int ReturnHome = 16;
            public const int FanSpeed = 32;
            private const int Battery = 64;
            private const int Status = 128;
            private const int SendCommand = 256;
            private const int Locate = 512;
            private const int CleanSpot = 1024;
            private const int Map = 2048;
            private const int State = 4096;
            private const int Start = 8192;

            public static readonly List<int> All = new List<int>
            {
                TurnOn, TurnOff, Start, Pause, Stop, ReturnHome, FanSpeed,
                Battery, Status, SendCommand, Locate, CleanSpot, Map,
                State
            };

            public static readonly Dictionary<int, (string service, string header)> ServiceMap =
                new Dictionary<int, (string service, string header)>
                {
                    {TurnOn, (service: "turn_on", header: "Turn On")},
                    {TurnOff, (service: "turn_off", header: "Turn Off")},
                    {Pause, (service: "pause", header: "Pause")},
                    {Stop, (service: "stop", header: "Stop")},
                    {ReturnHome, (service: "return_to_base", header: "Return To Base")},
                    {Locate, (service: "locate", header: "Locate")},
                    {CleanSpot, (service: "clean_spot", header: "Clean Spot")},
                    {Start, (service: "start", header: "Start")}
                };
        }
    }
}