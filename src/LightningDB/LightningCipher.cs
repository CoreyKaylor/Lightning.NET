using System;

namespace LightningDB;

/// <summary>
/// A page cipher for an encrypted environment. LMDB stores no cipher itself; the
/// configured cipher is invoked for every page written to or read from the data file.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe: pages may be decrypted concurrently from multiple
/// reader threads. Implementations should avoid per-call allocations; both methods sit on
/// the environment's page I/O hot path. The transform must produce exactly
/// <c>src.Length</c> output bytes; authentication data (e.g. an AEAD tag) goes in
/// <c>authData</c>, sized by <see cref="AuthDataSize"/>.
/// </remarks>
public abstract class LightningCipher
{
    /// <summary>
    /// Initializes the cipher.
    /// </summary>
    /// <param name="authDataSize">Bytes of per-page authentication data (e.g. 16 for an
    /// AES-GCM tag); zero for unauthenticated ciphers</param>
    protected LightningCipher(int authDataSize)
    {
        if (authDataSize < 0)
            throw new ArgumentOutOfRangeException(nameof(authDataSize));
        AuthDataSize = authDataSize;
    }

    /// <summary>
    /// Bytes of per-page authentication data reserved at the end of every page.
    /// </summary>
    public int AuthDataSize { get; }

    /// <summary>
    /// Encrypts a page.
    /// </summary>
    /// <param name="source">The plaintext page data</param>
    /// <param name="destination">Storage for the ciphertext; exactly <c>source.Length</c> bytes</param>
    /// <param name="key">The encryption key configured on the environment</param>
    /// <param name="iv">The per-page initialization vector supplied by LMDB
    /// (the page number followed by the transaction id; unique per page write)</param>
    /// <param name="authData">Storage for authentication data, <see cref="AuthDataSize"/> bytes</param>
    /// <returns>True on success, false on failure</returns>
    public abstract bool Encrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> authData);

    /// <summary>
    /// Decrypts a page.
    /// </summary>
    /// <param name="source">The ciphertext page data</param>
    /// <param name="destination">Storage for the plaintext; exactly <c>source.Length</c> bytes</param>
    /// <param name="key">The encryption key configured on the environment</param>
    /// <param name="iv">The per-page initialization vector supplied by LMDB</param>
    /// <param name="authData">The authentication data stored with the page,
    /// <see cref="AuthDataSize"/> bytes</param>
    /// <returns>True on success, false on failure (including authentication failure)</returns>
    public abstract bool Decrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> authData);
}
