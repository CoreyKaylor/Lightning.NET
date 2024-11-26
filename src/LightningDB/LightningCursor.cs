using System;
using System.Linq;
using System.Runtime.CompilerServices;

using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
/// Cursor to iterate over a database
/// </summary>
public class LightningCursor : IDisposable
{
    private nint _handle;
    private bool _disposed = false;

    /// <summary>
    /// Creates new instance of LightningCursor
    /// </summary>
    /// <param name="db">Database</param>
    /// <param name="txn">Transaction</param>
    internal LightningCursor(LightningDatabase db, LightningTransaction txn)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        if (txn == null)
            throw new ArgumentNullException(nameof(txn));

        mdb_cursor_open(txn._handle, db._handle, out _handle).ThrowOnError();

        Database = db;
        Transaction = txn;
    }

    /// <summary>
    /// Gets the native handle of the cursor
    /// </summary>
    public nint Handle()
    {
        return _handle;
    }

    /// <summary>
    /// Cursor's transaction.
    /// </summary>
    public LightningTransaction Transaction { get; }
    /// <summary>
    /// The database that the cursor is associated with.
    /// </summary>
    public LightningDatabase Database { get; }

    /// <summary>
    /// Position at specified key, if key is not found index will be positioned to closest match.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Set(byte[] key)
    {
        return Get(CursorOperation.Set, key).resultCode;
    }
        
    /// <summary>
    /// Position at specified key, if key is not found index will be positioned to closest match.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Set(ReadOnlySpan<byte> key)
    {
        return Get(CursorOperation.Set, key).resultCode;
    }

    /// <summary>
    /// Moves to the key and populates Current with the values stored.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) SetKey(byte[] key)
    {
        return Get(CursorOperation.SetKey, key);
    }
        
    /// <summary>
    /// Moves to the key and populates Current with the values stored.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) SetKey(ReadOnlySpan<byte> key)
    {
        return Get(CursorOperation.SetKey, key);
    }

    /// <summary>
    /// Position at key/data pair. Only for MDB_DUPSORT
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value</param>
    /// <returns>Returns true if the key/value pair was found.</returns>
    public MDBResultCode GetBoth(byte[] key, byte[] value)
    {
        return Get(CursorOperation.GetBoth, key, value).resultCode;
    }
        
    /// <summary>
    /// Position at key/data pair. Only for MDB_DUPSORT
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode GetBoth(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        return Get(CursorOperation.GetBoth, key, value).resultCode;
    }

    /// <summary>
    /// position at key, nearest data. Only for MDB_DUPSORT
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode GetBothRange(byte[] key, byte[] value)
    {
        return Get(CursorOperation.GetBothRange, key, value).resultCode;
    }
        
    /// <summary>
    /// position at key, nearest data. Only for MDB_DUPSORT
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode GetBothRange(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        return Get(CursorOperation.GetBothRange, key, value).resultCode;
    }

    /// <summary>
    /// Position at first key greater than or equal to specified key.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode SetRange(byte[] key)
    {
        return Get(CursorOperation.SetRange, key).resultCode;
    }
        
    /// <summary>
    /// Position at first key greater than or equal to specified key.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode SetRange(ReadOnlySpan<byte> key)
    {
        return Get(CursorOperation.SetRange, key).resultCode;
    }

    /// <summary>
    /// Position at first key/data item
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) First()
    {
        return Get(CursorOperation.First);
    }

    /// <summary>
    /// Position at first data item of current key. Only for MDB_DUPSORT
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) FirstDuplicate()
    {
        return Get(CursorOperation.FirstDuplicate);
    }

    /// <summary>
    /// Position at last key/data item
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) Last()
    {
        return Get(CursorOperation.Last);
    }

    /// <summary>
    /// Position at last data item of current key. Only for MDB_DUPSORT
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) LastDuplicate()
    {
        return Get(CursorOperation.LastDuplicate);
    }

    /// <summary>
    /// Return key/data at current cursor position
    /// </summary>
    /// <returns>Key/data at current cursor position</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) GetCurrent()
    {
        return Get(CursorOperation.GetCurrent);
    }

    /// <summary>
    /// Position at next data item
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) Next()
    {
        return Get(CursorOperation.Next);
    }

    /// <summary>
    /// Position at next data item of current key. Only for MDB_DUPSORT
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) NextDuplicate()
    {
        return Get(CursorOperation.NextDuplicate);
    }

    /// <summary>
    /// Position at first data item of next key. Only for MDB_DUPSORT.
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) NextNoDuplicate()
    {
        return Get(CursorOperation.NextNoDuplicate);
    }

    /// <summary>
    /// Return up to a page of duplicate data items at the next cursor position. Only for MDB_DUPFIXED
    /// It is assumed you know the array size to break up a single byte[] into byte[][].
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key will be empty here, values are 2D array</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) NextMultiple()
    {
        return Get(CursorOperation.NextMultiple);
    }

    /// <summary>
    /// Position at previous data item.
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) Previous()
    {
        return Get(CursorOperation.Previous);
    }

    /// <summary>
    /// Position at previous data item of current key. Only for MDB_DUPSORT.
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) PreviousDuplicate()
    {
        return Get(CursorOperation.PreviousDuplicate);
    }

    /// <summary>
    /// Position at last data item of previous key. Only for MDB_DUPSORT.
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key/value</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) PreviousNoDuplicate()
    {
        return Get(CursorOperation.PreviousNoDuplicate);
    }
        
    private (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation)
    {
        var mdbKey = new MDBValue();
        var mdbValue = new MDBValue();
        return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
    }

    private (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, byte[] key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        return Get(operation, key.AsSpan());
    }

    private unsafe (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, ReadOnlySpan<byte> key)
    {
        fixed (byte* keyPtr = key)
        {
            var mdbKey = new MDBValue(key.Length, keyPtr);
            var mdbValue = new MDBValue();
            return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
        }
    }

    private (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, byte[] key, byte[] value)
    {
        return Get(operation, key.AsSpan(), value.AsSpan());
    }

    private unsafe (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        fixed(byte* keyPtr = key)
        fixed(byte* valPtr = value)
        {
            var mdbKey = new MDBValue(key.Length, keyPtr);
            var mdbValue = new MDBValue(value.Length, valPtr);
            return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
        }
    }

    /// <summary>
    /// Store by cursor.
    /// This function stores key/data pairs into the database. The cursor is positioned at the new item, or on failure usually near it.
    /// Note: Earlier documentation incorrectly said errors would leave the state of the cursor unchanged.
    /// If the function fails for any reason, the state of the cursor will be unchanged. 
    /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
    /// </summary>
    /// <param name="key">The key operated on.</param>
    /// <param name="value">The data operated on.</param>
    /// <param name="options">
    /// Options for this operation. This parameter must be set to 0 or one of the values described here.
    ///     CursorPutOptions.Current - overwrite the data of the key/data pair to which the cursor refers with the specified data item. The key parameter is ignored.
    ///     CursorPutOptions.NoDuplicateData - enter the new key/data pair only if it does not already appear in the database. This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST if the key/data pair already appears in the database.
    ///     CursorPutOptions.NoOverwrite - enter the new key/data pair only if the key does not already appear in the database. The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports duplicates (MDB_DUPSORT).
    ///     CursorPutOptions.ReserveSpace - reserve space for data of the given size, but don't copy the given data. Instead, return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data is being generated later.
    ///     CursorPutOptions.AppendData - append the given key/data pair to the end of the database. No key comparisons are performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading unsorted keys with this flag will cause data corruption.
    ///     CursorPutOptions.AppendDuplicateData - as above, but for sorted dup data.
    /// </param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Put(byte[] key, byte[] value, CursorPutOptions options)
    {
        return Put(key.AsSpan(), value.AsSpan(), options);
    }


    /// <summary>
    /// Store by cursor.
    /// This function stores key/data pairs into the database. The cursor is positioned at the new item, or on failure usually near it.
    /// Note: Earlier documentation incorrectly said errors would leave the state of the cursor unchanged.
    /// If the function fails for any reason, the state of the cursor will be unchanged. 
    /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
    /// </summary>
    /// <param name="key">The key operated on.</param>
    /// <param name="value">The data operated on.</param>
    /// <param name="options">
    /// Options for this operation. This parameter must be set to 0 or one of the values described here.
    ///     CursorPutOptions.Current - overwrite the data of the key/data pair to which the cursor refers with the specified data item. The key parameter is ignored.
    ///     CursorPutOptions.NoDuplicateData - enter the new key/data pair only if it does not already appear in the database. This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST if the key/data pair already appears in the database.
    ///     CursorPutOptions.NoOverwrite - enter the new key/data pair only if the key does not already appear in the database. The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports duplicates (MDB_DUPSORT).
    ///     CursorPutOptions.ReserveSpace - reserve space for data of the given size, but don't copy the given data. Instead, return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data is being generated later.
    ///     CursorPutOptions.AppendData - append the given key/data pair to the end of the database. No key comparisons are performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading unsorted keys with this flag will cause data corruption.
    ///     CursorPutOptions.AppendDuplicateData - as above, but for sorted dup data.
    /// </param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public unsafe MDBResultCode Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, CursorPutOptions options)
    {
        fixed (byte* keyPtr = key)
        fixed (byte* valPtr = value)
        {
            var mdbKey = new MDBValue(key.Length, keyPtr);
            var mdbValue = new MDBValue(value.Length, valPtr);

            return mdb_cursor_put(_handle, mdbKey, mdbValue, options);
        }
    }

    /// <summary>
    /// Store by cursor.
    /// This function stores key/data pairs into the database. 
    /// If the function fails for any reason, the state of the cursor will be unchanged. 
    /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
    /// </summary>
    /// <param name="key">The key operated on.</param>
    /// <param name="values">The data items operated on.</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public unsafe MDBResultCode Put(byte[] key, byte[][] values)
    {
        const int StackAllocateLimit = 256;//I just made up a number, this can be much more aggressive -arc

        var overallLength = values.Sum(arr => arr.Length);//probably allocates but boy is it handy...


        //the idea here is to gain some perf by stackallocating the buffer to 
        //hold the contiguous keys
        if (overallLength < StackAllocateLimit)
        {
            Span<byte> contiguousValues = stackalloc byte[overallLength];

            return InnerPutMultiple(contiguousValues);
        }

        fixed (byte* contiguousValuesPtr = new byte[overallLength])
        {
            var contiguousValues = new Span<byte>(contiguousValuesPtr, overallLength);
            return InnerPutMultiple(contiguousValues);
        }

        //these local methods could be made static, but the compiler will emit these closures
        //as structs with very little overhead. Also static local functions isn't available 
        //until C# 8 so I can't use it anyway...
        MDBResultCode InnerPutMultiple(Span<byte> contiguousValuesBuffer)
        {
            FlattenInfo(contiguousValuesBuffer);
            var contiguousValuesPtr = (byte*)Unsafe.AsPointer(ref contiguousValuesBuffer.GetPinnableReference());

            var mdbValue = new MDBValue(GetSize(), contiguousValuesPtr);
            var mdbCount = new MDBValue(values.Length, null);

            Span<MDBValue> dataBuffer = stackalloc MDBValue[2] { mdbValue, mdbCount };

            fixed (byte* keyPtr = key)
            {
                var mdbKey = new MDBValue(key.Length, keyPtr);

                return mdb_cursor_put(_handle, ref mdbKey, ref dataBuffer, CursorPutOptions.MultipleData);
            }
        }

        void FlattenInfo(Span<byte> targetBuffer)
        {
            var cursor = targetBuffer;

            foreach(var buffer in values)
            {
                buffer.CopyTo(cursor);
                cursor = cursor.Slice(buffer.Length);
            }
        }

        int GetSize() 
        {
            if (values.Length == 0 || values[0] == null || values[0].Length == 0)
                return 0;

            return values[0].Length;
        }
    }

    /// <summary>
    /// Return up to a page of the duplicate data items at the current cursor position. Only for MDB_DUPFIXED
    /// It is assumed you know the array size to break up a single byte[] into byte[][].
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/>, and <see cref="MDBValue"/> key will be empty here, values are 2D array</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) GetMultiple()
    {
        return Get(CursorOperation.GetMultiple);
    }

    /// <summary>
    /// Delete current key/data pair.
    /// This function deletes the key/data pair to which the cursor refers.
    /// </summary>
    /// <param name="option">Options for this operation. This parameter must be set to 0 or one of the values described here.
    ///     MDB_NODUPDATA - delete all of the data items for the current key. This flag may only be specified if the database was opened with MDB_DUPSORT.</param>
    private MDBResultCode Delete(CursorDeleteOption option)
    {
        return mdb_cursor_del(_handle, option);
    }

    /// <summary>
    /// Delete current key/data pair.
    /// This function deletes the key/data range for which duplicates are found.
    /// </summary>
    public MDBResultCode DeleteDuplicateData()
    {
        return Delete(CursorDeleteOption.NoDuplicateData);
    }

    /// <summary>
    /// Delete current key/data pair.
    /// This function deletes the key/data pair to which the cursor refers.
    /// </summary>
    public MDBResultCode Delete()
    {
        return Delete(CursorDeleteOption.None);
    }

    /// <summary>
    /// Renew a cursor handle.
    /// Cursors are associated with a specific transaction and database and may not span threads. 
    /// Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead. 
    /// The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was created with.
    /// </summary>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Renew()
    {
        return Renew(Transaction);
    }

    /// <summary>
    /// Renew a cursor handle.
    /// Cursors are associated with a specific transaction and database and may not span threads. 
    /// Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead. 
    /// The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was created with.
    /// </summary>
    /// <param name="txn">Transaction to renew in.</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Renew(LightningTransaction txn)
    {
        if(txn == null)
            throw new ArgumentNullException(nameof(txn));

        if (!txn.IsReadOnly)
            throw new InvalidOperationException("Can't renew cursor on non-readonly transaction");

        return mdb_cursor_renew(txn._handle, _handle);
    }
    
    /// <summary>
    /// Return count of duplicates for current key.
    /// 
    /// This call is only valid on databases that support sorted duplicate data items DatabaseOpenFlags.DuplicatesFixed. 
    /// </summary>
    /// <param name="value">Output parameter where the duplicate count will be stored.</param>
    /// <returns>Returns <see cref="MDBResultCode"/></returns>
    public MDBResultCode Count(out int value)
    {
        return mdb_cursor_count(_handle, out value);
    }

    private bool ShouldCloseCursor()
    {
        return Database.IsOpened &&
               Transaction.State == LightningTransactionState.Ready;
    }

    private bool CheckReadOnly()
    {
        return Transaction.IsReadOnly && Database.IsOpened &&
               Transaction.State != LightningTransactionState.Ready;
    }

    /// <summary>
    /// Closes the cursor and deallocates all resources associated with it.
    /// </summary>
    /// <param name="disposing">True if called from Dispose.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        if (CheckReadOnly() && !disposing)
        {
            throw new InvalidOperationException("The LightningCursor in a read-only transaction must be disposed explicitly.");
        }
        
        if (ShouldCloseCursor())
        {
            mdb_cursor_close(_handle);
        }
        _handle = default;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// A cursor in a write-transaction can be closed before its transaction ends,
    /// and will otherwise be closed when its transaction ends. A cursor in a read-only transaction must be closed explicitly,
    /// before or after its transaction ends. It can be reused with Transaction.Renew() before finally closing it.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    ~LightningCursor()
    {
        Dispose(false);
    }
}