using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Fan : Entity
    {
        public const string DomainName = "fan";
        private static readonly List<string> OffStatesList = new List<string> {States.Off, States.Unavailable};

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
            var features = GetSupportedFeatures();
            if (features.Count == 0)
            {
                root.Click += (sender, args) => { HaClientContext.CallService(dispatcher, this, "toggle"); };
            }
            else
            {
                root.Items.Add(CreateMenuItem(dispatcher, "turn_on", "Turn On"));
                root.Items.Add(CreateMenuItem(dispatcher, "turn_off", "Turn Off"));
                if (features.Contains(SupportedFeatures.SetSpeed))
                {
                    var currentSpeed = GetAttribute("speed");
                    var speedRootItem = new MenuItem {Header = "Set Speed", StaysOpenOnClick = true };
                    GetListAttribute("speed_list")
                        .ForEach(speedValue =>
                            speedRootItem.Items.Add(
                                CreateMenuItem(dispatcher, "set_speed", speedValue, speedValue == currentSpeed,
                                    data: Tuple.Create<string, object>("speed", speedValue))));
                    root.Items.Add(speedRootItem);
                }

                if (features.Contains(SupportedFeatures.Oscillate))
                {
                    var oscillating = GetBoolAttribute("oscillating");
                    root.Items.Add(CreateMenuItem(dispatcher, "oscillate", "Oscillating", oscillating,
                        data: Tuple.Create<string, object>("oscillating", !oscillating)));
                }

                if (features.Contains(SupportedFeatures.Direction))
                {
                    var currentDirection = GetAttribute("speed");
                    var directionsItem = new MenuItem {Header = "Set Direction", StaysOpenOnClick = true };
                    new List<Tuple<string, string>>
                            {Tuple.Create("forward", "Forward"), Tuple.Create("reverse", "Reverse")}
                        .ForEach(t =>
                            directionsItem.Items.Add(
                                CreateMenuItem(dispatcher, "set_direction", t.Item2, t.Item1 == currentDirection,
                                    data: Tuple.Create<string, object>("direction", t.Item1))));
                    root.Items.Add(directionsItem);
                }
            }

            return root;
        }


        private static class SupportedFeatures
        {
            public const int SetSpeed = 1;
            public const int Oscillate = 2;
            public const int Direction = 4;

            public static List<int> All = new List<int>
            {
                SetSpeed, Oscillate, Direction
            };
        }
    }
}