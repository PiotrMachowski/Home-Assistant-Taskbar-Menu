using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Connection;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Light : Entity
    {
        public const string DomainName = "light";
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

        protected override Dictionary<int, (string service, string header)> FeatureToServiceMap()
        {
            return SupportedFeatures.ServiceMap;
        }

        protected override MenuItem ToMenuItem(Dispatcher dispatcher, string name)
        {
            var root = new MenuItem
            {
                Header = GetName(name),
                StaysOpenOnClick = true,
                IsEnabled = IsAvailable()
            };
            if (IsOn())
            {
                root.Icon = new PackIcon {Kind = PackIconKind.Tick};
            }

            if (GetSupportedFeatures().Count == 0)
            {
                root.Click += (sender, args) => HaClientContext.CallService(dispatcher, this, "toggle");
            }
            else
            {
                root.PreviewMouseDown += (sender, args) =>
                {
                    if (args.ChangedButton == MouseButton.Right)
                    {
                        args.Handled = ToggleIfPossible(dispatcher);
                    }
                };
                root.Items.Add(CreateMenuItem(dispatcher, "turn_on", "Turn On"));
                root.Items.Add(CreateMenuItem(dispatcher, "turn_off", "Turn Off"));
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.Brightness, 0, 255,
                    GetIntAttribute("brightness"), "brightness");
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.ColorTemp, GetIntAttribute("min_mireds"),
                    GetIntAttribute("max_mireds"), GetIntAttribute("color_temp", GetIntAttribute("min_mireds")),
                    "color_temp");
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.WhiteValue, 0, 255,
                    GetIntAttribute("white_value"), "white_value");
            }

            return root;
        }

        public override bool ToggleIfPossible(Dispatcher dispatcher)
        {
            HaClientContext.CallService(dispatcher, this, "toggle");
            return true;
        }

        private static class SupportedFeatures
        {
            public const int Brightness = 1;
            public const int ColorTemp = 2;
            public const int SupportEffect = 4;
            public const int SupportFlash = 8;
            public const int SupportColor = 16;
            public const int SupportTransition = 32;
            public const int WhiteValue = 128;

            public static List<int> All = new List<int>
            {
                Brightness, ColorTemp, WhiteValue
            };

            public static Dictionary<int, (string service, string header)> ServiceMap =
                new Dictionary<int, (string service, string header)>
                {
                    {Brightness, (service: "turn_on", header: "Brightness")},
                    {ColorTemp, (service: "turn_on", header: "Color Temperature")},
                    {WhiteValue, (service: "turn_on", header: "White Value")}
                };
        }
    }
}