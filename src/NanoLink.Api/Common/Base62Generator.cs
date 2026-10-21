using System.Security.Cryptography;

namespace NanoLink.Api.Common;

public static class Base62Generator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Generate(int length = 7)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        // RandomNumberGenerator é criptograficamente seguro e evita padrões sequenciais
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(chars);
    }
}
