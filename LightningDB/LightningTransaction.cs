using System;

namespace LightningDB
{
    public class LightningTransaction : IClosingEventSource, IDisposable
    {
        public const TransactionBeginFlags DefaultTransactionBeginFlags = TransactionBeginFlags.None;

        internal IntPtr _handle;

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
            Native.Execute(() => Native.mdb_txn_begin(environment._handle, parentHandle, flags, out handle));

            _handle = handle;

            this.State = LightningTransacrionState.Active;

            if (parent == null)
                this.Environment.Closing += EnvironmentOrParentTransactionClosing;
            else
                parent.Closing += EnvironmentOrParentTransactionClosing;
        }

        public event EventHandler<LightningClosingEventArgs> Closing;

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

        public LightningTransacrionState State { get; private set; }

        public LightningTransaction BeginTransaction(TransactionBeginFlags beginFlags)
        {
            return this.Environment.BeginTransaction(this, beginFlags);
        }

        public LightningTransaction BeginTransaction()
        {
            return this.BeginTransaction(DefaultTransactionBeginFlags);
        }

        public LightningDatabase OpenDatabase(string name = null, DatabaseOpenFlags flags = DatabaseOpenFlags.None)
        {
            return this.Environment.OpenDatabase(name, flags, this);
        }

        public void Reset()
        {
            if (!this.IsReadOnly)
                throw new InvalidOperationException("Can't reset non-readonly transaction");

            Native.mdb_txn_reset(_handle);
            this.State = LightningTransacrionState.Reseted;
        }

        public void Renew()
        {
            if (!this.IsReadOnly)
                throw new InvalidOperationException("Can't renew non-readonly transaction");

            if (this.State != LightningTransacrionState.Reseted)
                throw new InvalidOperationException("Transaction should be reseted first");

            Native.mdb_txn_renew(_handle);
            this.State = LightningTransacrionState.Active;
        }

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
                        Native.Execute(() => Native.mdb_txn_commit(_handle));
                    }
                    catch (LightningException)
                    {
                        this.Abort(false);
                        throw;
                    }
                    this.State = LightningTransacrionState.Commited;
                }
            }
            finally
            {
                this.DetachClosingHandler();
            }            
        }

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
                    Native.mdb_txn_abort(_handle);
                }
            }
            finally
            {
                this.DetachClosingHandler();
                this.State = LightningTransacrionState.Aborted;
            }
        }

        private void DetachClosingHandler()
        {
            if (this.ParentTransaction == null)
                this.Environment.Closing -= EnvironmentOrParentTransactionClosing;
            else
                this.ParentTransaction.Closing -= EnvironmentOrParentTransactionClosing;
        }

        public LightningEnvironment Environment { get; private set; }

        public LightningTransaction ParentTransaction { get; private set; }

        public bool IsReadOnly { get; private set; }

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

        public void Dispose()
        {
            this.Dispose(this.State != LightningTransacrionState.Aborted && this.State != LightningTransacrionState.Commited);
        }
    }
}
