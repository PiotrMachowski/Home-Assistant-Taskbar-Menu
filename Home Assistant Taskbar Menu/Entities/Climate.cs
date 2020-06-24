using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Climate : Entity
    {
        public const string DomainName = "climate";
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

        protected override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            var isOn = IsOn();
            return CreateMenuItem(dispatcher, isOn ? "turn_off" : "turn_on", GetName(name), isOn,
                IsAvailable());
        }

        private static class SupportedFeatures
        {
            private const int TargetTemperature = 1;
            private const int TargetTemperatureRange = 2;
            private const int TargetHumidity = 4;
            private const int FanMode = 8;
            private const int PresetMode = 16;
            private const int SwingMode = 32;
            private const int AuxHeat = 64;

            public static readonly List<int> All = new List<int>
            {
                TargetTemperature, TargetTemperatureRange, TargetHumidity, FanMode, PresetMode, SwingMode, AuxHeat
            };
        }
    }
}