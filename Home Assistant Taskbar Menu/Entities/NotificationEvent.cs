using System;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class NotificationEvent
    {
        public static string Domain = "persistent_notification";

        public Type EventType { get; }
        public string Id { get; }
        public string Title { get; }
        public string Message { get; }

        public NotificationEvent(Type eventType, string id, string title, string message)
        {
            EventType = eventType;
            Id = id;
            Title = title;
            Message = message;
        }

        public static NotificationEvent FromJson(string json)
        {
            try
            {
                JToken data = JObject.Parse(json)["event"]?["data"];
                string entityId = data?["entity_id"]?.ToString();
                var newState = data?["new_state"];
                if (newState != null && entityId != null)
                {
                    string id = entityId.Replace($"{Domain}.", "");
                    string title = newState["attributes"]?["title"]?.ToString();
                    string message = newState["attributes"]?["message"]?.ToString();
                    return new NotificationEvent(Type.CREATE, id, title, message);
                }
            }
            catch (Exception)
            {
                //ignored
            }

            return null;
        }

        public enum Type
        {
            CREATE
        }
    }
}