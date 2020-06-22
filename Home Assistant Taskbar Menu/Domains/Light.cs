using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Light : Domain
    {
        public const string Name = "light";
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
                var onItem = new MenuItem {Header = "Turn On"};
                onItem.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "turn_on", stateObject);
                };
                root.Items.Add(onItem);
                var offItem = new MenuItem {Header = "Turn Off"};
                offItem.Click += (sender, args) =>
                {
                    HaClientContext.CallService(dispatcher, Name, "turn_off", stateObject);
                };
                root.Items.Add(offItem);
                if (features.Contains(SupportedFeatures.Brightness))
                {
                    var brightness = stateObject.GetAttributeValue<long?>("brightness") ?? 0;
                    var slider = new Slider
                    {
                        Minimum = 0, Maximum = 255, MinWidth = 100, ToolTip = "Brightness", Value = brightness
                    };
                    slider.PreviewMouseUp += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "turn_on", stateObject,
                            Tuple.Create<string, object>("brightness", (int) slider.Value));
                    };
                    root.Items.Add(slider);
                }

                if (features.Contains(SupportedFeatures.ColorTemp))
                {
                    var minMireds = stateObject.GetAttributeValue<long>("min_mireds");
                    var maxMireds = stateObject.GetAttributeValue<long>("max_mireds");
                    var colorTemp = stateObject.GetAttributeValue<long?>("color_temp") ?? minMireds;
                    var slider = new Slider
                    {
                        Minimum = minMireds,
                        Maximum = maxMireds,
                        MinWidth = 100,
                        ToolTip = "Color Temperature",
                        Value = colorTemp
                    };
                    slider.PreviewMouseUp += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "turn_on", stateObject,
                            Tuple.Create<string, object>("color_temp", (int) slider.Value));
                    };
                    root.Items.Add(slider);
                }

                if (features.Contains(SupportedFeatures.WhiteValue))
                {
                    var whiteValue = stateObject.GetAttributeValue<long?>("white_value") ?? 0;
                    var slider = new Slider
                    {
                        Minimum = 0,
                        Maximum = 255,
                        MinWidth = 100,
                        ToolTip = "White Value",
                        Value = whiteValue
                    };
                    slider.PreviewMouseUp += (sender, args) =>
                    {
                        HaClientContext.CallService(dispatcher, Name, "turn_on", stateObject,
                                Tuple.Create<string, object>("white_value", (int) slider.Value));
                    };
                    root.Items.Add(slider);
                }
            }

            return root;
        }

        private static class SupportedFeatures
        {
            public const int Brightness = 1;
            public const int ColorTemp = 2;
            public const int WhiteValue = 128;

            public static List<int> All = new List<int>
            {
                Brightness, ColorTemp, WhiteValue
            };

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }
        }
    }
}