using System;

namespace LightningDB;

/// <summary>
/// Encryption settings for an environment: the page cipher and the encryption key.
/// Assign to <see cref="EnvironmentConfiguration.Encryption"/> before the environment is created.
/// </summary>
public sealed class EncryptionConfiguration
{
    /// <summary>
    /// Creates encryption settings for an environment.
    /// </summary>
    /// <param name="cipher">The page cipher, e.g. <c>AesGcmCipher</c></param>
    /// <param name="key">The encryption key; copied defensively. The same key must be
    /// supplied every time the environment is opened.</param>
    public EncryptionConfiguration(LightningCipher cipher, ReadOnlySpan<byte> key)
    {
        Cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));
        if (key.IsEmpty)
            throw new ArgumentException("An encryption key is required", nameof(key));
        Key = key.ToArray();
    }

    /// <summary>
    /// The page cipher.
    /// </summary>
    public LightningCipher Cipher { get; }

    internal byte[] Key { get; }
}
