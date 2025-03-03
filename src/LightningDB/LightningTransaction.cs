using System;

using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
/// Represents a transaction in the LightningDB environment.
/// Provides methods for managing database operations within the scope of a transaction, including
/// database access, key-value storage, and transaction control (commit, abort, etc.).
/// </summary>
/// <remarks>
/// A transaction and its cursors must only be used by a single thread, and a thread may only 
/// have a single transaction at a time (except for read-only transactions when MDB_NOTLS is in use).
/// 
/// Transactions can be nested to any level. When a parent transaction has active child transactions,
/// it and its cursors may not issue any operations other than Commit and Abort.
/// 
/// Possible errors when creating transactions include:
/// - MDB_PANIC: A fatal error occurred earlier and the environment must be shut down
/// - MDB_MAP_RESIZED: Another process wrote data beyond this environment's mapsize and the map must be resized
/// - MDB_READERS_FULL: A read-only transaction was requested but the reader lock table is full
/// - ENOMEM: Out of memory
/// 
/// Cursors created within a transaction cannot span across transactions.
/// </remarks>
public sealed class LightningTransaction : IDisposable
{
    /// <summary>
    /// Default options used to begin new transactions.
    /// </summary>
    public const TransactionBeginFlags DefaultTransactionBeginFlags = TransactionBeginFlags.None;

    internal nint _handle;
    private readonly nint _originalHandle;
    private bool _disposed;

    /// <summary>
    /// Created new instance of LightningTransaction
    /// </summary>
    /// <param name="environment">Environment.</param>
    /// <param name="parent">Parent transaction or null.</param>
    /// <param name="flags">Transaction open options.</param>
    internal LightningTransaction(LightningEnvironment environment, LightningTransaction parent, TransactionBeginFlags flags)
    {
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        ParentTransaction = parent;
        IsReadOnly = flags == TransactionBeginFlags.ReadOnly;
        State = LightningTransactionState.Ready;

        var parentHandle = parent?._handle ?? default(nint);
        mdb_txn_begin(environment._handle, parentHandle, flags, out _handle).ThrowOnError();
        _originalHandle = _handle;
    }

    /// <summary>
    /// Current transaction state.
    /// </summary>
    public LightningTransactionState State { get; private set; }

    /// <summary>
    /// Begin a child transaction.
    /// </summary>
    /// <param name="beginFlags">Options for a new transaction.</param>
    /// <returns>New child transaction.</returns>
    public LightningTransaction BeginTransaction(TransactionBeginFlags beginFlags = DefaultTransactionBeginFlags)
    {
        return new LightningTransaction(Environment, this, beginFlags);
    }

    /// <summary>
    /// Opens a database in context of this transaction.
    /// </summary>
    /// <param name="name">Database name (optional). If null then the default name is used.</param>
    /// <param name="configuration">Database open options.</param>
    /// <param name="closeOnDispose">Close database handle on dispose</param>
    /// <returns>Created database wrapper.</returns>
    public LightningDatabase OpenDatabase(string name = null, DatabaseConfiguration configuration = null, bool closeOnDispose = false)
    {
        configuration ??= new DatabaseConfiguration();
        var db = new LightningDatabase(name, this, configuration, closeOnDispose);
        return db;
    }

    /// <summary>
    /// Drops the database.
    /// </summary>
    public MDBResultCode DropDatabase(LightningDatabase database)
    {
        return database.Drop(this);
    }

    /// <summary>
    /// Truncates all data from the database.
    /// </summary>
    public MDBResultCode TruncateDatabase(LightningDatabase database)
    {
        return database.Truncate(this);
    }

    /// <summary>
    /// Create a cursor.
    /// Cursors are associated with a specific transaction and database and may not span threads.
    /// </summary>
    /// <param name="db">A database.</param>
    public LightningCursor CreateCursor(LightningDatabase db)
    {
        return new LightningCursor(db, this);
    }

    /// <summary>
    /// Get value from a database.
    /// </summary>
    /// <param name="db">The database to query.</param>
    /// <param name="key">An array containing the key to look up.</param>
    /// <returns>Requested value's byte array if exists, or null if not.</returns>
    public (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(LightningDatabase db, byte[] key)
    {//argument validation delegated to next call

        return Get(db, key.AsSpan());
    }

    /// <summary>
    /// Get value from a database.
    /// </summary>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <returns>Requested value's byte array if exists, or null if not.</returns>
    public unsafe (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(LightningDatabase db, ReadOnlySpan<byte> key)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        fixed(byte* keyBuffer = key)
        {
            var mdbKey = new MDBValue(key.Length, keyBuffer);

            return (mdb_get(_handle, db._handle, ref mdbKey, out var mdbValue), mdbKey, mdbValue);
        }
    }

    /// <summary>
    /// Put data into a database.
    /// </summary>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="value">A byte array containing the value found in the database, if it exists.</param>
    /// <param name="options">Operation options (optional).</param>
    public MDBResultCode Put(LightningDatabase db, byte[] key, byte[] value, PutOptions options = PutOptions.None)
    {//argument validation delegated to next call
        return Put(db, key.AsSpan(), value.AsSpan(), options);
    }

    /// <summary>
    /// Put data into a database.
    /// </summary>
    /// <param name="db">Database.</param>
    /// <param name="key">Key byte array.</param>
    /// <param name="value">Value byte array.</param>
    /// <param name="options">Operation options (optional).</param>
    public unsafe MDBResultCode Put(LightningDatabase db, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, PutOptions options = PutOptions.None)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        fixed (byte* keyPtr = key)
        fixed (byte* valuePtr = value)
        {
            var mdbKey = new MDBValue(key.Length, keyPtr);
            var mdbValue = new MDBValue(value.Length, valuePtr);

            return mdb_put(_handle, db._handle, mdbKey, mdbValue, options);
        }
    }

