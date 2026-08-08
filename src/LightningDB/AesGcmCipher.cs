#if NET8_0_OR_GREATER
using System;
using System.Security.Cryptography;
using System.Threading;

namespace LightningDB;

/// <summary>
/// The built-in AES-256-GCM page cipher. AES-GCM is hardware-accelerated (AES-NI / ARMv8
/// crypto extensions) on all supported platforms except browser-wasm, making it the
/// recommended default for encrypted environments.
/// </summary>
/// <remarks>
/// The 12-byte GCM nonce is derived from LMDB's per-page IV (page number + transaction id,
/// unique per page write) and the 16-byte tag is stored as the page's authentication data.
/// Not available on platforms where <see cref="AesGcm.IsSupported"/> is false (e.g.
/// browser-wasm); supply a custom <see cref="LightningCipher"/> there.
/// </remarks>
public sealed class AesGcmCipher : LightningCipher, IDisposable
{
    private const int TagSize = 16;
    private const int NonceSize = 12;

    //AesGcm instance methods aren't documented as thread-safe, and pages decrypt
    //concurrently from reader threads, so each thread gets its own instance
    private readonly ThreadLocal<AesGcm> _aes;

    public AesGcmCipher() : base(TagSize)
    {
        if (!AesGcm.IsSupported)
            throw new PlatformNotSupportedException(
                "AES-GCM is not supported on this platform; supply a custom LightningCipher instead.");
        _aes = new ThreadLocal<AesGcm>(trackAllValues: true);
    }

    public override bool Encrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> authData)
    {
        Span<byte> nonce = stackalloc byte[NonceSize];
        FillNonce(iv, nonce);
        GetAes(key).Encrypt(nonce, source, destination, authData);
        return true;
    }

    public override bool Decrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> authData)
    {
        Span<byte> nonce = stackalloc byte[NonceSize];
        FillNonce(iv, nonce);
        try
        {
            GetAes(key).Decrypt(nonce, source, authData, destination);
            return true;
        }
        catch (AuthenticationTagMismatchException)
        {
            return false;
        }
    }

    private AesGcm GetAes(ReadOnlySpan<byte> key)
    {
        var aes = _aes.Value;
        if (aes == null)
        {
            aes = new AesGcm(key, TagSize);
            _aes.Value = aes;
        }
        return aes;
    }

    private static void FillNonce(ReadOnlySpan<byte> iv, Span<byte> nonce)
    {
        //the IV is 16 bytes on 64-bit platforms and 8 on 32-bit; take what fits
        //and zero-pad the remainder
        if (iv.Length >= nonce.Length)
        {
            iv[..nonce.Length].CopyTo(nonce);
        }
        else
        {
            nonce.Clear();
            iv.CopyTo(nonce);
        }
    }

    public void Dispose()
    {
        foreach (var aes in _aes.Values)
            aes.Dispose();
        _aes.Dispose();
    }
}
#endif
