using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using HADotNet.Core.Models;
using Home_Assistant_Taskbar_Menu.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public class HomeAssistantWebsocketsClient
    {
        private readonly Timer _timer;
        private readonly WebsocketClient _websocketClient;
        private readonly string _token;
        private readonly List<ApiConsumer> _consumers;
        private long _counter;
        private bool _authenticated;
        private readonly List<Action<StateObject>> _stateChangeListeners;

        public HomeAssistantWebsocketsClient(Configuration configuration)
        {
            _token = configuration.Token;
            _counter = 1;
            _websocketClient = new WebsocketClient(new Uri(configuration.Url));
            _timer = new Timer(60 * 1000 * 10);
            _consumers = new List<ApiConsumer>();
            _stateChangeListeners = new List<Action<StateObject>>();
            bool debug = !true;
            if (debug)
            {
                _consumers.Add(new ApiConsumer(msg => true, Console.WriteLine));
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
                        StateObject stateObject = JObject.Parse(msg.Text)["event"]["data"]["new_state"]
                            .ToObject<StateObject>();
                        _stateChangeListeners.ForEach(a => Task.Run(() => a.Invoke(stateObject)));
                    }));
            _websocketClient.ReconnectionHappened.Subscribe((recInfo) =>
            {
                Console.WriteLine($"RECONNECTION HAPPENED: {recInfo.Type}");
            });

        }

        private void AuthFlow()
        {
            _consumers.Add(
                new ApiConsumer(
                    msg => (string) JObject.Parse(msg.Text)["type"] == "auth_required",
                    msg =>
                    {
                        Console.WriteLine("AUTH REQUIRED");
                        _authenticated = false;
                        Authenticate();
                    }));
            _consumers.Add(
                new ApiConsumer(
                    msg => (string) JObject.Parse(msg.Text)["type"] == "auth_ok",
                    msg =>
                    {
                        Console.WriteLine("AUTH OK");
                        _authenticated = true;
                        SubscribeStateChange();
                    }));
        }

        private void Authenticate()
        {
            var authMessage = $"{{\"type\": \"auth\",\"access_token\": \"{_token}\"}}";
            _websocketClient.Send(authMessage);
        }

        public async Task Start()
        {
            Console.WriteLine("STARTING");
            await _websocketClient.Start();
            _timer.Elapsed += (sender, args) => Task.Run(() => Ping());
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async Task SubscribeStateChange()
        {
            Console.WriteLine("SUBSCRIBE STATE CHANGES");
            var id = _counter++;
            var subscribeMsg =
                $"{{ \"id\": {id},\"type\": \"subscribe_events\",\"event_type\": \"state_changed\"}}";
            await CallApi(id, subscribeMsg);
        }

        private async Task Ping()
        {
            Console.Out.WriteLine("PING");
            var id = _counter++;
            var subscribeMsg =
                $"{{ \"id\": {id},\"type\": \"ping\"}}";
            await CallApi(id, subscribeMsg);
        }

        public void AddStateChangeListener(Action<StateObject> handler)
        {
            _stateChangeListeners.Add(handler);
        }

        public async Task GetStates(Action<List<StateObject>> callback)
        {
            Console.WriteLine("GETTING STATES");
            var id = _counter++;
            var subscribeMsg =
                $"{{ \"id\": {id},\"type\": \"get_states\"}}";
            await CallApi(id, subscribeMsg, msg =>
            {
                List<StateObject> states = JObject.Parse(msg.Text)["result"].Children<JToken>()
                    .Select(n => n.ToObject<StateObject>()).ToList();
                states.Sort((o1, o2) => string.Compare(o1.EntityId, o2.EntityId, StringComparison.Ordinal));
                Task.Run(() => callback.Invoke(states));
            });
        }

        private async Task CallApi(long id, string message, Action<ResponseMessage> resultHandler = null)
        {
            while (!_authenticated)
            {
                await Task.Delay(10);
            }

            await Task.Run(() => _websocketClient.Send(message));
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

        public async Task CallService(HomeAssistantServiceCallData serviceCallData)
        {
            var id = _counter++;
            JObject serviceCallObject = JObject.FromObject(serviceCallData);
            serviceCallObject["id"] = id;
            serviceCallObject["type"] = "call_service";
            var json = JsonConvert.SerializeObject(serviceCallObject);
            Console.WriteLine($"CALLING SERVICE: {json}");
            await CallApi(id, json);
        }
    }
}