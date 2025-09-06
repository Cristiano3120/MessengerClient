using System.Diagnostics.CodeAnalysis;

namespace MessengerClient.APIResponse
{
    internal sealed record APIResponse<T>
    {
        [MemberNotNullWhen(true, nameof(Data))]
        public bool IsSuccess { get; init; } = false;
        public T? Data { get; init; }
        public APIError APIError { get; init; }
        public List<APIFieldError> FieldErrors { get; init; } = [];
    }
}
