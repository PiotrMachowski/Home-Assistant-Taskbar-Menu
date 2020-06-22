using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Fan : Domain
    {
        public const string Name = "fan";
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
            var supportedFeatures = stateObject.GetAttributeValue<long>("supported_features");
            var features = SupportedFeatures.Decode((int) supportedFeatures);
            if (features.Count == 0)
            {
                root.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "toggle", stateObject);
                };
            }
            else
            {
                if (features.Contains(SupportedFeatures.SetSpeed))
                {
                    var speedList = stateObject.GetAttributeValue<JArray>("speed_list").Select(i => (string) i)
                        .ToList();
                    var currentSpeed = stateObject.GetAttributeValue<string>("speed");
                    var speedRootItem = new MenuItem {Header = "Set Speed"};
                    foreach (var speedValue in speedList)
                    {
                        var optionItem = new MenuItem
                            {Header = speedValue, IsChecked = speedValue == currentSpeed};
                        optionItem.Click += (sender, args) =>
                        {
                            HaClientContext.CallService(dispatcher, Name, "set_speed", stateObject,
                                Tuple.Create<string, object>("speed", speedValue));
                        };
                        speedRootItem.Items.Add(optionItem);
                    }

                    root.Items.Add(speedRootItem);
                }

                if (features.Contains(SupportedFeatures.Oscillate))
                {
                    var oscillating = stateObject.GetAttributeValue<bool>("oscillating");
                    var oscillationItem = new MenuItem
                    {
                        Header = "Oscillating",
                        IsChecked = oscillating
                    };
                    oscillationItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "oscillate", stateObject,
                            Tuple.Create<string, object>("oscillating", !oscillating));
                    };
                    root.Items.Add(oscillationItem);
                }

                if (features.Contains(SupportedFeatures.Direction))
                {
                    var directions = new List<Tuple<string, string>>
                        {Tuple.Create("forward", "Forward"), Tuple.Create("reverse", "Reverse")};
                    var currentDirection = stateObject.GetAttributeValue<string>("speed");
                    var directionsItem = new MenuItem {Header = "Set Direction"};
                    foreach (var (parameter, parameterLabel ) in directions)
                    {
                        var optionItem = new MenuItem
                            {Header = parameterLabel, IsChecked = parameter == currentDirection};
                        optionItem.Click += (sender, args) =>
                        {
                            HaClientContext.CallService(dispatcher, Name, "set_direction", stateObject,
                                Tuple.Create<string, object>("direction", parameter));
                        };
                        directionsItem.Items.Add(optionItem);
                    }

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

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }
        }
    }
}