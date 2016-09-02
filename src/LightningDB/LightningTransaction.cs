using System;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// Represents a transaction.
    /// </summary>
    public class LightningTransaction : IDisposable
    {
        /// <summary>
        /// Default options used to begin new transactions.
        /// </summary>
        public const TransactionBeginFlags DefaultTransactionBeginFlags = TransactionBeginFlags.None;

        private IntPtr _handle;
        private readonly IntPtr _originalHandle;

        /// <summary>
        /// Created new instance of LightningTransaction
        /// </summary>
        /// <param name="environment">Environment.</param>
        /// <param name="parent">Parent transaction or null.</param>
        /// <param name="flags">Transaction open options.</param>
        internal LightningTransaction(LightningEnvironment environment, LightningTransaction parent, TransactionBeginFlags flags)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            Environment = environment;
            ParentTransaction = parent;
            IsReadOnly = (flags & TransactionBeginFlags.ReadOnly) == TransactionBeginFlags.ReadOnly;
            State = LightningTransactionState.Active;
            Environment.Disposing += Dispose;
            if (parent != null)
            {
                parent.Disposing += Dispose;
                parent.StateChanging += OnParentStateChanging;
            }

            var parentHandle = parent?.Handle() ?? IntPtr.Zero;
            mdb_txn_begin(environment.Handle(), parentHandle, flags, out _handle);
            _originalHandle = _handle;
        }

        public IntPtr Handle()
        {
            return _handle;
        }

        private void OnParentStateChanging(LightningTransactionState state)
        {
            switch (state)
            {
                case LightningTransactionState.Aborted:
                case LightningTransactionState.Commited:
                    Abort();
                    break;
                default:
                    break;
            }
        }

        public event Action Disposing;
        private event Action<LightningTransactionState> StateChanging;

        /// <summary>
        /// Current transaction state.
        /// </summary>
        public LightningTransactionState State { get; internal set; }

        /// <summary>
        /// Begin a child transaction.
        /// </summary>
        /// <param name="beginFlags">Options for a new transaction.</param>
        /// <returns>New child transaction.</returns>
        public LightningTransaction BeginTransaction(TransactionBeginFlags beginFlags)
        {
            return new LightningTransaction(Environment, this, beginFlags);
        }

        /// <summary>
        /// Begins a child transaction.
        /// </summary>
        /// <returns>New child transaction with default options.</returns>
        public LightningTransaction BeginTransaction()
        {
            return BeginTransaction(DefaultTransactionBeginFlags);
        }

        /// <summary>
        /// Opens a database in context of this transaction.
        /// </summary>
        /// <param name="name">Database name (optional). If null then the default name is used.</param>
        /// <param name="configuration">Database open options.</param>
        /// <returns>Created database wrapper.</returns>
        public LightningDatabase OpenDatabase(string name = null, DatabaseConfiguration configuration = null)
        {
            configuration = configuration ?? new DatabaseConfiguration();
            var db = new LightningDatabase(name, this, configuration);
            return db;
        }

        /// <summary>
        /// Drops the database.
        /// </summary>
        public void DropDatabase(LightningDatabase database)
        {
            database.Drop(this);
        }

        /// <summary>
        /// Truncates all data from the database.
        /// </summary>
        public void TruncateDatabase(LightningDatabase database)
        {
            database.Truncate(this);
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
        /// <param name="db">Database </param>
        /// <param name="key">Key byte array.</param>
        /// <returns>Requested value's byte array if exists, or null if not.</returns>
        public byte[] Get(LightningDatabase db, byte[] key)
        {
            byte[] value;
            TryGet(db, key, out value);
            return value;
        }

        /// <summary>
        /// Tries to get a value by its key.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="key">Key byte array.</param>
        /// <param name="value">Value byte array if exists.</param>
        /// <returns>True if key exists, false if not.</returns>
        public bool TryGet(LightningDatabase db, byte[] key, out byte[] value)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            return mdb_get(_handle, db.Handle(), key, out value) != MDB_NOTFOUND;
        }

        /// <summary>
        /// Check whether data exists in database.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>True if key exists, false if not.</returns>
        public bool ContainsKey(LightningDatabase db, byte[] key)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            byte[] value;
            return TryGet(db, key, out value);
        }

        /// <summary>
        /// Put data into a database.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="key">Key byte array.</param>
        /// <param name="value">Value byte array.</param>
        /// <param name="options">Operation options (optional).</param>
        public void Put(LightningDatabase db, byte[] key, byte[] value, PutOptions options = PutOptions.None)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            mdb_put(_handle, db.Handle(), key, value, options);
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
        public void Delete(LightningDatabase db, byte[] key, byte[] value)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            mdb_del(_handle, db.Handle(), key, value);
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
        public void Delete(LightningDatabase db, byte[] key)
        {
            mdb_del(_handle, db.Handle(), key);
        }

        /// <summary>
        /// Reset current transaction.
        /// </summary>
        public void Reset()
        {
            if (!IsReadOnly)
                throw new InvalidOperationException("Can't reset non-readonly transaction");

            mdb_txn_reset(_handle);
            State = LightningTransactionState.Reseted;
        }

        /// <summary>
        /// Renew current transaction.
        /// </summary>
        public void Renew()
        {
            if (!IsReadOnly)
                throw new InvalidOperationException("Can't renew non-readonly transaction");

            if (State != LightningTransactionState.Reseted)
                throw new InvalidOperationException("Transaction should be reseted first");

            mdb_txn_renew(_handle);
            State = LightningTransactionState.Active;
        }

        /// <summary>
        /// Commit all the operations of a transaction into the database.
        /// All cursors opened within the transaction will be closed by this call. 
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Commit()
        {
            State = LightningTransactionState.Commited;
            StateChanging?.Invoke(State);
            mdb_txn_commit(_handle);
        }

        /// <summary>
        /// Abandon all the operations of the transaction instead of saving them.
        /// All cursors opened within the transaction will be closed by this call.
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Abort()
        {
            State = LightningTransactionState.Aborted;
            StateChanging?.Invoke(State);
            mdb_txn_abort(_handle);
        }

        /// <summary>
        /// The number of items in the database.
        /// </summary>
        /// <param name="db">The database we are counting items in.</param>
        /// <returns>The number of items.</returns>
        public long GetEntriesCount(LightningDatabase db)
        {
            MDBStat stat;
            mdb_stat(_handle, db.Handle(), out stat);

            return stat.ms_entries.ToInt64();
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
        /// Abort this transaction and deallocate all resources associated with it (including databases).
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero)
                return;

            Environment.Disposing -= Dispose;
            if (ParentTransaction != null)
            {
                ParentTransaction.Disposing -= Dispose;
                ParentTransaction.StateChanging -= OnParentStateChanging;
            }

            Disposing?.Invoke();

            if (State == LightningTransactionState.Active || State == LightningTransactionState.Reseted)
                Abort();

            _handle = IntPtr.Zero;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Dispose this transaction and deallocate all resources associated with it (including databases).
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
            if (tran == null)
                return false;

            return _handle.Equals(tran._handle);
        }
    }
}
