using System;

namespace LightningDB;

/// <summary>
/// A page checksum for an environment. The configured checksum is computed for every page
/// written and verified for every page read; a mismatch surfaces as
/// <see cref="MDBResultCode.BadChecksum"/>.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe and should avoid per-call allocations; the
/// computation sits on the environment's page I/O hot path.
/// </remarks>
public abstract class LightningChecksum
{
    /// <summary>
    /// The size of computed checksum values in bytes.
    /// </summary>
    public abstract int Size { get; }

    /// <summary>
    /// Computes the checksum of the source data.
    /// </summary>
    /// <param name="source">The data to checksum</param>
    /// <param name="destination">Storage for the checksum, <see cref="Size"/> bytes</param>
    /// <param name="key">The encryption key when encryption is also configured on the
    /// environment (for keyed hashes), otherwise empty</param>
    public abstract void Compute(ReadOnlySpan<byte> source, Span<byte> destination, ReadOnlySpan<byte> key);
}

#if NET8_0_OR_GREATER
/// <summary>
/// The built-in SHA-256 page checksum.
/// </summary>
public sealed class Sha256Checksum : LightningChecksum
{
    public override int Size => 32;

    public override void Compute(ReadOnlySpan<byte> source, Span<byte> destination, ReadOnlySpan<byte> key)
        => System.Security.Cryptography.SHA256.HashData(source, destination);
}
#endif

/// <summary>
/// BLAKE2b-256 page checksum implemented inside the native LMDB library. Only
/// functional on browser-wasm, where managed page callbacks are unavailable
/// (the .NET WebAssembly runtime cannot marshal delegates as native callbacks).
/// When combined with <see cref="NativeChaCha20Poly1305Cipher"/>, the hash is
/// keyed with the encryption key.
/// </summary>
/// <remarks>
/// <see cref="Compute"/> is never invoked from managed code; the hashing runs
/// entirely inside the native library.
/// </remarks>
public sealed class NativeBlake2bChecksum : LightningChecksum
{
    public override int Size => 32;

    public override void Compute(ReadOnlySpan<byte> source, Span<byte> destination, ReadOnlySpan<byte> key)
        => throw new PlatformNotSupportedException(
            $"{nameof(NativeBlake2bChecksum)} executes inside the native library and is only supported on browser-wasm.");
}
