using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class Domain
    {
        private static readonly Dictionary<string, Func<StateObject, string, Dispatcher, ItemsControl>> Converters =
            new Dictionary<string, Func<StateObject, string, Dispatcher, ItemsControl>>
            {
                {Automation.Name, Automation.ToItemsControl},
                {Climate.Name, Climate.ToItemsControl},
                {Cover.Name, Cover.ToItemsControl},
                {Fan.Name, Fan.ToItemsControl},
                {InputBoolean.Name, InputBoolean.ToItemsControl},
                {InputNumber.Name, InputNumber.ToItemsControl},
                {InputSelect.Name, InputSelect.ToItemsControl},
                {Light.Name, Light.ToItemsControl},
                {Lock.Name, Lock.ToItemsControl},
                {MediaPlayer.Name, MediaPlayer.ToItemsControl},
                {Scene.Name, Scene.ToItemsControl},
                {Script.Name, Script.ToItemsControl},
                {Switch.Name, Switch.ToItemsControl},
                {Vacuum.Name, Vacuum.ToItemsControl}
            };

        //ICON https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit

        public static bool IsSupported(StateObject stateObject)
        {
            var domain = stateObject.EntityId.Split('.')[0];
            return SupportedDomainsList().Contains(domain);
        }

        public static void ConvertToMenuItem(StateObject stateObject, string name, Action<EntityMenuItem> saver,
            Dispatcher dispatcher)
        {
            var domain = stateObject.EntityId.Split('.')[0];
            if (Converters.ContainsKey(domain))
            {
                try
                {
                    var item = Converters[domain].Invoke(stateObject, name, dispatcher);
                    saver.Invoke(new EntityMenuItem(item, stateObject));
                }
                catch (Exception)
                {
                    Console.WriteLine($"ERROR PROCESSING: {stateObject.EntityId}");
                    // ignored
                }
            }
        }

        protected static string GetName(StateObject stateObject, string name)
        {
            return !string.IsNullOrEmpty(name)
                ? name
                : (stateObject.GetAttributeValue<string>("friendly_name")
                   ?? stateObject.EntityId).Replace("_", "__");
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

        private static List<string> SupportedDomainsList()
        {
            return Converters.Keys.ToList();
        }
    }
}