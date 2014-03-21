using System;
using System.Text;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Represents a transaction.
    /// </summary>
    public class LightningTransaction : IClosingEventSource, IDisposable
    {
        /// <summary>
        /// Default options used to begin new transactions.
        /// </summary>
        public const TransactionBeginFlags DefaultTransactionBeginFlags = TransactionBeginFlags.None;

        internal IntPtr _handle;

        private LightningDatabase _defaultDatabase;

        /// <summary>
        /// Created new instance of LightningTransaction
        /// </summary>
        /// <param name="environment">Environment.</param>
        /// <param name="parent">Parent transaction or null.</param>
        /// <param name="flags">Transaction open options.</param>
        public LightningTransaction(LightningEnvironment environment, LightningTransaction parent, TransactionBeginFlags flags)
        {
            if (environment == null)
                throw new ArgumentNullException("environment");

            this.Environment = environment;
            this.ParentTransaction = parent;
            this.IsReadOnly = flags == TransactionBeginFlags.ReadOnly;
            
            var parentHandle = parent != null
                ? parent._handle
                : IntPtr.Zero;

            IntPtr handle = default(IntPtr);
            NativeMethods.Execute(lib => lib.mdb_txn_begin(environment._handle, parentHandle, flags, out handle));

            _handle = handle;

            this.State = LightningTransactionState.Active;

            if (parent == null)
                this.Environment.Closing += EnvironmentOrParentTransactionClosing;
            else
                parent.Closing += EnvironmentOrParentTransactionClosing;
        }

        /// <summary>
        /// Triggered when the transaction is going to be deallocated.
        /// </summary>
        public event EventHandler<LightningClosingEventArgs> Closing;

        /// <summary>
        /// Called when the transaction is going to be deallocated.
        /// </summary>
        /// <param name="environmentClosing">Is this deallocation caused by closing corresponding environment.</param>
        protected virtual void OnClosing(bool environmentClosing)
        {
            if (this.Closing != null)
                this.Closing(this, new LightningClosingEventArgs(environmentClosing));
        }

        private void EnvironmentOrParentTransactionClosing(object sender, LightningClosingEventArgs e)
        {
            try
            {
                this.Abort(e.EnvironmentClosing);
            }
            catch
            {
            }
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

        /// <summary>
        /// Opens a database in context of this transaction.
        /// </summary>
        /// <param name="name">Database name (optional). If null then the default name is used.</param>
        /// <param name="flags">Database open options (optionsl).</param>
        /// <param name="encoding">Database keys encoding.</param>
        /// <returns>Created database wrapper.</returns>
        public LightningDatabase OpenDatabase(string name = null, DatabaseOpenFlags? flags = null, Encoding encoding = null)
        {
            if (name == null && (!flags.HasValue || flags.Value == LightningConfig.Database.DefaultOpenFlags))
            {                
                if (_defaultDatabase == null || _defaultDatabase.IsReleased)
                    _defaultDatabase = this.Environment.OpenDatabase(name, this, flags, encoding);

                if (_defaultDatabase.Encoding != (encoding ?? LightningConfig.Database.DefaultEncoding))
                    throw new InvalidOperationException("Can not change encoding of already opened database");

                return _defaultDatabase;
            }

            return this.Environment.OpenDatabase(name, this, flags, encoding);
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
            return new LightningCursor(db, this);
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

        /// <summary>
        /// Commit all the operations of a transaction into the database.
        /// All cursors opened within the transaction will be closed by this call. 
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Commit()
        {
            try
            {
                try
                {
                    this.OnClosing(false);
                }
                finally
                {
                    try
                    {
                        NativeMethods.Execute(lib => lib.mdb_txn_commit(_handle));
                    }
                    catch (LightningException)
                    {
                        this.Abort(false);
                        throw;
                    }
                    this.State = LightningTransactionState.Commited;
                }
            }
            finally
            {
                this.DetachClosingHandler();
            }            
        }

        /// <summary>
        /// Abandon all the operations of the transaction instead of saving them.
        /// All cursors opened within the transaction will be closed by this call.
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        public void Abort()
        {
            this.Abort(false);
        }

        private void Abort(bool environmentClosing)
        {
            try
            {
                try
                {
                    this.OnClosing(environmentClosing);
                }
                finally
                {
                    NativeMethods.Library.mdb_txn_abort(_handle);
                }
            }
            finally
            {
                this.DetachClosingHandler();
                this.State = LightningTransactionState.Aborted;
            }
        }

        private void DetachClosingHandler()
        {
            if (this.ParentTransaction == null)
                this.Environment.Closing -= EnvironmentOrParentTransactionClosing;
            else
                this.ParentTransaction.Closing -= EnvironmentOrParentTransactionClosing;
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
                    this.Abort(false);
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
    }
}
