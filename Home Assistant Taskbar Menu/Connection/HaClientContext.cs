using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using HADotNet.Core.Models;
using Home_Assistant_Taskbar_Menu.Connection;

namespace Home_Assistant_Taskbar_Menu
{
    public static class HaClientContext
    {
        public static HomeAssistantWebsocketsClient HomeAssistantWebsocketClient { get; set; }

        public static async Task Start()
        {
            await HomeAssistantWebsocketClient.Start();
        }

        public static void AddStateChangeListener(Action<StateObject> listener)
        {
            HomeAssistantWebsocketClient.AddStateChangeListener(listener);
        }

        public static async Task GetStates(Action<List<StateObject>> callback)
        {
            await HomeAssistantWebsocketClient.GetStates(callback);
        }


        public static void CallService(Dispatcher dispatcher, string domain, string service,
            StateObject stateObject, params Tuple<string, object>[] data)
        {
            dispatcher.Invoke(() =>
                HomeAssistantWebsocketClient.CallService(
                    new HomeAssistantServiceCallData(domain, service, stateObject, data)));
        }
    }
}