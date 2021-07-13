using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

            if (GetSupportedFeatures().Count == 0 ||
                GetSupportedModes().Equals(new HashSet<string> {SupportedColorModes.OnOff}))
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
                    "color_temp",
                    changer: (slider, value) => slider.Foreground = new SolidColorBrush(FromMireds(slider, value)));
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.WhiteValue, 0, 255,
                    GetIntAttribute("white_value"), "white_value");
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.Color, 0, 360,
                    ParseDouble(GetListAttribute("hs_color", 0)), "hs_color", 1,
                    (slider, value) => slider.Foreground = new SolidColorBrush(FromHue(value)),
                    converter: value => new[] {(int) value, 100});
                if (IsSupported(SupportedFeatures.Effect))
                {
                    var effectItem = new MenuItem {Header = "Effect", StaysOpenOnClick = true};
                    var currentEffect = GetAttribute("effect");
                    GetListAttribute("effect_list").ForEach(effect =>
                    {
                        effectItem.Items.Add(CreateMenuItem(dispatcher, "turn_on", effect,
                            effect == currentEffect,
                            data: Tuple.Create<string, object>("effect", effect)));
                    });
                    root.Items.Add(effectItem);
                }
            }


            return root;
        }

        public override bool ToggleIfPossible(Dispatcher dispatcher)
        {
            HaClientContext.CallService(dispatcher, this, "toggle");
            return true;
        }

        protected override List<int> GetSupportedFeatures()
        {
            List<int> supportedFeatures = new List<int>(base.GetSupportedFeatures());
            HashSet<string> supportedModes = GetSupportedModes();
            if (!supportedFeatures.Contains(SupportedFeatures.Brightness) &&
                supportedModes.Intersect(SupportedColorModes.BrightnessModes).Any())
            {
                supportedFeatures.Add(SupportedFeatures.Brightness);
            }

            if (!supportedFeatures.Contains(SupportedFeatures.Color) &&
                supportedModes.Intersect(SupportedColorModes.ColorModes).Any())
            {
                supportedFeatures.Add(SupportedFeatures.Color);
            }

            if (!supportedFeatures.Contains(SupportedFeatures.ColorTemp) &&
                supportedModes.Contains(SupportedColorModes.ColorTemp))
            {
                supportedFeatures.Add(SupportedFeatures.ColorTemp);
            }

            if (!supportedFeatures.Contains(SupportedFeatures.WhiteValue) &&
                supportedModes.Contains(SupportedColorModes.White))
            {
                supportedFeatures.Add(SupportedFeatures.WhiteValue);
            }

            return supportedFeatures;
        }

        private HashSet<string> GetSupportedModes()
        {
            return new HashSet<string>(GetListAttribute("supported_color_modes"));
        }

        private static Color FromHue(double hue)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            byte v = 255;
            byte p = 0;
            byte q = (byte) (255 * (1 - f));
            byte t = (byte) (255 * (1 - (1 - f)));

            switch (hi)
            {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }

        private static Color FromMireds(Slider slider, double mireds)
        {
            double percent = (mireds - slider.Minimum) / (slider.Maximum - slider.Minimum);
            if (percent >= 0.5)
            {
                percent = 2 * (percent - 0.5);
                return Color.Add(Color.Multiply(Colors.White, (float) (1 - percent)),
                    Color.Multiply(Color.FromRgb(255, 160, 0), (float) percent));
            }

            percent = 2 * percent;
            return Color.Add(Color.Multiply(Color.FromRgb(166, 209, 255), (float) (1 - percent)),
                Color.Multiply(Colors.White, (float) percent));
        }

        private static class SupportedFeatures
        {
            public const int Brightness = 1;
            public const int ColorTemp = 2;
            public const int Effect = 4;
            public const int Flash = 8;
            public const int Color = 16;
            public const int Transition = 32;
            public const int WhiteValue = 128;

            public static readonly List<int> All = new List<int>
            {
                Brightness, ColorTemp, WhiteValue, Color, Effect
            };

            public static Dictionary<int, (string service, string header)> ServiceMap =
                new Dictionary<int, (string service, string header)>
                {
                    {Brightness, (service: "turn_on", header: "Brightness")},
                    {ColorTemp, (service: "turn_on", header: "Color Temperature")},
                    {Color, (service: "turn_on", header: "Color")},
                    {WhiteValue, (service: "turn_on", header: "White Value")}
                };
        }

        private static class SupportedColorModes
        {
            public const string Unknown = "unknown";
            public const string OnOff = "onoff";
            public const string Brightness = "brightness";
            public const string ColorTemp = "color_temp";
            public const string Hs = "hs";
            public const string Xy = "xy";
            public const string Rgb = "rgb";
            public const string Rgbw = "rgbw";
            public const string Rgbww = "rgbww";
            public const string White = "white";

            public static readonly HashSet<string> All = new HashSet<string>
                {OnOff, Brightness, ColorTemp, Hs, Xy, Rgb, Rgbw, Rgbww, White};

            public static readonly HashSet<string> ColorModes = new HashSet<string> {Hs, Xy, Rgb, Rgbw, Rgbww};

            public static readonly HashSet<string> BrightnessModes = new HashSet<string>
                {Brightness, ColorTemp, Hs, Xy, Rgb, Rgbw, Rgbww, White};
        }
    }
}