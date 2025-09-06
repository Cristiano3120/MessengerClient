namespace MessengerClient
{
    public sealed record User
    {
        public string? Email { get; init; }

        [Filter("***")]
        public string? Password { get; init; }
        public string Username { get; init; }

        [Filter($"byte[]")]
        public byte[] ProfilPicture { get; init; } = [];
        public ulong Id { get; init; }
        public string? Biography { get; init; }
        public DateOnly? Birthday { get; init; }

        /// <summary> Two Factor Authentication </summary>
        public bool? TFAEnabled { get; init; }
    }
}
