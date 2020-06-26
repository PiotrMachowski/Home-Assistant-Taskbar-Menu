using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Utils;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public abstract class Entity
    {
        [JsonProperty("entity_id")] public string EntityId { get; set; }

        public string State { get; set; }

        public Dictionary<string, object> Attributes { protected get; set; }

        public string GetAttribute(string name)
        {
            return Attributes.ContainsKey(name) ? Attributes[name]?.ToString() : null;
        }

        public bool IsOn()
        {
            return !OffStates().Contains(State) && OffStates().Count != 0;
        }

        public bool IsAvailable()
        {
            return State != States.Unavailable;
        }

        public string GetName(string nameOverride = null)
        {
            return (!string.IsNullOrEmpty(nameOverride)
                ? nameOverride
                : GetAttribute("friendly_name")
                  ?? EntityId).Replace("_", "__");
        }

        public abstract string Domain();

        public MenuItem ToMenuItemSafe(Dispatcher dispatcher, string name)
        {
            try
            {
                return ToMenuItem(dispatcher, name);
            }
            catch (Exception)
            {
                ConsoleWriter.WriteLine($"ERROR CREATING UI FOR ENTITY: {EntityId}", ConsoleColor.Red);
                return new MenuItem { Header = $"ERROR: {EntityId.Replace("_", "__")}", IsEnabled = false };
            }
        }

        protected bool GetBoolAttribute(string name)
        {
            return bool.Parse(GetAttribute(name) ?? "false");
        }

        protected int GetIntAttribute(string name, int defaultValue = 0)
        {
            return ParseInt(GetAttribute(name));
        }

        protected double GetDoubleAttribute(string name)
        {
            return ParseDouble(GetAttribute(name));
        }

        protected double ParseDouble(string value)
        {
            return double.Parse(value?.Replace(",", ".") ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        protected int ParseInt(string value)
        {
            return int.Parse(value?.Replace(",", ".") ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        protected List<string> GetListAttribute(string name)
        {
            return Attributes.ContainsKey(name)
                ? ((JArray)Attributes[name]).Select(i => (string)i).ToList()
                : new List<string>();
        }

        protected virtual List<string> OffStates()
        {
            return new List<string>();
        }

        protected virtual List<int> AllSupportedFeatures()
        {
            return new List<int>();
        }

        protected virtual Dictionary<int, (string service, string header)> FeatureToServiceMap()
        {
            return new Dictionary<int, (string service, string header)>();
        }

        protected List<int> GetSupportedFeatures()
        {
            var supportedFeatures = GetIntAttribute("supported_features");
            return AllSupportedFeatures()
                .Where(sf => (sf & supportedFeatures) > 0)
                .ToList();
        }

        protected abstract MenuItem ToMenuItem(Dispatcher dispatcher, string name);

        protected bool IsSupported(int supportedFeature)
        {
            return GetSupportedFeatures().Contains(supportedFeature);
        }

        protected void AddMenuItemIfSupported(Dispatcher dispatcher, ItemsControl root, int supportedFeature)
        {
            var featureToServiceMap = FeatureToServiceMap();
            if (IsSupported(supportedFeature) && featureToServiceMap.ContainsKey(supportedFeature))
            {
                root.Items.Add(CreateMenuItem(dispatcher, featureToServiceMap[supportedFeature]));
            }
        }

        protected MenuItem CreateMenuItem(Dispatcher dispatcher, (string service, string header) featureToService)
        {
            return CreateMenuItem(dispatcher, featureToService.service, featureToService.header);
        }

        protected MenuItem CreateMenuItem(Dispatcher dispatcher, string service, string header, bool isChecked = false,
            bool isEnabled = true, string toolTip = null, params Tuple<string, object>[] data)
        {
            var serviceItem = new MenuItem {Header = header, ToolTip = toolTip, StaysOpenOnClick = true};
            if (isChecked)
            {
                serviceItem.Icon = new PackIcon {Kind = PackIconKind.Tick};
            }

            serviceItem.Click += (sender, args) => { HaClientContext.CallService(dispatcher, this, service, data); };
            return serviceItem;
        }

        protected Slider CreateSlider(Dispatcher dispatcher, double min, double max, double value, string service,
            string toolTip, string attribute, double step = 1)
        {
            var slider = new Slider
            {
                Minimum = min, Maximum = max, MinWidth = 100, ToolTip = toolTip, Value = value,
                IsSnapToTickEnabled = true,
                TickFrequency = step
            };
            slider.PreviewMouseUp += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, this, service,
                    Tuple.Create<string, object>(attribute, step == 1 ? (int) slider.Value : slider.Value));
            };
            return slider;
        }

        protected void AddSliderIfSupported(Dispatcher dispatcher, ItemsControl root, int supportedFeature, double min,
            double max, double value, string attribute, double step = 1)
        {
            var supportedFeatures = GetSupportedFeatures();
            var featureToServiceMap = FeatureToServiceMap();
            if (supportedFeatures.Contains(supportedFeature) && featureToServiceMap.ContainsKey(supportedFeature))
            {
                var (service, header) = featureToServiceMap[supportedFeature];
                root.Items.Add(CreateSlider(dispatcher, min, max, value, service, header, attribute, step));
            }
        }

        protected static class States
        {
            public const string Unavailable = "unavailable";
            public const string On = "on";
            public const string Off = "off";
            public const string Open = "open";
            public const string Closed = "closed";
            public const string Idle = "idle";
            public const string Docked = "docked";
        }

        public override string ToString()
        {
            var fName = GetAttribute("friendly_name");
            return fName != null ? $"{fName} ({EntityId})" : EntityId;
        }
    }
}