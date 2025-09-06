using MessengerClient.APIResponse;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MessengerClient.Logging
{
    internal static partial class Logger
    {
        private static readonly string _pathToLogFile;
        private const string PathToLoggingFolder = @"Logs/";
        private const byte MaxAmmountOfLoggingFiles = 10;
        private const string ErrorTag = "ERROR";

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        static Logger()
        {
            _ = AllocConsole();
            _pathToLogFile = MaintainLoggingSystem();
        }

        private static string MaintainLoggingSystem()
        {
            string pathToLoggingDic = Helper.GetDynamicPath(PathToLoggingFolder);
            if (!Directory.Exists(pathToLoggingDic))
            {
                _ = Directory.CreateDirectory(pathToLoggingDic);
            }
            else
            {
                string[] files = Directory.GetFiles(pathToLoggingDic, "*.md");

                if (files.Length >= MaxAmmountOfLoggingFiles)
                {
                    files = [.. files.OrderBy(File.GetCreationTime)];
                    // +1 to make room for a new File
                    int filesToRemove = files.Length - MaxAmmountOfLoggingFiles + 1;

                    for (int i = 0; i < filesToRemove; i++)
                    {
                        File.Delete(files[i]);
                    }
                }
            }

            string timestamp = DateTime.Now.ToString("dd-MM-yyyy/HH-mm-ss");
            string pathToNewFile = Helper.GetDynamicPath($"{pathToLoggingDic}{timestamp}.md");
            File.Create(pathToNewFile).Close();

            return pathToNewFile;
        }

        public static void Log(string msg)
        {
            const string Tag = "INFO";
            Write(ConsoleColor.White, msg, Tag);
        }

        public static void LogError<T>(APIResponse<T> aPIResponse, CallerInfos callerInfos)
        {
            if (aPIResponse is null)
            {
                Write(ConsoleColor.Red, "APIResponse was null. Server didn´t answer", ErrorTag);
                return;
            }

            HttpStatusCode statusCode = aPIResponse.APIError.StatusCode;
            string errorMsg = $"API request was not successful. StatusCode: {statusCode}({(int)statusCode}), Message: {aPIResponse.APIError.Message}";
            string errorInfos = $"ERROR in file {callerInfos.FilePath}, in {callerInfos.CallerName}, at line: {callerInfos.LineNum}";

            Write(ConsoleColor.Red, errorInfos, ErrorTag);
            Write(ConsoleColor.Red, errorMsg, ErrorTag);
            
            string fieldErrors = JsonSerializer.Serialize(aPIResponse.FieldErrors, App.JsonSerializerOptions);
            Write(ConsoleColor.Red, $"FieldErrors: {fieldErrors}", ErrorTag, endingChar: '\0');
        }

        public static void LogError<T>(T exception, CallerInfos callerInfos, bool logOnlyToFile = false) where T : Exception
        {
            if (exception is UnobservedTaskExceptionEventArgs unobservedEx)
            {
                foreach (Exception innerEx in unobservedEx.Exception.Flatten().InnerExceptions)
                {
                    LogError(innerEx, callerInfos);
                }

                return;
            }

            string errorInfos = $"ERROR in file {callerInfos.FilePath}, in {callerInfos.CallerName}, at line: {callerInfos.LineNum}";
            Write(ConsoleColor.Red, errorInfos, ErrorTag, logOnlyToFile, endingChar: '\0');
            Write(ConsoleColor.Red, exception.Message, ErrorTag, logOnlyToFile);
            Write(ConsoleColor.Red, $"{exception}", ErrorTag, logOnlyToFile);

            if (exception.InnerException is not null)
                LogError(exception.InnerException, callerInfos);
        }

        /// <summary>
        /// [ERROR]: Will be infront of every log message.
        /// </summary>
        public static void LogError(string exception, CallerInfos callerInfos)
        {
            string errorInfos = $"{ErrorTag} in file {callerInfos.FilePath}, in {callerInfos.CallerName}(...)";
            Write(ConsoleColor.Red, errorInfos, ErrorTag);
            Write(ConsoleColor.Red, $"{exception}\n", ErrorTag);
        }

        /// <summary>
        /// Formats the HTTP payload and writes it to the console and the log file.
        /// </summary>
        internal static void LogHttpPayload<T>(PayloadType payloadType, HttpRequestType requestType, string content)
        {
            ConsoleColor color = payloadType switch
            {
                PayloadType.Received => ConsoleColor.DarkGreen,
                PayloadType.Sent => ConsoleColor.DarkCyan,
                _ => throw new NotImplementedException($"This PayloadType is not implemented yet. " +
                    $"At: {CallerInfos.Create().CallerName}")
            };

            try
            {
                JsonNode jsonNode = JsonNode.Parse(content) 
                    ?? throw new InvalidOperationException("JsonNode.Parse returned null");

                Type type = typeof(T);
                if (type.CustomAttributes.Any())
                {
                    foreach (System.Reflection.PropertyInfo prop in type.GetProperties())
                    {
                        FilterAttribute? filterAttribute = prop.GetCustomAttributes(typeof(FilterAttribute), false)
                            .FirstOrDefault() as FilterAttribute;

                        if (filterAttribute is not null)
                        {
                            jsonNode[prop.Name.ToCamelCase()] = filterAttribute.FilterString;
                        }
                    }
                }

                content = JsonSerializer.Serialize(jsonNode, App.JsonSerializerOptions);
                content = content.Insert(0, "\n");
            }
            finally { }

            Write(color, content, $"{payloadType}({requestType})");
        }

        /// <summary>
        /// Set endingChar to '\0' to avoid making a new line.
        /// </summary>
        private static void Write(ConsoleColor color, string message, string tag, bool logOnlyToFile = false, char endingChar = '\n')
        {
            try
            {
                string log = $"[{DateTime.Now}] [{tag}]: {message}";
                if (!logOnlyToFile)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine($"{log}{endingChar}");
                    Console.ResetColor();
                }

                using (StreamWriter streamWriter = new(_pathToLogFile, true))
                {
                    streamWriter.WriteLine($"{log}{endingChar}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
