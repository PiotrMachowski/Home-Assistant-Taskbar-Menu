using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Home_Assistant_Taskbar_Menu.Entities;
using Home_Assistant_Taskbar_Menu.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public class HomeAssistantWebsocketsClient
    {
        private readonly bool _debug = false;
        private readonly Timer _timer;
        private readonly WebsocketClient _websocketClient;
        private readonly string _token;
        private readonly List<ApiConsumer> _consumers;
        private long _counter;
        private bool _authenticated;

        private bool Authenticated
        {
            get => _authenticated;
            set
            {
                _authenticatedListener?.Invoke(value);
                _authenticated = value;
            }
        }

        private Action<bool> _authenticatedListener;
        private readonly Dictionary<object, Action<Entity>> _stateChangeListeners;
        private readonly List<Action<NotificationEvent>> _notificationListeners;
        private readonly List<Action<List<Entity>>> _entitiesListListeners;

        public HomeAssistantWebsocketsClient(Configuration configuration)
        {
            _token = configuration.Token;
            _counter = 1;
            _websocketClient = new WebsocketClient(new Uri(configuration.Url));
            _timer = new Timer(60 * 1000);
            _consumers = new List<ApiConsumer>();
            _stateChangeListeners = new Dictionary<object, Action<Entity>>();
            _entitiesListListeners = new List<Action<List<Entity>>>();
            _notificationListeners = new List<Action<NotificationEvent>>();
            if (_debug)
            {
                _consumers.Add(
                    new ApiConsumer(msg => true,
                        msg => ConsoleWriter.WriteLine("RECEIVED: " + msg.Text, ConsoleColor.Gray)));
            }

            AuthFlow();
            _websocketClient.MessageReceived.Subscribe(msg =>
            {
                var toRemove = _consumers.Where(c => c.Consume(msg)).ToList();
                toRemove.ForEach(c => _consumers.Remove(c));
            });
            _consumers.Add(
                new ApiConsumer(
                    msg =>
                    {
                        var jsonObject = JObject.Parse(msg.Text);
                        return (string) jsonObject["type"] == "event" &&
                               (string) jsonObject["event"]["event_type"] == "state_changed";
                    },
                    msg =>
                    {
                        Entity entity = EntityCreator.CreateFromChangedState(msg.Text);
                        if (entity != null)
                        {
                            _stateChangeListeners.Values.ToList().ForEach(a => Task.Run(() => a.Invoke(entity)));
                        }
                    }));
            _consumers.Add(
                new ApiConsumer(
                    msg =>
                    {
                        var jsonObject = JObject.Parse(msg.Text);
                        return (string) jsonObject["type"] == "event" &&
                               (string) jsonObject["event"]["event_type"] == "state_changed" &&
                               ((string) jsonObject["event"]["data"]["entity_id"]).Contains(NotificationEvent.Domain);
                    },
                    msg =>
                    {
                        NotificationEvent notificationEvent = NotificationEvent.FromJson(msg.Text);
                        if (notificationEvent != null)
                        {
                            _notificationListeners.ForEach(n => Task.Run(() => n.Invoke(notificationEvent)));
                        }
                    }));
            _websocketClient.ReconnectionHappened.Subscribe(recInfo =>
            {
                {
                    ConsoleWriter.WriteLine($"RECONNECTION HAPPENED: {recInfo.Type}", ConsoleColor.Yellow);
                    if (recInfo.Type == ReconnectionType.NoMessageReceived)
                    {
                        HaClientContext.Recreate();
                    }

                    if (recInfo.Type != ReconnectionType.Initial)
                    {
                        Authenticated = false;
                        Authenticate();
                    }
                }
            });
        }

        public void AddAuthenticationStateListener(Action<bool> handler)
        {
            _authenticatedListener = handler;
        }

        public void AddStateChangeListener(object identifier, Action<Entity> handler)
        {
            _stateChangeListeners.Add(identifier, handler);
        }

        public void RemoveStateChangeListener(object identifier)
        {
            _stateChangeListeners.Remove(identifier);
        }

        public void AddEntitiesListListener(Action<List<Entity>> handler)
        {
            _entitiesListListeners.Add(handler);
        }

        public async Task Start()
        {
            ConsoleWriter.WriteLine("STARTING", ConsoleColor.Blue);
            await _websocketClient.Start();
            _timer.Elapsed += (sender, args) => Ping();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public void Disconnect()
        {
            _websocketClient.Dispose();
        }

        private void AuthFlow()
        {
            _consumers.Add(
                new ApiConsumer(
                    msg => (string) JObject.Parse(msg.Text)["type"] == "auth_required",
                    msg =>
                    {
                        ConsoleWriter.WriteLine("AUTH REQUIRED", ConsoleColor.Yellow);
                        Authenticated = false;
                        Authenticate();
                    }));
            _consumers.Add(
                new ApiConsumer(
                    msg => (string) JObject.Parse(msg.Text)["type"] == "auth_ok",
                    msg =>
                    {
                        ConsoleWriter.WriteLine("AUTH OK", ConsoleColor.Green);
                        Authenticated = true;
                        Task.Run(GetStates);
                        SubscribeStateChange();
                    }));
        }

        private void Authenticate()
        {
            var authMessage = $"{{\"type\": \"auth\",\"access_token\": \"{_token}\"}}";
            CallApi(authMessage);
        }

        private void SubscribeStateChange()
        {
            ConsoleWriter.WriteLine("SUBSCRIBE STATE CHANGES", ConsoleColor.Blue);
            var id = _counter++;
            var subscribeMsg = $"{{\"id\": {id},\"type\": \"subscribe_events\",\"event_type\": \"state_changed\"}}";
            CallApi(subscribeMsg);
        }

        private void Ping()
        {
            if (Authenticated)
            {
                ConsoleWriter.WriteLine("PING", ConsoleColor.Blue);
                var id = _counter++;
                var subscribeMsg = $"{{\"id\": {id},\"type\": \"ping\"}}";
                CallApi(subscribeMsg);
            }
        }

        private async Task GetStates()
        {
            ConsoleWriter.WriteLine("GETTING STATES", ConsoleColor.Blue);
            var id = _counter++;
            var subscribeMsg = $"{{\"id\": {id},\"type\": \"get_states\"}}";
            await CallApi(id, subscribeMsg, msg =>
            {
                List<Entity> states = EntityCreator.CreateFromStateList(msg.Text);
                states.Sort((o1, o2) => string.Compare(o1.EntityId, o2.EntityId, StringComparison.Ordinal));
                Task.Run(() => _entitiesListListeners.ForEach(l => l.Invoke(states)));
            });
        }

        private async Task CallApi(long id, string message, Action<ResponseMessage> resultHandler = null,
            bool waitFotAuth = true)
        {
            while (!Authenticated & waitFotAuth)
            {
                await Task.Delay(10);
            }

            if (!Authenticated)
            {
                return;
            }

            await Task.Run(() => CallApi(message));
            if (resultHandler != null)
            {
                var tuple = new ApiConsumer(
                    msg =>
                    {
                        var jsonObject = JObject.Parse(msg.Text);
                        return (string) jsonObject["type"] == "result" &&
                               (int) jsonObject["id"] == id;
                    },
                    resultHandler, true);
                _consumers.Add(tuple);
            }
        }

        private void CallApi(string message)
        {
            if (_debug)
            {
                ConsoleWriter.WriteLine("SENT: " + message, ConsoleColor.Gray);
            }

            _websocketClient.Send(message);
        }

        public void CallService(HomeAssistantServiceCallData serviceCallData)
        {
            var id = _counter++;
            JObject serviceCallObject = JObject.FromObject(serviceCallData);
            serviceCallObject["id"] = id;
            serviceCallObject["type"] = "call_service";
            var json = JsonConvert.SerializeObject(serviceCallObject);
            ConsoleWriter.WriteLine($"CALLING SERVICE: {json}", ConsoleColor.Blue);
            CallApi(json);
        }

        public void AddNotificationListener(Action<NotificationEvent> listener)
        {
            _notificationListeners.Add(listener);
        }
    }
}