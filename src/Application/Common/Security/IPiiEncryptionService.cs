namespace FinFlow.Application.Common.Security;

/// <summary>
/// Authenticated symmetric encryption for PII fields (bank account number, etc.).
/// Implementation must be authenticated (AEAD) — confidentiality alone is not enough
/// for PII because we also need tamper detection.
///
/// Output of <see cref="Encrypt"/> is opaque to callers; only <see cref="Decrypt"/>
/// of the same implementation can recover the plaintext.
/// </summary>
public interface IPiiEncryptionService
{
    /// <summary>Encrypt a UTF-8 string. Output is self-contained (nonce included).</summary>
    byte[] Encrypt(string plaintext);

    /// <summary>Decrypt a previously encrypted blob. Throws if tampered or wrong key.</summary>
    string Decrypt(byte[] ciphertext);

    /// <summary>
    /// Derive last-4 view from plaintext. Helper so callers don't have to think about
    /// edge cases (short numbers, whitespace).
    /// </summary>
    string MaskLast4(string plaintext);
}
