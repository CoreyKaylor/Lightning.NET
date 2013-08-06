using System;
using System.Collections.Generic;
using System.Text;
using LightningDB.BasicExtensions;

namespace LightningDB
{
    public class LightningCursor : IDatabaseAttributesProvider, IPutter, IDisposable
    {
        private readonly IntPtr _handle;
        private bool _shouldDispose;

        public LightningCursor(LightningDatabase db, LightningTransaction txn)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (txn == null)
                throw new ArgumentNullException("txn");

            if (db.Environment != txn.Environment)
                throw new ArgumentException("db and txn belong to different environments");

            IntPtr handle = default(IntPtr);
            Native.Execute(() => Native.mdb_cursor_open(txn._handle, db._handle, out handle));

            _handle = handle;

            this.Database = db;
            this.Transaction = txn;

            _shouldDispose = true;

            if (txn.IsReadOnly)
                this.Environment.Closing += EnvironmentOrTransactionClosing;
            else
                this.Transaction.Closing += EnvironmentOrTransactionClosing;
        }

        private void EnvironmentOrTransactionClosing(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch { }
        }

        private void DetachClosingHandler()
        {
            if (this.Transaction.IsReadOnly)
                this.Environment.Closing -= EnvironmentOrTransactionClosing;
            else
                this.Transaction.Closing -= EnvironmentOrTransactionClosing;
        }

        public LightningEnvironment Environment { get { return this.Database.Environment; } }

        public LightningDatabase Database { get; private set; }

        public LightningTransaction Transaction { get; private set; }

        //TODO: tests
        public KeyValuePair<byte[], byte[]> Get(CursorOperation operation)
        {
            var keyStruct = new ValueStructure();
            var valueStruct = new ValueStructure();

            var res = Native.Read(() => Native.mdb_cursor_get(_handle, ref keyStruct, ref valueStruct, operation));

            return new KeyValuePair<byte[], byte[]>(keyStruct.ToByteArray(res), valueStruct.ToByteArray(res));
        }

        //TODO: tests
        public void Put(byte[] key, byte[] value, PutOptions options)
        {
            using(var keyMarshalStruct = new MarshalValueStructure(key))
            using (var valueMarshalStruct = new MarshalValueStructure(value))
            {
                var keyStruct = keyMarshalStruct.ValueStructure;
                var valueStruct = valueMarshalStruct.ValueStructure;

                Native.Execute(() => Native.mdb_cursor_put(_handle, keyStruct, valueStruct, options));
            }
        }

        //TODO: tests
        public void Delete(CursorDeleteOption option)
        {
            Native.Execute(() => Native.mdb_cursor_del(_handle, option));
        }

        public void Renew()
        {
            this.Renew(null);
        }

        //TODO: tests
        public void Renew(LightningTransaction txn)
        {
            txn = txn ?? this.Transaction;

            if (!txn.IsReadOnly)
                throw new InvalidOperationException("Can't renew cursor on non-readonly transaction");

            Native.Execute(() => Native.mdb_cursor_renew(txn._handle, _handle));
        }

        //TODO: tests
        public void Close()
        {
            try
            {
                Native.mdb_cursor_close(_handle);
            }
            finally
            {
                this.DetachClosingHandler();
            }
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (shouldDispose)
                this.Close();
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }

        #endregion

        #region IDatabaseAttributesProvider Members

        DatabaseOpenFlags IDatabaseAttributesProvider.OpenFlags { get { return this.Database.OpenFlags; } }

        string IDatabaseAttributesProvider.Name { get { return this.Database.Name; } }

        Encoding IDatabaseAttributesProvider.Encoding { get { return this.Database.Encoding; } }

        #endregion
    }
}
