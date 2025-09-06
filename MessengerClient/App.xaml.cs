using MessengerClient.APIResponse;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace MessengerClient
{
    public partial class App : Application
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await WaitTillServerReachableAsync();
            await LoginAsync();
        }

        public static async Task WaitTillServerReachableAsync()
        {
            int maxRetryDelay = 5000; // 30 seconds
            int retryDelay = 5000; // 5 seconds
            int retryCount = 0;

            while (true)
            {
                retryCount++;

                APIResponse<bool> aPIResponse = await Http.GetAsync<bool>("auth/ping", CallerInfos.Create());
                if (aPIResponse.IsSuccess)
                {
                    Logger.Log("Server reached!.");
                    return;
                }

                if (retryCount % 5 == 0)
                {
                    retryDelay = Math.Min(retryDelay * 2, maxRetryDelay);
                    Logger.Log($"Doubling retry time. New time: {retryDelay / 1000} seconds");
                }

                Logger.Log($"Server not reachable. Retrying in {retryDelay / 1000} seconds... (Attempt {retryCount})");
                await Task.Delay(retryDelay);
            }
        }

        public static async Task LoginAsync()
        {
            LoginData? loginData = SensibleData.GetLoginData();
            if (loginData is null)
            {
                _ = GUIHelper.SwitchWindows<LoadingScreen, Login>(switchDelay: 0);
                return;
            }

            APIResponse<User> apiResponse = await Http.PostAsync<LoginData, User>(loginData, "/login", CallerInfos.Create());
            if (!apiResponse.IsSuccess)
            {
                Logger.LogError(apiResponse, CallerInfos.Create());
                return;
            }

            HomeWindow home = new(apiResponse.Data);
            _ = GUIHelper.SwitchWindows<LoadingScreen, HomeWindow>(home);
        }
    }
}
