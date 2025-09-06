using System.Net;

namespace MessengerClient.APIResponse
{
    internal readonly record struct APIError
    {
        public HttpStatusCode StatusCode { get; init; }
        public string Message { get; init; }
    }
}
