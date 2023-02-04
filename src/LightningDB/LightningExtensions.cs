using System;
using System.Collections.Generic;
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
        if (resultCode == MDBResultCode.Success)
            return resultCode;
        var statusCode = (int) resultCode;
        var message = mdb_strerror(statusCode);
        throw new LightningException(message, statusCode); 
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
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string mdb_strerror(int err)
    {
        var ptr = Lmdb.mdb_strerror(err);
        return Marshal.PtrToStringAnsi(ptr);
    }

    /// <summary>
    /// Enumerates the key/value pairs of the <see cref="LightningCursor"/> starting at the current position.
    /// </summary>
    /// <param name="cursor"><see cref="LightningCursor"/></param>
    /// <returns><see cref="ValueTuple"/> key/value pairs of <see cref="MDBValue"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ValueTuple<MDBValue, MDBValue>> AsEnumerable(this LightningCursor cursor)
    {
        while(cursor.Next() == MDBResultCode.Success)
        {
            var (resultCode, key, value) = cursor.GetCurrent();
            resultCode.ThrowOnError();
            yield return (key, value);
        }
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
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, byte[] key, out byte[] value)
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
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, out byte[] value)
    {
        var (resultCode, _, mdbValue) = tx.Get(db, key);
        if (resultCode == MDBResultCode.Success)
        {
            value = mdbValue.CopyToNewArray();
            return true;
        }
        value = default;
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