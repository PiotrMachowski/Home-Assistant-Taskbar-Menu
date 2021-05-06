using System;
using Home_Assistant_Taskbar_Menu.Utils;
using RestSharp;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public class HomeAssistantRestClient
    {
        private readonly RestClient _client;

        public HomeAssistantRestClient(Configuration configuration)
        {
            _client = new RestClient(configuration.HttpUrl()) {AutomaticDecompression = false};
            _client.AddDefaultHeader("Authorization", $"Bearer {configuration.Token}");
        }

        public void CallService(string domain, string service, string data)
        {
            ConsoleWriter.WriteLine("Calling service", ConsoleColor.Red);
            ConsoleWriter.WriteLine($"{domain}.{service}", ConsoleColor.Blue);
            ConsoleWriter.WriteLine(data, ConsoleColor.Green);
            var request = new RestRequest($"/api/services/{domain}/{service}");
            request.AddParameter("application/json", data, ParameterType.RequestBody);
            _client.Post(request);
        }
    }
}