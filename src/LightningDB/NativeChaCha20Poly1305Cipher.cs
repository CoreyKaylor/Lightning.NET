using System;

namespace LightningDB;

/// <summary>
/// ChaCha20-Poly1305 (RFC 8439) page cipher implemented inside the native LMDB
/// library. Only functional on browser-wasm, where managed page callbacks are
/// unavailable (the .NET WebAssembly runtime cannot marshal delegates as native
/// callbacks) and the platform provides no AEAD cipher (<c>AesGcm</c>,
/// <c>ChaCha20Poly1305</c> and friends all throw). Requires a 32-byte key and
/// reserves a 16-byte Poly1305 tag per page.
/// </summary>
/// <remarks>
/// <para>
/// Experimental: enabling encryption switches LMDB to its chunked page-cache
/// I/O model (<c>MDB_REMAP_CHUNKS</c>; <c>WriteMap</c> is ignored). Under
/// emscripten's in-memory file system this has a known limitation: data
/// committed in one transaction is <b>not visible to later read transactions in
/// the same environment session</b> (the meta-page mapping created at open never
/// reflects subsequent writes). The supported encrypted pattern is
/// write → <c>Flush(force: true)</c> → dispose → reopen → read; reads within
/// the writing transaction itself work normally.
/// </para>
/// <para>
/// On-disk format: pages are encrypted with the IETF ChaCha20 stream starting
/// at block counter 1, with the Poly1305 key derived from block 0 and the MAC
/// computed per RFC 8439 with empty additional data — the same detached-AEAD
/// construction as libsodium's <c>crypto_aead_chacha20poly1305_ietf</c> and
/// upstream LMDB's crypto.c example. The 12-byte nonce is
/// <c>LE32(pgno) || LE32(txnid) || 0x00000000</c> (page number and transaction
/// id are 32-bit in wasm builds).
/// </para>
/// <para>
/// <see cref="Encrypt"/>/<see cref="Decrypt"/> are never invoked from managed
/// code; the crypto runs entirely inside the native library.
/// </para>
/// </remarks>
public sealed class NativeChaCha20Poly1305Cipher : LightningCipher
{
    /// <summary>
    /// The required encryption key length in bytes.
    /// </summary>
    public const int KeySize = 32;

    /// <summary>
    /// Creates the cipher marker. The actual cipher is installed natively when the
    /// environment is created.
    /// </summary>
    public NativeChaCha20Poly1305Cipher() : base(16)
    {
    }

    /// <inheritdoc />
    public override bool Encrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> authData)
        => throw new PlatformNotSupportedException(
            $"{nameof(NativeChaCha20Poly1305Cipher)} executes inside the native library and is only supported on browser-wasm.");

    /// <inheritdoc />
    public override bool Decrypt(ReadOnlySpan<byte> source, Span<byte> destination,
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> authData)
        => throw new PlatformNotSupportedException(
            $"{nameof(NativeChaCha20Poly1305Cipher)} executes inside the native library and is only supported on browser-wasm.");
}
