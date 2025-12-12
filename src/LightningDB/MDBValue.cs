using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LightningDB;

/// <summary>
/// A managed version of the native MDB_val type
/// </summary>
/// <remarks>
/// For Performance and Correctness, the layout of this struct must not be changed.
/// This struct is blittable and is marshalled directly to Native code via
/// P/Invoke.
/// </remarks>
#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
public unsafe struct MDBValue
{
    /// <remarks>
    /// We only expose this shape constructor to basically force you to use
    /// a fixed statement to obtain the pointer. If we accepted a Span or
    /// ReadOnlySpan here, we would have to do scarier things to pin/unpin
    /// the buffer. Since this library is geared towards safe and easy usage,
    /// this way somewhat forces you onto the correct path.
    ///
    /// </remarks>
    /// <param name="bufferSize">The length of the buffer</param>
    /// <param name="pinnedOrStackAllocBuffer">A pointer to a buffer.
    /// The underlying memory may be managed(an array), unmanaged or stack-allocated.
    /// If it is managed, it **MUST** be pinned via either GCHandle.Alloc or a fixed statement
    /// </param>
    internal MDBValue(int bufferSize, byte* pinnedOrStackAllocBuffer)
    {
        size = bufferSize;
        data = pinnedOrStackAllocBuffer;
    }
    //DO NOT REORDER
    internal nint size;

    //DO NOT REORDER
    internal byte* data;

    /// <summary>
    /// Gets a read-only span representation of the buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
    public ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(data, (int)size);
#else
    public ReadOnlySpan<byte> AsSpan() =>
        MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(data), (int)size);
#endif

    /// <summary>
    /// Gets a writable span representation of the buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> AsWritableSpan() => new Span<byte>(data, (int)size);

    /// <summary>
    /// Reads a value of type T from the buffer
    /// </summary>
    /// <typeparam name="T">The unmanaged type to read</typeparam>
    /// <returns>The value read from the buffer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>() where T : unmanaged =>
        Unsafe.ReadUnaligned<T>(data);

    /// <summary>
    /// Casts the buffer to a read-only span of type T
    /// </summary>
    /// <typeparam name="T">The unmanaged type to cast to</typeparam>
    /// <returns>A read-only span of the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> Cast<T>() where T : unmanaged =>
        MemoryMarshal.Cast<byte, T>(AsSpan());

    /// <summary>
    /// Copies the buffer contents to the destination span
    /// </summary>
    /// <param name="destination">The destination span to copy to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<byte> destination) =>
        Unsafe.CopyBlockUnaligned(
            ref MemoryMarshal.GetReference(destination),
            ref Unsafe.AsRef<byte>(data),
            (uint)size);

    /// <summary>
    /// Copies the data of the buffer to a new array
    /// </summary>
    /// <returns>A newly allocated array containing data copied from the de-referenced data pointer</returns>
    /// <remarks>Equivalent to AsSpan().ToArray() but makes intent a little more clear</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] CopyToNewArray() => AsSpan().ToArray();
}
