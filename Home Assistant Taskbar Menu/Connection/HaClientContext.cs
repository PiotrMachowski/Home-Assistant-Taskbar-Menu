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

        public static void AddStateChangeListener(Action<Entity> listener)
        {
            HomeAssistantWebsocketClient.AddStateChangeListener(listener);
        }

        public static async Task GetStates(Action<List<Entity>> callback)
        {
            await HomeAssistantWebsocketClient.GetStates(callback);
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