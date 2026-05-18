using System.Security.Cryptography;
using FinFlow.Application.Common.Abstractions;
using FinFlow.Infrastructure.Security;
using Xunit;

namespace FinFlow.UnitTests.Infrastructure.Security;

public class AesGcmPiiEncryptionServiceTests
{
    private static AesGcmPiiEncryptionService BuildService()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return new AesGcmPiiEncryptionService(new InMemorySecretProvider(("PII_ENCRYPTION_KEY", key)));
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsPlaintext()
    {
        var sut = BuildService();
        var pt = "1234567890";

        var ct = sut.Encrypt(pt);
        var roundTripped = sut.Decrypt(ct);

        Assert.Equal(pt, roundTripped);
    }

    [Fact]
    public void Encrypt_DiacriticsRoundTrip()
    {
        var sut = BuildService();
        var pt = "Nguyễn Văn Á";

        var ct = sut.Encrypt(pt);
        var roundTripped = sut.Decrypt(ct);

        Assert.Equal(pt, roundTripped);
    }

    [Fact]
    public void Encrypt_SamePlaintextProducesDifferentCiphertext()
    {
        var sut = BuildService();
        var pt = "1234567890";

        var ct1 = sut.Encrypt(pt);
        var ct2 = sut.Encrypt(pt);

        Assert.NotEqual(ct1, ct2);   // different nonces
        Assert.Equal(sut.Decrypt(ct1), sut.Decrypt(ct2));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_Throws()
    {
        var sut = BuildService();
        var ct = sut.Encrypt("secret");
        ct[ct.Length - 1] ^= 0xFF;   // flip last byte

        Assert.Throws<AuthenticationTagMismatchException>(() => sut.Decrypt(ct));
    }

    [Fact]
    public void Decrypt_DifferentKey_Throws()
    {
        var key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var enc = new AesGcmPiiEncryptionService(new InMemorySecretProvider(("PII_ENCRYPTION_KEY", key1)));
        var dec = new AesGcmPiiEncryptionService(new InMemorySecretProvider(("PII_ENCRYPTION_KEY", key2)));

        var ct = enc.Encrypt("secret");

        Assert.Throws<AuthenticationTagMismatchException>(() => dec.Decrypt(ct));
    }

    [Fact]
    public void Constructor_MissingKey_ThrowsClearError()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new AesGcmPiiEncryptionService(new InMemorySecretProvider()));

        Assert.Contains("PII_ENCRYPTION_KEY", ex.Message);
    }

    [Fact]
    public void Constructor_NotBase64_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new AesGcmPiiEncryptionService(new InMemorySecretProvider(("PII_ENCRYPTION_KEY", "not-base64!!!"))));

        Assert.Contains("base64", ex.Message);
    }

    [Fact]
    public void Constructor_WrongKeySize_Throws()
    {
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));   // 128-bit
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new AesGcmPiiEncryptionService(new InMemorySecretProvider(("PII_ENCRYPTION_KEY", shortKey))));

        Assert.Contains("32 bytes", ex.Message);
    }

    [Fact]
    public void Decrypt_TooShortCiphertext_Throws()
    {
        var sut = BuildService();

        Assert.Throws<CryptographicException>(() => sut.Decrypt(new byte[5]));
    }

    [Theory]
    [InlineData("1234567890", "****7890")]
    [InlineData("12345", "****2345")]
    [InlineData("1234", "1234")]
    [InlineData("12", "12")]
    [InlineData("", "")]
    [InlineData("  1234  ", "1234")]
    public void MaskLast4_HandlesEdgeCases(string input, string expected)
    {
        var sut = BuildService();

        Assert.Equal(expected, sut.MaskLast4(input));
    }

    private sealed class InMemorySecretProvider : ISecretProvider
    {
        private readonly Dictionary<string, string> _values;

        public InMemorySecretProvider(params (string Key, string Value)[] entries)
        {
            _values = entries.ToDictionary(e => e.Key, e => e.Value);
        }

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_values.TryGetValue(key, out var v) ? v : null);

        public string? Get(string key) => _values.TryGetValue(key, out var v) ? v : null;
    }
}
