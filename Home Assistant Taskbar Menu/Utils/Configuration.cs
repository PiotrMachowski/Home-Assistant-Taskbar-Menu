namespace Home_Assistant_Taskbar_Menu.Utils
{
    public class Configuration
    {
        public string Url { get; }

        public string Token { get; }

        public Configuration(string url, string token)
        {
            Url = url;
            Token = token;
        }
    }
}