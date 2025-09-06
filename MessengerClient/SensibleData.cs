using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MessengerClient
{
    internal static class SensibleData
    {
        private const string CredentialsFileName = "credentials.dat";
        private static string CredentialsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            MessengerInfo.Name,
            CredentialsFileName
        );


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static LoginData? GetLoginData()
        {
            EnsureCredentialsDirectoryExists();
            if (!File.Exists(CredentialsFilePath))
            {
                return null;
            }

            byte[] encryptedBytes = File.ReadAllBytes(CredentialsFilePath);
            if (encryptedBytes.Length == 0)
            {
                return null;
            }

            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            string jsonStr = Encoding.UTF8.GetString(decryptedBytes);

            LoginData? loginData = JsonSerializer.Deserialize<LoginData>(jsonStr);
            loginData?.IsAutoLogin = true;

            if (loginData is null)
            {
                ClearLoginData(); // Clear cause the data is damaged
            }

            return loginData;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static void SetLoginData(LoginData loginData)
        {
            EnsureCredentialsDirectoryExists();

            string jsonStr = JsonSerializer.Serialize(loginData);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
            byte[] protectedData = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);

            File.WriteAllBytes(CredentialsFilePath, protectedData);
        }

        public static void ClearLoginData()
        {
            EnsureCredentialsDirectoryExists();
            File.WriteAllBytes(CredentialsFilePath, []);
        }

        private static void EnsureCredentialsDirectoryExists()
        {
            string directoryPath = CredentialsFilePath.Replace(CredentialsFileName, "");
            if (directoryPath is not null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

    }
}
