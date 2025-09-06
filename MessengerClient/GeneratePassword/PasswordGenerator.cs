using System.Security.Cryptography;
using System.Windows;

namespace MessengerClient.GeneratePassword
{
    public static class PasswordGenerator
    {
        private static readonly Dictionary<Chars, string> _charMap = new()
        {
            { Chars.LowerCase, "abcdefghijklmnopqrstuvwxyz" },
            { Chars.UpperCase, "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
            { Chars.Digits, "0123456789" },
            { Chars.Brackets, "()" },
            { Chars.CurlyBrackets, "{}" },
            { Chars.Dollar, "$" },
            { Chars.ExclamationMark, "!" },
            { Chars.QuestionMark, "?" },
            { Chars.AtSign, "@" },
            { Chars.Hashtag, "#" },
            { Chars.Percent, "%" },
            { Chars.Dot, "." },
            { Chars.And, "&" },
            { Chars.Euro, "€" }
        };

        public static string GeneratePassword()
        {
            const byte passwordLength = 30;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                List<char> allowedChars = [];
                foreach (KeyValuePair<Chars, string> kvp in _charMap)
                {
                    if (kvp.Key == Chars.LowerCase || kvp.Key == Chars.UpperCase)
                    {
                        allowedChars.AddRange(kvp.Value);
                    }
                    else
                    {
                        int repeatCharCount = Random.Shared.Next(2, 4); ;
                        for (int i = 0; i < repeatCharCount; i++)
                        {
                            allowedChars.AddRange(kvp.Value);
                        }
                    }
                }

                char[] password = new char[passwordLength];
                byte[] randomBytes = new byte[passwordLength];

                for (int i = 0; i < passwordLength; i++)
                {
                    rng.GetBytes(randomBytes, 0, 1);
                    int index = randomBytes[0] % allowedChars.Count;
                    password[i] = allowedChars[index];
                }

                return new string(password);
            }
        }
    }
}
