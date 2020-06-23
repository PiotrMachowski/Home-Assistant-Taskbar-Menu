using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Entities;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public static class EntityCreator
    {
        private static readonly List<string> SupportedDomains = new List<string>
        {
            Automation.DomainName,
            Climate.DomainName,
            Cover.DomainName,
            Fan.DomainName,
            InputBoolean.DomainName,
            InputNumber.DomainName,
            InputSelect.DomainName,
            Light.DomainName,
            Lock.DomainName,
            MediaPlayer.DomainName,
            Scene.DomainName,
            Script.DomainName,
            Switch.DomainName,
            Vacuum.DomainName
        };


        public static MyStateObject Create(JToken jToken)
        {
            var domain = jToken["entity_id"].ToString().Split('.')[0];
            JToken newState = jToken;
            switch (domain)
            {
                case Automation.DomainName:
                    return newState?.ToObject<Automation>();
                case Climate.DomainName:
                    return newState?.ToObject<Climate>();
                case Cover.DomainName:
                    return newState?.ToObject<Cover>();
                case Fan.DomainName:
                    return newState?.ToObject<Fan>();
                case InputBoolean.DomainName:
                    return newState?.ToObject<InputBoolean>();
                case InputNumber.DomainName:
                    return newState?.ToObject<InputNumber>();
                case InputSelect.DomainName:
                    return newState?.ToObject<InputSelect>();
                case Light.DomainName:
                    return newState?.ToObject<Light>();
                case Lock.DomainName:
                    return newState?.ToObject<Lock>();
                case MediaPlayer.DomainName:
                    return newState?.ToObject<MediaPlayer>();
                case Scene.DomainName:
                    return newState?.ToObject<Scene>();
                case Switch.DomainName:
                    return newState?.ToObject<Switch>();
                case Vacuum.DomainName:
                    return newState?.ToObject<Vacuum>();
            }

            return null;
        }

        public static MyStateObject CreateFromChangedState(string json)
        {
            JToken jToken = JObject.Parse(json)["event"]?["data"];
            string entityId = jToken?["entity_id"].ToString();
            var domain = entityId?.Split('.')[0];
            if (IsSupported(domain))
            {
                var new_state = jToken?["new_state"];
                var myStateObject = Create(new_state);
                return myStateObject;
            }

            return null;
        }

        public static List<MyStateObject> CreateFromStateList(string json)
        {
            return JObject.Parse(json)["result"].Children<JToken>()
                .Select(jtoken => Create(jtoken))
                .Where(v => v != null)
                .ToList();
        }

        public static void ConvertToMenuItem(string json, string name, Action<MyStateObject> saver,
            Dispatcher dispatcher)
        {
            JToken jToken = JObject.Parse(json)["event"]?["data"];
            var new_state = jToken?["new_state"];
            string entityId = jToken?["entity_id"].ToString();
            var domain = entityId?.Split('.')[0];
            if (IsSupported(domain))
            {
                try
                {
                    Console.WriteLine($"CHANGED ENTITY: {entityId}");
                    var item = Create(new_state);
                    saver.Invoke(item);
                }
                catch (Exception)
                {
                    Console.WriteLine($"ERROR PROCESSING: {entityId}");
                    // ignored
                }
            }
        }

        public static bool IsSupported(string domain)
        {
            return SupportedDomains.Contains(domain);
        }
    }
}