using System;
using System.Collections.Generic;
using System.Linq;
using Home_Assistant_Taskbar_Menu.Entities;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public static class EntityCreator
    {
        private static readonly List<string> SupportedDomains = new List<string>
        {
            AutomationEntity.DomainName,
            ButtonEntity.DomainName,
            ClimateEntity.DomainName,
            CoverEntity.DomainName,
            FanEntity.DomainName,
            InputBooleanEntity.DomainName,
            InputButton.DomainName,
            InputNumberEntity.DomainName,
            InputSelectEntity.DomainName,
            LightEntity.DomainName,
            LockEntity.DomainName,
            MediaPlayerEntity.DomainName,
            NumberEntity.DomainName,
            SceneEntity.DomainName,
            ScriptEntity.DomainName,
            SelectEntity.DomainName,
            SirenEntity.DomainName,
            SwitchEntity.DomainName,
            VacuumEntity.DomainName
        };


        public static Entity Create(JToken jToken)
        {
            var domain = jToken["entity_id"].ToString().Split('.')[0];
            JToken newState = jToken;
            switch (domain)
            {
                case AutomationEntity.DomainName:
                    return newState?.ToObject<AutomationEntity>();
                case ButtonEntity.DomainName:
                    return newState?.ToObject<ButtonEntity>();
                case ClimateEntity.DomainName:
                    return newState?.ToObject<ClimateEntity>();
                case CoverEntity.DomainName:
                    return newState?.ToObject<CoverEntity>();
                case FanEntity.DomainName:
                    return newState?.ToObject<FanEntity>();
                case InputBooleanEntity.DomainName:
                    return newState?.ToObject<InputBooleanEntity>();
                case InputButton.DomainName:
                    return newState?.ToObject<InputButton>();
                case InputNumberEntity.DomainName:
                    return newState?.ToObject<InputNumberEntity>();
                case InputSelectEntity.DomainName:
                    return newState?.ToObject<InputSelectEntity>();
                case LightEntity.DomainName:
                    return newState?.ToObject<LightEntity>();
                case LockEntity.DomainName:
                    return newState?.ToObject<LockEntity>();
                case MediaPlayerEntity.DomainName:
                    return newState?.ToObject<MediaPlayerEntity>();
                case NumberEntity.DomainName:
                    return newState?.ToObject<NumberEntity>();
                case SceneEntity.DomainName:
                    return newState?.ToObject<SceneEntity>();
                case ScriptEntity.DomainName:
                    return newState?.ToObject<ScriptEntity>();
                case SelectEntity.DomainName:
                    return newState?.ToObject<SelectEntity>();
                case SirenEntity.DomainName:
                    return newState?.ToObject<SirenEntity>();
                case SwitchEntity.DomainName:
                    return newState?.ToObject<SwitchEntity>();
                case VacuumEntity.DomainName:
                    return newState?.ToObject<VacuumEntity>();
            }

            return null;
        }

        public static Entity CreateFromChangedState(string json)
        {
            JToken jToken = JObject.Parse(json)["event"]?["data"];
            string entityId = jToken?["entity_id"].ToString();
            var domain = entityId?.Split('.')[0];
            try
            {
                if (IsSupported(domain))
                {
                    var new_state = jToken?["new_state"];
                    var myStateObject = Create(new_state);
                    return myStateObject;
                }
            }
            catch (Exception)
            {
                ConsoleWriter.WriteLine($"ERROR CREATING MENU ITEM FOR: {entityId}", ConsoleColor.Red);
            }

            return null;
        }

        public static List<Entity> CreateFromStateList(string json)
        {
            return JObject.Parse(json)["result"].Children<JToken>()
                .Select(Create)
                .Where(v => v != null)
                .ToList();
        }

        public static bool IsSupported(string domain)
        {
            return SupportedDomains.Contains(domain);
        }
    }
}