    /// <summary>
    /// Delete items from a database.
    /// This function removes key/data pairs from the database.
    /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored.
    /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted.
    /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted.
    /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
    /// </summary>
    /// <param name="db">A database handle returned by mdb_dbi_open()</param>
    /// <param name="key">The key to delete from the database</param>
    /// <param name="value">The data to delete (optional)</param>
    public MDBResultCode Delete(LightningDatabase db, byte[] key, byte[] value)
    {//argument validation delegated to next call
        return Delete(db, key.AsSpan(), value.AsSpan());
    }

    /// <summary>
    /// Delete items from a database.
    /// This function removes key/data pairs from the database.
    /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored.
    /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted.
    /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted.
    /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
    /// </summary>
    /// <param name="db">A database handle returned by mdb_dbi_open()</param>
    /// <param name="key">The key to delete from the database</param>
    /// <param name="value">The data to delete (optional)</param>
    public unsafe MDBResultCode Delete(LightningDatabase db, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        fixed (byte* keyPtr = key)
        fixed (byte* valuePtr = value)
        {
            var mdbKey = new MDBValue(key.Length, keyPtr);
            if (value.IsEmpty)
            {
                return mdb_del(_handle, db._handle, mdbKey);
            }
            var mdbValue = new MDBValue(value.Length, valuePtr);
            return mdb_del(_handle, db._handle, mdbKey, mdbValue);
        }
    }

    /// <summary>
    /// Delete items from a database.
    /// This function removes key/data pairs from the database.
    /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored.
    /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted.
    /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted.
    /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
    /// </summary>
    /// <param name="db">A database handle returned by mdb_dbi_open()</param>
    /// <param name="key">The key to delete from the database</param>
    public MDBResultCode Delete(LightningDatabase db, byte[] key)
    {
        return Delete(db, key.AsSpan());
    }


    /// <summary>
    /// Delete items from a database.
    /// This function removes key/data pairs from the database.
    /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored.
    /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted.
    /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted.
    /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
    /// </summary>
    /// <param name="db">A database handle returned by mdb_dbi_open()</param>
    /// <param name="key">The key to delete from the database</param>
    public unsafe MDBResultCode Delete(LightningDatabase db, ReadOnlySpan<byte> key)
    {
        fixed(byte* ptr = key) {
            var mdbKey = new MDBValue(key.Length, ptr);
            return mdb_del(_handle, db._handle, mdbKey);
        }
    }

    /// <summary>
    /// Aborts the read-only transaction and resets the transaction handle so it can be
    /// reused after calling <see cref="Renew"/>
    /// </summary>
    /// <remarks>
    /// Resetting a transaction releases any internal locks or resources associated with it without
    /// completely destroying the transaction object. This is more efficient than creating a new
    /// transaction when you need to perform another read operation after completing one.
    ///
    /// After calling Reset, you must call <see cref="Renew"/> before using the transaction again.
    ///
    /// This method can only be called on read-only transactions. For read-write transactions,
    /// you must use <see cref="Commit"/> or <see cref="Abort"/>.
    ///
    /// This method also releases the reader slot in the lock table, allowing it to be
    /// reused immediately instead of waiting for the environment to close or the thread to terminate.
    /// </remarks>
    public void Reset()
    {
        if (!IsReadOnly)
            throw new InvalidOperationException("Can't reset non-readonly transaction");
        if(State != LightningTransactionState.Ready && State != LightningTransactionState.Done)
            throw new InvalidOperationException("Transaction has already been reset");

        State = LightningTransactionState.Reset;
        mdb_txn_reset(_handle);
    }

    /// <summary>
    /// Renews a read-only transaction previously released with <see cref="Reset"/>
    /// </summary>
    /// <remarks>
    /// This function refreshes the transaction handle and makes it available for reuse.
    /// It must be called before reusing a read-only transaction that was reset with <see cref="Reset"/>.
    ///
    /// Renewing a transaction is much more efficient than creating a new transaction, as it
    /// reuses the same transaction handle and reader slot.
    ///
    /// This function can only be used on read-only transactions that have been reset.
    ///
    /// After a successful call to Renew, the transaction will be in the Ready state and
    /// can be used for database operations again.
    /// </remarks>
    public MDBResultCode Renew()
    {
        if (!IsReadOnly)
            throw new InvalidOperationException("Can't renew non-readonly transaction");

        if (State != LightningTransactionState.Reset)
            throw new InvalidOperationException("Transaction should be reset first");

        State = LightningTransactionState.Done;
        var result = mdb_txn_renew(_handle).ThrowOnError();
        State = LightningTransactionState.Ready;
        return result;
    }

