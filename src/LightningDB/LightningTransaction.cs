using System;
using System.Runtime.InteropServices;
using System.Text;
using LightningDB.Factories;
using LightningDB.Native;

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

        internal IntPtr _handle;

        private readonly TransactionManager _subTransactionsManager;
        private readonly CursorManager _cursorManager;

        /// <summary>
        /// Created new instance of LightningTransaction
        /// </summary>
        /// <param name="environment">Environment.</param>
        /// <param name="parent">Parent transaction or null.</param>
        /// <param name="flags">Transaction open options.</param>
        internal LightningTransaction(LightningEnvironment environment, IntPtr handle, LightningTransaction parent, TransactionBeginFlags flags)
        {
            if (environment == null)
                throw new ArgumentNullException("environment");

            this.Environment = environment;
            this.ParentTransaction = parent;
            this.IsReadOnly = flags == TransactionBeginFlags.ReadOnly;
            this.State = LightningTransactionState.Active;

            _handle = handle;
            _subTransactionsManager = new TransactionManager(environment, this);
            _cursorManager = new CursorManager(this);
        }

        internal TransactionManager SubTransactionsManager { get { return _subTransactionsManager; } }

        internal CursorManager CursorManager { get { return _cursorManager; } }

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
            return this.Environment.BeginTransaction(this, beginFlags);
        }

        /// <summary>
        /// Begins a child transaction.
        /// </summary>
        /// <returns>New child transaction with default options.</returns>
        public LightningTransaction BeginTransaction()
        {
            return this.BeginTransaction(DefaultTransactionBeginFlags);
        }

        private static CompareFunction CreateComparisonFunction(
            LightningDatabase db, Func<LightningDatabase, byte[], byte[], int> compare)
        {
            return (IntPtr left, IntPtr right) =>
                compare.Invoke(db, NativeMethods.ValueByteArrayFromPtr(left), NativeMethods.ValueByteArrayFromPtr(right));
        }

        internal CompareFunction SetCompareFunction(LightningDatabase db, Func<LightningDatabase, byte[], byte[], int> compare)
        {
            var compareFunction = CreateComparisonFunction(db, compare);
            
            NativeMethods.Execute(lib =>
                lib.mdb_set_compare(_handle, db._handle, compareFunction));

            return compareFunction;
        }

        /// <summary>
        /// Opens a database in context of this transaction.
        /// </summary>
        /// <param name="name">Database name (optional). If null then the default name is used.</param>
        /// <param name="flags">Database open options (optionsl).</param>
        /// <param name="encoding">Database keys encoding.</param>
        /// <param name="compare">Key comparison function</param>
        /// <returns>Created database wrapper.</returns>
        public LightningDatabase OpenDatabase(
            string name = null, 
            DatabaseOpenFlags? flags = null, 
            Encoding encoding = null,
            Func<LightningDatabase, byte[], byte[], int> compare = null)
        {
            var db = this.Environment.OpenDatabase(name, this, flags, encoding);

            if (compare != null)
            {
                var comparer = SetCompareFunction(db, compare);
                SubTransactionsManager.StoreComparer(comparer);
            }

            return db;
        }

        /// <summary>
        /// Deletes or closes a database.
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="delete">Database is deleted permanently if true, or just closed if false.</param>
        public void DropDatabase(LightningDatabase db, bool delete)
        {
            NativeMethods.Execute(lib => lib.mdb_drop(_handle, db._handle, delete));

            db.Close(false);
        }

        /// <summary>
        /// Create a cursor.
        /// Cursors are associated with a specific transaction and database and may not span threads.
        /// </summary>
        /// <param name="db">A database.</param>
        public LightningCursor CreateCursor(LightningDatabase db)
        {
            return CursorManager.OpenCursor(db);
        }

        private bool TryGetInternal(UInt32 dbi, byte[] key, out Func<byte[]> valueFactory)
        {
            valueFactory = null;
            
            using (var keyMarshalStruct = new MarshalValueStructure(key))
            {
                var valueStruct = default(ValueStructure);
                var keyStructure = keyMarshalStruct.ValueStructure;

                var res = NativeMethods.Read(lib => lib.mdb_get(_handle, dbi, ref keyStructure, out valueStruct));

                var exists = res != NativeMethods.MDB_NOTFOUND;
                if (exists)
                    valueFactory = () => valueStruct.ToByteArray(res);

                return exists;
            }
        }

        /// <summary>
        /// Get value from a database.
        /// </summary>
        /// <param name="db">Database </param>
        /// <param name="key">Key byte array.</param>
        /// <returns>Requested value's byte array if exists, or null if not.</returns>
        public byte[] Get(LightningDatabase db, byte[] key)
        {
            byte[] value = null;
            this.TryGet(db, key, out value);

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
                throw new ArgumentNullException("db");

            Func<byte[]> factory;
            var result = this.TryGetInternal(db._handle, key, out factory);

            value = result
                ? factory.Invoke()
                : null;

            return result;
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
                throw new ArgumentNullException("db");

            Func<byte[]> factory;
            return this.TryGetInternal(db._handle, key, out factory);
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
                throw new ArgumentNullException("db");

            using (var keyStructureMarshal = new MarshalValueStructure(key))
            using (var valueStructureMarshal = new MarshalValueStructure(value))
            {
                var keyStruct = keyStructureMarshal.ValueStructure;
                var valueStruct = valueStructureMarshal.ValueStructure;

                NativeMethods.Execute(lib => lib.mdb_put(_handle, db._handle, ref keyStruct, ref valueStruct, options));
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
        public void Delete(LightningDatabase db, byte[] key, byte[] value = null)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            using (var keyMarshalStruct = new MarshalValueStructure(key))
            {
                var keyStructure = keyMarshalStruct.ValueStructure;
                if (value != null)
                {
                    using (var valueMarshalStruct = new MarshalValueStructure(value))
                    {
                        var valueStructure = valueMarshalStruct.ValueStructure;
                        NativeMethods.Execute(lib => lib.mdb_del(_handle, db._handle, ref keyStructure, ref valueStructure));
                        return;
                    }
                }
                NativeMethods.Execute(lib => lib.mdb_del(_handle, db._handle, ref keyStructure, IntPtr.Zero));
            }
        }

        /// <summary>
        /// Reset current transaction.
        /// </summary>
        public void Reset()
        {
            if (!this.IsReadOnly)
                throw new InvalidOperationException("Can't reset non-readonly transaction");

            NativeMethods.Library.mdb_txn_reset(_handle);
            this.State = LightningTransactionState.Reseted;
        }

        /// <summary>
        /// Renew current transaction.
        /// </summary>
        public void Renew()
        {
            if (!this.IsReadOnly)
                throw new InvalidOperationException("Can't renew non-readonly transaction");

            if (this.State != LightningTransactionState.Reseted)
                throw new InvalidOperationException("Transaction should be reseted first");

            NativeMethods.Library.mdb_txn_renew(_handle);
            this.State = LightningTransactionState.Active;
        }

        private void NotifyDiscarded()
        {
            var manager = this.ParentTransaction != null
                ? this.ParentTransaction.SubTransactionsManager
                : this.Environment.TransactionManager;

            manager.WasDiscarded(this);
        }

        private void Discard(Action body)
        {
            try
            {
                _subTransactionsManager.AbortAll();
            }
            finally
            {
                try
                {
                    CursorManager.CloseAll();
                }
                finally
                {
                    try
                    {
                        body.Invoke();
                    }
                    finally
                    {
                        NotifyDiscarded();
                    }
                }
            }
        }

        /// <summary>
        /// Commit all the operations of a transaction into the database.
        /// All cursors opened within the transaction will be closed by this call. 
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Commit()
        {
            Discard(() =>
            {
                try
                {
                    NativeMethods.Execute(lib => lib.mdb_txn_commit(_handle));

                    this.State = LightningTransactionState.Commited;

                    NotifyDiscarded();
                }
                catch (LightningException)
                {
                    this.Abort();
                    throw;
                }
            });
        }

        /// <summary>
        /// Abandon all the operations of the transaction instead of saving them.
        /// All cursors opened within the transaction will be closed by this call.
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Abort()
        {
            Discard(() =>
            {
                this.State = LightningTransactionState.Aborted;
                NativeMethods.Library.mdb_txn_abort(_handle);
            });
        }

        public long GetEntriesCount(LightningDatabase db)
        {
            var stat = new MDBStat();
            NativeMethods.Execute(lib => lib.mdb_stat(_handle, db._handle, out stat));

            return stat.ms_entries.ToInt64();
        }

        /// <summary>
        /// Environment in which the transaction was opened.
        /// </summary>
        public LightningEnvironment Environment { get; private set; }

        /// <summary>
        /// Parent transaction of this transaction.
        /// </summary>
        public LightningTransaction ParentTransaction { get; private set; }

        /// <summary>
        /// Whether this transaction is read-only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Abort this transaction and deallocate all resources associated with it (including databases).
        /// </summary>
        /// <param name="shouldDispose">True if not disposed yet.</param>
        protected virtual void Dispose(bool shouldDispose)
        {
            if (shouldDispose && !IntPtr.Zero.Equals(_handle))
            {
                try
                {
                    this.Abort();
                }
                catch { }
            }
        }

        /// <summary>
        /// Abort this transaction and deallocate all resources associated with it (including databases).
        /// </summary>
        public void Dispose()
        {
            this.Dispose(this.State != LightningTransactionState.Aborted && this.State != LightningTransactionState.Commited);
        }

        public override int GetHashCode()
        {
            return _handle.GetHashCode();
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
