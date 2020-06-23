using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public static class ResourceProvider
    {
        public static string NameAndVersion()
        {
            return $"{Assembly.GetExecutingAssembly().GetName().Name} {Version()}";
        }

        public static string Version()
        {
            return $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public static string CopyrightInfo()
        {
            return Assembly.GetExecutingAssembly().CustomAttributes
                .Where(ca => ca.AttributeType?.FullName == "System.Reflection.AssemblyCopyrightAttribute")
                .Select(ca => ca.ConstructorArguments[0]).First().Value.ToString();
        }

        public static string RepoUri()
        {
            return Assembly.GetExecutingAssembly().CustomAttributes
                .Where(ca => ca.AttributeType?.FullName == "System.Reflection.AssemblyDescriptionAttribute")
                .Select(ca => ca.ConstructorArguments[0]).First().Value.ToString();
        }

        public static (string version, string url) LatestVersion()
        {
            try
            {
                using (var webClient = new WebClient {Headers = {["User-Agent"] = @"HATM"}})
                {
                    var releaseUrl = RepoUri().Replace("github.com/", "api.github.com/repos/") + "/releases/latest";
                    var json = webClient.DownloadString(releaseUrl);
                    var version = JObject.Parse(json)["tag_name"];
                    var url = JObject.Parse(json)["html_url"];
                    return version != null && url != null ? (version.ToString(), url.ToString()) : (null, null);
                }
            }
            catch (Exception)
            {
                //ignored
            }

            return (null, null);
        }

        public static bool IsUpToDate((string version, string url) latestVersion)
        {
            return latestVersion.version == null || latestVersion.version == Version();
        }
    }
}