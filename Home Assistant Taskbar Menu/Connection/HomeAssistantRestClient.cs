using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Home_Assistant_Taskbar_Menu.Utils;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public class HomeAssistantRestClient
    {
        private readonly HttpClient _httpClient;

        public HomeAssistantRestClient(Configuration configuration)
        {
            _httpClient = new HttpClient {BaseAddress = new Uri(configuration.HttpUrl())};
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.Token}");
            ConsoleWriter.WriteLine($"HTTP URL: {configuration.HttpUrl()}", ConsoleColor.Green);
        }

        public void CallService(string domain, string service, string data)
        {
            ConsoleWriter.WriteLine("Calling service: ", ConsoleColor.Yellow);
            ConsoleWriter.Write("  Service: ", ConsoleColor.Yellow);
            ConsoleWriter.WriteLine($"{domain}.{service}", ConsoleColor.Blue, false);
            ConsoleWriter.Write("  Data: ", ConsoleColor.Yellow);
            ConsoleWriter.WriteLine(data, ConsoleColor.Gray, false);

            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync($"/api/services/{domain}/{service}",
                content).Result;

            ConsoleWriter.WriteLine("Response:", ConsoleColor.Green);
            ConsoleWriter.Write($"  Status code: ", ConsoleColor.Blue);
            ConsoleWriter.WriteLine(response.StatusCode.ToString(),
                response.StatusCode == HttpStatusCode.OK ? ConsoleColor.Green : ConsoleColor.Red, false);
            ConsoleWriter.Write($"  Body: ", ConsoleColor.Cyan);
            ConsoleWriter.WriteLine(response.Content.ReadAsStringAsync().Result, ConsoleColor.Gray, false);
        }
    }
}