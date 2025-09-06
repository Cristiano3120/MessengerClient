using DnsClient;
using System.IO;
using System.Text.Json;

namespace MessengerClient
{
    internal static class Helper
    {
        public static JsonElement GetConfig()
        {
            const string ConfigFileName = "Config.json";

            string configFilePath = GetDynamicPath(ConfigFileName);
            return JsonDocument.Parse(File.ReadAllText(configFilePath)).RootElement;
        }

        public static string GetDynamicPath(string relativePath)
        {
            string projectBasePath = AppContext.BaseDirectory;
            int binIndex = projectBasePath.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

            if (binIndex == -1)
            {
                throw new Exception("Could not determine project base path!");
            }

            projectBasePath = projectBasePath[..binIndex];
            return Path.Combine(projectBasePath, relativePath);
        }

        public static async Task<bool> DomainHasMxRecordAsync(string email)
        {
            try
            {
                string domain = email.Split('@')[1];
                LookupClient lookup = new();
                IDnsQueryResponse result = await lookup.QueryAsync(domain, QueryType.MX);

                return result.Answers.MxRecords().Any();
            }
            catch
            {
                return false;
            }
        }

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            {
                return str;
            }

            return char.ToLower(str[0]) + str[1..];
        }
    }
}
