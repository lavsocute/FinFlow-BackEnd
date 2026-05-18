using System.Security.Cryptography;
using System.Text;
using FinFlow.Application.Common.Abstractions;
using FinFlow.Application.Common.Security;

namespace FinFlow.Infrastructure.Security;

/// <summary>
/// AES-256-GCM authenticated encryption for PII at rest. Key sourced from
/// <see cref="ISecretProvider"/> under the key <c>PII_ENCRYPTION_KEY</c>.
///
/// Output layout: <c>[nonce(12) | tag(16) | ciphertext(N)]</c>. Self-contained so
/// rotating to a new <see cref="AesGcmPiiEncryptionService"/> with a different key
/// only requires the old key to remain available for decrypt during migration.
/// </summary>
internal sealed class AesGcmPiiEncryptionService : IPiiEncryptionService
{
    public const string KeyConfigName = "PII_ENCRYPTION_KEY";

    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;

    private readonly byte[] _key;

    public AesGcmPiiEncryptionService(ISecretProvider secrets)
    {
        var keyB64 = secrets.Get(KeyConfigName);
        if (string.IsNullOrWhiteSpace(keyB64))
        {
            throw new InvalidOperationException(
                $"{KeyConfigName} is not configured. " +
                "Generate a 32-byte key (e.g. `openssl rand -base64 32`) and set the env var.");
        }

        try
        {
            _key = Convert.FromBase64String(keyB64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                $"{KeyConfigName} must be base64-encoded.");
        }

        if (_key.Length != KeySize)
        {
            throw new InvalidOperationException(
                $"{KeyConfigName} must decode to {KeySize} bytes (256-bit). Got {_key.Length}.");
        }
    }

    public byte[] Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var ptBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ct = new byte[ptBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, ptBytes, ct, tag);

        var output = new byte[NonceSize + TagSize + ct.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
        Buffer.BlockCopy(ct, 0, output, NonceSize + TagSize, ct.Length);
        return output;
    }

    public string Decrypt(byte[] ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (ciphertext.Length < NonceSize + TagSize)
            throw new CryptographicException("Ciphertext too short to contain nonce + tag.");

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var ct = new byte[ciphertext.Length - NonceSize - TagSize];
        Buffer.BlockCopy(ciphertext, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(ciphertext, NonceSize + TagSize, ct, 0, ct.Length);

        var pt = new byte[ct.Length];
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ct, tag, pt);
        return Encoding.UTF8.GetString(pt);
    }

    public string MaskLast4(string plaintext)
    {
        var trimmed = plaintext?.Trim() ?? string.Empty;
        return trimmed.Length <= 4 ? trimmed : $"****{trimmed[^4..]}";
    }
}
