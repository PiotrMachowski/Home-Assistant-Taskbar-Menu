using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
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

        protected bool GetBoolAttribute(string name)
        {
            return bool.Parse(GetAttribute(name) ?? "false");
        }

        protected int GetIntAttribute(string name, int defaultValue = 0)
        {
            return int.Parse(GetAttribute(name) ?? defaultValue.ToString());
        }

        protected double GetDoubleAttribute(string name)
        {
            return double.Parse(GetAttribute(name) ?? "0");
        }

        protected List<string> GetListAttribute(string name)
        {
            return Attributes.ContainsKey(name)
                ? ((JArray) Attributes[name]).Select(i => (string) i).ToList()
                : new List<string>();
        }

        public bool IsOn()
        {
            return !OffStates().Contains(State);
        }

        public string GetName(string nameOverride)
        {
            return (!string.IsNullOrEmpty(nameOverride)
                ? nameOverride
                : GetAttribute("friendly_name")
                  ?? EntityId).Replace("_", "__");
        }

        public abstract string Domain();

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


        public abstract Control ToMenuItem(Dispatcher dispatcher, string name);

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
            string toolTip = null, params Tuple<string, object>[] data)
        {
            var serviceItem = new MenuItem {Header = header, IsChecked = isChecked, ToolTip = toolTip};
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
    }
}