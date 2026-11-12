using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB;

public static class LightningExtensions
{
    /// <summary>
    /// Throws a <see cref="LightningException"/> on anything other than NotFound, or Success
    /// </summary>
    /// <param name="resultCode">The result code to evaluate for errors</param>
    /// <returns><see cref="MDBResultCode"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MDBResultCode ThrowOnReadError(this MDBResultCode resultCode)
    {
        return resultCode == MDBResultCode.NotFound 
            ? resultCode : resultCode.ThrowOnError();
    }

    /// <summary>
    /// Throws a <see cref="LightningException"/> on anything other than Success
    /// </summary>
    /// <param name="resultCode">The result code to evaluate for errors</param>
    /// <returns><see cref="MDBResultCode"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MDBResultCode ThrowOnError(this MDBResultCode resultCode)
    {
        if (resultCode != MDBResultCode.Success)
            ThrowLightningException(resultCode);
        return resultCode;
    }

    // Kept out of line so the inlined success check at every call site stays tiny.
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    private static void ThrowLightningException(MDBResultCode resultCode)
    {
        var statusCode = (int) resultCode;
        throw new LightningException(mdb_strerror(statusCode), statusCode);
    }

    /// <summary>
    /// Throws a <see cref="LightningException"/> on anything other than NotFound, or Success 
    /// </summary>
    /// <param name="result">A <see cref="ValueTuple"/> representing the get result operation</param>
    /// <returns>The provided <see cref="ValueTuple"/> if no error occurs</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (MDBResultCode resultCode, MDBValue key, MDBValue value) ThrowOnReadError(
        this ValueTuple<MDBResultCode, MDBValue, MDBValue> result)
    {
        result.Item1.ThrowOnReadError();
        return result;
    }
        
    private static string mdb_strerror(int err)
    {
        var ptr = Lmdb.mdb_strerror(err);
        return Marshal.PtrToStringAnsi(ptr) ?? $"Unknown error {err}";
    }

    /// <summary>
    /// Enumerates the key/value pairs of the <see cref="LightningCursor"/> starting at the current position.
    /// </summary>
    /// <param name="cursor"><see cref="LightningCursor"/></param>
    /// <returns><see cref="ValueTuple"/> key/value pairs of <see cref="MDBValue"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CursorEnumerable AsEnumerable(this LightningCursor cursor)
    {
        return new CursorEnumerable(cursor);
    }

    /// <summary>
    /// Enumerates the values for a given key. Requires MDB_DUPSORT
    /// </summary>
    /// <param name="cursor"><see cref="LightningCursor"/></param>
    /// <param name="key">The key with multiple values</param>
    /// <returns><see cref="MDBValue"/> representing each value for a given key</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CursorDuplicateValuesEnumerable AllValuesFor(this LightningCursor cursor, byte[] key)
    {
        return cursor.AllValuesFor(key.AsSpan());
    }

    /// <summary>
    /// Enumerates the values for a given key. Requires MDB_DUPSORT
    /// </summary>
    /// <param name="cursor"><see cref="LightningCursor"/></param>
    /// <param name="key">The key with multiple values</param>
    /// <returns><see cref="MDBValue"/> representing each value for a given key</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CursorDuplicateValuesEnumerable AllValuesFor(this LightningCursor cursor, ReadOnlySpan<byte> key)
    {
        var result = cursor.Set(key);
        result.ThrowOnReadError();
        return new CursorDuplicateValuesEnumerable(cursor);
    }

    /// <summary>
    /// Tries to get a value by its key.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="value">A byte array containing the value found in the database, if it exists.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, byte[] key, out byte[]? value)
    {
        return TryGet(tx, db, key.AsSpan(), out value);
    }

    /// <summary>
    /// Tries to get a value by its key.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="value">A byte array containing the value found in the database, if it exists.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, out byte[]? value)
    {
        var (resultCode, _, mdbValue) = tx.Get(db, key);
        if (resultCode == MDBResultCode.Success)
        {
            value = mdbValue.CopyToNewArray();
            return true;
        }
        value = null;
        return false;
    }
        
    /// <summary>
    /// Tries to get a value by its key.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="destinationValueBuffer">
    /// A buffer to receive the value data retrieved from the database
    /// </param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, byte[] destinationValueBuffer)
    {
        var (resultCode, _, mdbValue) = tx.Get(db, key);
        if (resultCode != MDBResultCode.Success) 
            return false;
            
        var valueSpan = mdbValue.AsSpan();
        if (valueSpan.TryCopyTo(destinationValueBuffer))
        {
            return true;
        }
        throw new LightningException("Incorrect buffer size given in destinationValueBuffer", (int)MDBResultCode.BadValSize);
    }

    /// <summary>
    /// Tries to get a value by its key, copying it into a caller-owned buffer.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="destination">The buffer to receive the value data.</param>
    /// <param name="valueLength">
    /// The length of the stored value. When the method returns false, a non-zero valueLength means the key
    /// exists but <paramref name="destination"/> was too small (valueLength is the required size); zero means
    /// the key was not found.
    /// </param>
    /// <returns>True if the key exists and the value was copied to <paramref name="destination"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, Span<byte> destination, out int valueLength)
    {
        var (resultCode, _, mdbValue) = tx.Get(db, key);
        if (resultCode != MDBResultCode.Success)
        {
            valueLength = 0;
            return false;
        }

        var valueSpan = mdbValue.AsSpan();
        valueLength = valueSpan.Length;
        return valueSpan.TryCopyTo(destination);
    }

    /// <summary>
    /// Tries to get a value by its key, copying it into the provided <see cref="IBufferWriter{T}"/>.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="destination">The writer to receive the value data.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, IBufferWriter<byte> destination)
    {
        var (resultCode, _, mdbValue) = tx.Get(db, key);
        if (resultCode != MDBResultCode.Success)
            return false;

        var valueSpan = mdbValue.AsSpan();
        valueSpan.CopyTo(destination.GetSpan(valueSpan.Length));
        destination.Advance(valueSpan.Length);
        return true;
    }

    /// <summary>
    /// Check whether data exists in database.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key)
    {
        var (resultCode, _, _) = tx.Get(db, key);
        return resultCode == MDBResultCode.Success;
    }
        
    /// <summary>
    /// Check whether data exists in database.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey(this LightningTransaction tx, LightningDatabase db, byte[] key)
    {
        return ContainsKey(tx, db, key.AsSpan());
    }
}