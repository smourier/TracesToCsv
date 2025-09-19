namespace TracesToCsv.Utilities;

public static class CryptoUtilities
{
    private static readonly byte[] _iv =
    [
        0xde, 0xad, 0xbe, 0xef, 0xca, 0xfe, 0xba, 0xbe,
        0xba, 0xad, 0xf0, 0x0d, 0x4b, 0x1d, 0x51, 0x66
    ];

    public static string EncryptToString(string clearText, string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (string.IsNullOrWhiteSpace(clearText))
            return string.Empty;

        // https://www.rfc-editor.org/rfc/rfc4648.txt
        return Convert.ToBase64String(Encrypt(clearText, password)).Replace('+', '-').Replace('/', '_');
    }

    public unsafe static string? Decrypt(string base64EncryptedText, string password, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (!string.IsNullOrWhiteSpace(base64EncryptedText))
        {
            // https://www.rfc-editor.org/rfc/rfc4648.txt
            Span<byte> bytes = stackalloc byte[maxSize];
            if (Convert.TryFromBase64String(base64EncryptedText.Replace('-', '+').Replace('_', '/'), bytes, out var written))
                return Decrypt(bytes[..written].ToArray(), password);
        }
        return null;
    }

    public static byte[] Encrypt(string clearText, string password)
    {
        ArgumentNullException.ThrowIfNull(clearText);
        ArgumentNullException.ThrowIfNull(password);

        using var aes = Aes.Create();
        aes.Key = Pbkdf2(password);
        aes.IV = _iv;
        using var ms = new MemoryStream();
        using var crypto = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        crypto.Write(Encoding.UTF8.GetBytes(clearText));
        crypto.FlushFinalBlock();
        return ms.ToArray();
    }

    public static string? Decrypt(byte[] encrypted, string password)
    {
        ArgumentNullException.ThrowIfNull(encrypted);
        ArgumentNullException.ThrowIfNull(password);

        using var aes = Aes.Create();
        aes.Key = Pbkdf2(password);
        aes.IV = _iv;
        using var input = new MemoryStream(encrypted);
        try
        {
            using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var output = new MemoryStream();
            crypto.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }
        catch
        {
            return null;
        }
    }

    // config is a bit down but we mostly care about perf, not super high security
    private unsafe static byte[] Pbkdf2(string password) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password).AsSpan(), [], 100, HashAlgorithmName.SHA256, 16);
}
