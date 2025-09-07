namespace MessengerClient
{
    internal readonly record struct Verify
    {
        public required int VerificationCode { get; init; }
        public required ulong UserId { get; init; }
    }
}
