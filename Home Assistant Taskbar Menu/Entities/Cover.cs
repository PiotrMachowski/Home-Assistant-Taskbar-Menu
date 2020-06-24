using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Cover : Entity
    {
        public const string DomainName = "cover";
        private static readonly List<string> OffStatesList = new List<string> {States.Closed, States.Unavailable};

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
                root.Icon = new PackIcon {Kind = PackIconKind.Tick};
            }

            var features = GetSupportedFeatures();
            if (new HashSet<int> {SupportedFeatures.Open, SupportedFeatures.Close}.SetEquals(features))
            {
                root.Click += (sender, args) => { HaClientContext.CallService(dispatcher, this, "toggle"); };
            }
            else if (new HashSet<int> {SupportedFeatures.OpenTilt, SupportedFeatures.CloseTilt}.SetEquals(features))
            {
                root.Click += (sender, args) => { HaClientContext.CallService(dispatcher, this, "toggle_cover_tilt"); };
            }
            else
            {
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Open);
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Close);
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Stop);
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.SetPosition, 0, 100,
                    GetDoubleAttribute("current_position"), "position");
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.OpenTilt);
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.CloseTilt);
                AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.StopTilt);
                AddSliderIfSupported(dispatcher, root, SupportedFeatures.SetTiltPosition, 0, 100,
                    GetDoubleAttribute("current_tilt_position"), "tilt_position");
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

            public static readonly List<int> All = new List<int>
            {
                Open, Close, Stop, SetPosition, OpenTilt, CloseTilt, StopTilt, SetTiltPosition
            };

            public static readonly Dictionary<int, (string service, string header)> ServiceMap =
                new Dictionary<int, (string service, string header)>
                {
                    {Open, (service: "open_cover", header: "Open")},
                    {Close, (service: "close_cover", header: "Close")},
                    {Stop, (service: "stop_cover", header: "Stop")},
                    {SetPosition, (service: "set_cover_position", header: "Position")},
                    {OpenTilt, (service: "open_cover_tilt", header: "Open Tilt")},
                    {CloseTilt, (service: "close_cover_tilt", header: "Close Tilt")},
                    {StopTilt, (service: "stop_cover_tilt", header: "Stop Tilt")},
                    {SetTiltPosition, (service: "set_cover_tilt_position", header: "Tilt Position")}
                };
        }
    }
}