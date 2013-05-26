using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public class LightningCursor : IDatabaseAttributesProvider, IPutter, IDisposable
    {
        private IntPtr _handle;
        private bool _shouldDispose;
        private EventHandler<LightningClosingEventArgs> _environmentOrTransactionClosing;

        public LightningCursor(LightningDatabase db, LightningTransaction txn)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (txn == null)
                throw new ArgumentNullException("txn");

            if (db.Environment != txn.Environment)
                throw new ArgumentException("db and txn belong to different environments");

            IntPtr handle;
            var res = Native.mdb_cursor_open(txn._handle, db._handle, out handle);
            if (res != 0)
                throw new LightningException(res);

            _handle = handle;

            this.Database = db;
            this.Transaction = txn;

            _shouldDispose = true;
            _environmentOrTransactionClosing = new EventHandler<LightningClosingEventArgs>(this.EnvironmentOrTransactionClosing);

            if (txn.IsReadOnly)
                this.Environment.Closing += _environmentOrTransactionClosing;
            else
                this.Transaction.Closing += _environmentOrTransactionClosing;
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
                this.Environment.Closing -= _environmentOrTransactionClosing;
            else
                this.Transaction.Closing -= _environmentOrTransactionClosing;
        }

        public LightningEnvironment Environment { get { return this.Database.Environment; } }

        public LightningDatabase Database { get; private set; }

        public LightningTransaction Transaction { get; private set; }

        //TODO: tests
        public KeyValuePair<byte[], byte[]> Get(CursorOperation operation)
        {
            var keyStruct = new ValueStructure();
            var valueStruct = new ValueStructure();

            var res = Native.mdb_cursor_get(_handle, ref keyStruct, ref valueStruct, operation);
            if (res != 0)
                throw new LightningException(res);

            var keyBuffer = new byte[keyStruct.size];
            var valueBuffer = new byte[valueStruct.size];

            Marshal.Copy(keyStruct.data, keyBuffer, 0, keyStruct.size);
            Marshal.Copy(valueStruct.data, valueBuffer, 0, valueStruct.size);

            //TODO: Possible leak. Is the original data which is copied to buffer stays in memory?
            return new KeyValuePair<byte[], byte[]>(keyBuffer, valueBuffer);
        }

        //TODO: tests
        public void Put(byte[] key, byte[] value, PutOptions options)
        {
            var keyStruct = new ValueStructure
            {
                data = Marshal.AllocHGlobal(key.Length),
                size = key.Length
            };

            var valueStruct = new ValueStructure
            {
                data = Marshal.AllocHGlobal(value.Length),
                size = value.Length
            };

            try
            {
                Marshal.Copy(key, 0, keyStruct.data, key.Length);
                Marshal.Copy(value, 0, valueStruct.data, value.Length);

                var res = Native.mdb_cursor_put(_handle, keyStruct, valueStruct, options);
                if (res != 0)
                    throw new LightningException(res);
            }
            finally
            {
                Marshal.FreeHGlobal(keyStruct.data);
                Marshal.FreeHGlobal(valueStruct.data);
            }
        }

        //TODO: tests
        public void Delete(CursorDeleteOption option)
        {
            var res = Native.mdb_cursor_del(_handle, option);
            if (res != 0)
                throw new LightningException(res);
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

            var res = Native.mdb_cursor_renew(txn._handle, _handle);
            if (res != 0)
                throw new LightningException(res);
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
