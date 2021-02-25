using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public static class HaClientContext
    {
        private static Configuration Configuration { get; set; }
        private static HomeAssistantWebsocketsClient HomeAssistantWebsocketClient { get; set; }

        public static void Initialize(Configuration configuration)
        {
            Configuration = configuration;
            HomeAssistantWebsocketClient = new HomeAssistantWebsocketsClient(Configuration);
        }

        public static void Recreate()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

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

        public static void AddNotificationListener(Action<NotificationEvent> listener)
        {
            HomeAssistantWebsocketClient.AddNotificationListener(listener);
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