    /// <summary>
    /// Commit all the operations of a transaction into the database.
    /// </summary>
    public MDBResultCode Commit()
    {
        if(State != LightningTransactionState.Ready)
            throw new InvalidOperationException("Transaction that is not ready cannot be committed");
        if (ParentTransaction != null && ParentTransaction.State != LightningTransactionState.Ready)
        {
            State = ParentTransaction.State;
            return MDBResultCode.BadTxn;
        }

        State = LightningTransactionState.Done;
        return mdb_txn_commit(_handle);
    }

    /// <summary>
    /// Abandon all the operations of the transaction instead of saving them.
    /// </summary>
    public void Abort()
    {
        if(State != LightningTransactionState.Ready)
            throw new InvalidOperationException("Transaction that is not ready cannot be committed");
        State = LightningTransactionState.Done;
        mdb_txn_abort(_handle);
    }

    /// <summary>
    /// The number of items in the database.
    /// </summary>
    /// <param name="db">The database we are counting items in.</param>
    /// <returns>The number of items.</returns>
    public long GetEntriesCount(LightningDatabase db)
    {
        mdb_stat(_handle, db._handle, out var stat).ThrowOnError();

        return stat.ms_entries;
    }

    /// <summary>
    /// Retrieve the statistics for the specified database.
    /// </summary>
    /// <param name="db">The database we are interested in.</param>
    /// <returns>The retrieved statistics.</returns>
    public Stats GetStats(LightningDatabase db)
    {
        mdb_stat(_handle, db._handle, out var stat).ThrowOnError();
        return new Stats
        {
            BranchPages = stat.ms_branch_pages,
            BTreeDepth = stat.ms_depth,
            Entries = stat.ms_entries,
            LeafPages = stat.ms_leaf_pages,
            OverflowPages = stat.ms_overflow_pages,
            PageSize = stat.ms_psize
        };
    }

    /// <summary>
    /// Environment in which the transaction was opened.
    /// </summary>
    public LightningEnvironment Environment { get; }

    /// <summary>
    /// Parent transaction of this transaction.
    /// </summary>
    public LightningTransaction ParentTransaction { get; }

    /// <summary>
    /// Whether this transaction is read-only.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets the transaction ID.
    /// </summary>
    public int Id => mdb_txn_id(_handle);

    /// <summary>
    /// Compares two data items according to the database's key comparison function.
    /// </summary>
    /// <param name="db">The database to use for comparison</param>
    /// <param name="a">First item to compare</param>
    /// <param name="b">Second item to compare</param>
    /// <returns>Negative value if a is less than b, zero if equal, positive if a is greater than b</returns>
    public unsafe int CompareKeys(LightningDatabase db, ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        fixed (byte* aPtr = a)
        fixed (byte* bPtr = b)
        {
            var mdbA = new MDBValue(a.Length, aPtr);
            var mdbB = new MDBValue(b.Length, bPtr);

            return mdb_cmp(_handle, db._handle, ref mdbA, ref mdbB);
        }
    }

    /// <summary>
    /// Compares two data items according to the database's data comparison function.
    /// </summary>
    /// <param name="db">The database to use for comparison</param>
    /// <param name="a">First item to compare</param>
    /// <param name="b">Second item to compare</param>
    /// <returns>Negative value if a is less than b, zero if equal, positive if a is greater than b</returns>
    public unsafe int CompareData(LightningDatabase db, ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (db == null)
            throw new ArgumentNullException(nameof(db));

        fixed (byte* aPtr = a)
        fixed (byte* bPtr = b)
        {
            var mdbA = new MDBValue(a.Length, aPtr);
            var mdbB = new MDBValue(b.Length, bPtr);

            return mdb_dcmp(_handle, db._handle, ref mdbA, ref mdbB);
        }
    }

    /// <summary>
    /// Dispose this transaction and deallocate all resources associated with it.
    /// </summary>
    /// <param name="disposing">True if called from Dispose.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;
        if (!Environment.IsOpened)
            throw new InvalidOperationException("A transaction must be disposed before closing the environment");
        if (State == LightningTransactionState.Ready && Environment.IsOpened)
        {
            Abort();
        }
        State = LightningTransactionState.Released;

        _handle = default;

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Dispose this transaction and deallocate all resources associated with it.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    ~LightningTransaction()
    {
        Dispose(false);
    }

    public override int GetHashCode()
    {
        return _originalHandle.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var tran = obj as LightningTransaction;
        return tran != null && _handle.Equals(tran._handle);
    }
}