using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Cover : Domain
    {
        public const string Name = "cover";

        private static readonly List<string> OffStates = new List<string> {States.Closed, States.Unavailable};

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
            if (new HashSet<int>
                {SupportedFeatures.Open, SupportedFeatures.Close}.SetEquals(features))
            {
                root.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "toggle", stateObject);
                };
            }
            else if (new HashSet<int>
                {SupportedFeatures.OpenTilt, SupportedFeatures.CloseTilt}.SetEquals(features))
            {
                root.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "toggle_cover_tilt", stateObject);
                };
            }
            else
            {
                if (features.Contains(SupportedFeatures.Open))
                {
                    var serviceItem = new MenuItem {Header = "Open"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "open_cover", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.Close))
                {
                    var serviceItem = new MenuItem {Header = "Close"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "close_cover", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.Stop))
                {
                    var serviceItem = new MenuItem {Header = "Stop"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "stop_cover", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.SetPosition))
                {
                    var position = double.Parse(stateObject.GetAttributeValue<object>("current_position")?.ToString() ?? "0");
                    var slider = new Slider
                        {Minimum = 0, Maximum = 100, MinWidth = 100, ToolTip = "Position", Value = position};
                    slider.PreviewMouseUp += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "set_cover_position", stateObject,
                            Tuple.Create<string, object>("position", (int) slider.Value));
                    };
                    root.Items.Add(slider);
                }

                if (features.Contains(SupportedFeatures.OpenTilt))
                {
                    var serviceItem = new MenuItem {Header = "Open Tilt"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "open_cover_tilt", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.CloseTilt))
                {
                    var serviceItem = new MenuItem {Header = "Close Tilt"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "close_cover_tilt", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.StopTilt))
                {
                    var serviceItem = new MenuItem {Header = "Stop Tilt"};
                    serviceItem.Click += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "stop_cover_tilt", stateObject);
                    };
                    root.Items.Add(serviceItem);
                }

                if (features.Contains(SupportedFeatures.SetTiltPosition))
                {
                    var position =
                        int.Parse(stateObject.GetAttributeValue<object>("current_tilt_position")?.ToString() ?? "0");
                    var slider = new Slider
                        {Minimum = 0, Maximum = 100, MinWidth = 100, ToolTip = "Tilt Position", Value = position};
                    slider.PreviewMouseUp += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "set_cover_tilt_position", stateObject,
                            Tuple.Create<string, object>("tilt_position", (int) slider.Value));
                    };
                    root.Items.Add(slider);
                }
            }

            return root;
        }


        private static class SupportedFeatures
        {
            public const int Open = 1;
            public const int Close = 2;
            public const int SetPosition = 4;
            public const int Stop = 8;
            public const int OpenTilt = 16;
            public const int CloseTilt = 32;
            public const int StopTilt = 64;
            public const int SetTiltPosition = 128;

            public static List<int> All = new List<int>
            {
                Open, Close, SetPosition, Stop, OpenTilt, CloseTilt, StopTilt, SetTiltPosition
            };

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }
        }
    }
}