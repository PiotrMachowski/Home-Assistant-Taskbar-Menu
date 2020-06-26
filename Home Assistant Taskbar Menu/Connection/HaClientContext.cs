using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Connection;
using Home_Assistant_Taskbar_Menu.Entities;

namespace Home_Assistant_Taskbar_Menu
{
    public static class HaClientContext
    {
        public static HomeAssistantWebsocketsClient HomeAssistantWebsocketClient { get; set; }

        public static async Task Start()
        {
            await HomeAssistantWebsocketClient.Start();
        }

        public static void AddStateChangeListener(object identifier, Action<Entity> listener)
        {
            HomeAssistantWebsocketClient.AddStateChangeListener(identifier, listener);
        }

        public static void RemoveStateChangeListener(object identifier)
        {
            HomeAssistantWebsocketClient.RemoveStateChangeListener(identifier);
        }

        public static void AddAuthenticationStateListener(Action<bool> listener)
        {
            HomeAssistantWebsocketClient.AddAuthenticationStateListener(listener);
        }

        public static void AddEntitiesListListener(Action<List<Entity>> listener)
        {
            HomeAssistantWebsocketClient.AddEntitiesListListener(listener);
        }

        public static void CallService(Dispatcher dispatcher, Entity stateObject, string service,
            params Tuple<string, object>[] data)
        {
            dispatcher.Invoke(() =>
                HomeAssistantWebsocketClient.CallService(
                    new HomeAssistantServiceCallData(service, stateObject, data)));
        }
    }
}