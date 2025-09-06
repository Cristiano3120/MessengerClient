using MessengerClient.APIResponse;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MessengerClient
{
    internal static class Http
    {
        private static readonly HttpClient _httpClient = InitHttpClient();

        private static HttpClient InitHttpClient()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri(GetUri()),
                Timeout = TimeSpan.FromSeconds(5)
            };

            return client;
        }

        public static async Task<APIResponse<T>> GetAsync<T>(string endpoint, CallerInfos callerInfos)
            => await HandleRequestAsync<T, T>(HttpRequestType.Get, default!, callerInfos, endpoint);

        public static async Task<APIResponse<bool>> DeleteAsync(string endpoint, CallerInfos callerInfos)
            => await HandleRequestAsync<bool, bool>(HttpRequestType.Delete, default!, callerInfos, endpoint);

        public static async Task<APIResponse<TOutput>> PostAsync<TInput, TOutput>(TInput content, string endpoint, CallerInfos callerInfos)
            => await HandleRequestAsync<TInput, TOutput>(HttpRequestType.Post, content, callerInfos, endpoint);

        public static async Task<APIResponse<TOutput>> PatchAsync<TInput, TOutput>(TInput content, string endpoint, CallerInfos callerInfos)
            => await HandleRequestAsync<TInput, TOutput>(HttpRequestType.Patch, content, callerInfos, endpoint);

        public static async Task<APIResponse<TOutput>> PutAsync<TInput, TOutput>(TInput content, string endpoint, CallerInfos callerInfos)
            => await HandleRequestAsync<TInput, TOutput>(HttpRequestType.Put, content, callerInfos, endpoint);

        private static async Task<APIResponse<TOutput>> HandleRequestAsync<TInput, TOutput>(HttpRequestType httpRequestType, TInput input, CallerInfos callerInfos,
            string endpoint)
        {
            APIResponse<TOutput> result = default!;
            try
            {
                if (!endpoint.StartsWith('/'))
                {
                    endpoint = endpoint.Insert(0, "/");
                }

                Logger.Log($"[{httpRequestType}]: {endpoint}");

                HttpResponseMessage response = httpRequestType switch
                {
                    HttpRequestType.Get => await _httpClient.GetAsync(endpoint),
                    HttpRequestType.Delete => await _httpClient.DeleteAsync(endpoint),
                    HttpRequestType.Post => await SendJsonAsync(httpRequestType, endpoint, input),
                    HttpRequestType.Patch => await SendJsonAsync(httpRequestType, endpoint, input),
                    HttpRequestType.Put => await SendJsonAsync(httpRequestType, endpoint, input),
                    _ => throw new InvalidEnumArgumentException(
                        nameof(httpRequestType), (int)httpRequestType, typeof(HttpRequestType))
                };

                string json = await response.Content.ReadAsStringAsync();
                Logger.LogHttpPayload<TOutput>(PayloadType.Received, httpRequestType, json);

                result = JsonSerializer.Deserialize<APIResponse<TOutput>>(json, App.JsonSerializerOptions) 
                    ?? throw new InvalidOperationException($"Deserialized result was null. Expected {typeof(APIResponse<TOutput>)}");
                
                if (!result.IsSuccess)
                {
                    Logger.LogError(result, callerInfos);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(result, callerInfos);
                Logger.LogError(ex, callerInfos);

                LoadingScreen loadingScreen = new();
                loadingScreen.Show();
                GUIHelper.CloseAllWindowsExcept(loadingScreen);

                return new APIResponse<TOutput>();
            }
        }

        private static async Task<HttpResponseMessage> SendJsonAsync<T>(HttpRequestType httpRequestType, string endpoint, T? input)
        {
            StringContent content = new(JsonSerializer.Serialize(input, App.JsonSerializerOptions),
                Encoding.UTF8,
                "application/json");

            Logger.LogHttpPayload<T>(PayloadType.Sent, httpRequestType, $"{await content.ReadAsStringAsync()}\n");

            return httpRequestType switch
            {
                HttpRequestType.Post => await _httpClient.PostAsync(endpoint, content),
                HttpRequestType.Patch => await _httpClient.PatchAsync(endpoint, content),
                HttpRequestType.Put => await _httpClient.PutAsync(endpoint, content),
                _ => throw new InvalidOperationException($"Unsupported method {httpRequestType} in {CallerInfos.Create().CallerName}")
            };
        }

        private static string GetUri()
        {
            JsonElement config = Helper.GetConfig();
            bool testing = config.TryGetProperty("testing", out JsonElement property) && property.GetBoolean();

            if (testing)
            {
                return "http://localhost";
            }

            Exception exception = new("URI missing or damaged");
            if (config.TryGetProperty("uri", out JsonElement uriProperty))
            {
                return uriProperty.GetString() ?? throw exception;
            }

            throw exception;
        }
    }
}
