using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

public interface IEncryptionService
{
    byte[] Encrypt(string plainText);
    string Decrypt(byte[] cipherText);
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _nonce; // 12 bytes cho GCM

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        var opts = options.Value;

        // Key phải đúng 32 bytes (256 bit)
        _key = Convert.FromHexString(opts.Key);

        // Nonce cố định hoặc derive từ tenant (ở đây dùng cố định để đơn giản)
        // Nâng cao: có thể dùng random nonce + lưu kèm cipher text
        _nonce = Convert.FromHexString(opts.Nonce);
    }

    public byte[] Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return null;

        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16]; // GCM tag 128 bit

        using var aes = new AesGcm(_key);

        aes.Encrypt(_nonce, plaintextBytes, ciphertext, tag);

        // Kết hợp: nonce (12) + ciphertext + tag (16)
        var result = new byte[_nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(_nonce, 0, result, 0, _nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, _nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, _nonce.Length + ciphertext.Length, tag.Length);

        return result;
    }

    public string Decrypt(byte[] cipherText)
    {
        if (cipherText == null || cipherText.Length < 28) // 12 nonce + 16 tag min
            return null;

        var nonce = new byte[12];
        var tag = new byte[16];
        var ciphertextLength = cipherText.Length - 28;
        var ciphertext = new byte[ciphertextLength];

        Buffer.BlockCopy(cipherText, 0, nonce, 0, 12);
        Buffer.BlockCopy(cipherText, 12, ciphertext, 0, ciphertextLength);
        Buffer.BlockCopy(cipherText, 12 + ciphertextLength, tag, 0, 16);

        var plaintextBytes = new byte[ciphertextLength];

        using var aes = new AesGcm(_key);

        aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}

public class EncryptionOptions
{
    public string Key { get; set; }
    public string Nonce { get; set; }
}
