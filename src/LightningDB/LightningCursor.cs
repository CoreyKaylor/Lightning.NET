using System;
using System.Collections.Generic;

namespace LightningDB
{
    public class LightningCursor : IDisposable
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

        public KeyValuePair<byte[], byte[]> MoveTo(byte[] key)
        {
            using (var marshalKey = new MarshalValueStructure(key))
                return this.Get(CursorOperation.Set, marshalKey.ValueStructure, null);
        }

        public KeyValuePair<byte[], byte[]> MoveTo(byte[] key, byte[] value)
        {
            using (var marshalKey = new MarshalValueStructure(key))
            using (var marshalValue = new MarshalValueStructure(value))
                return this.Get(CursorOperation.GetBoth, marshalKey.ValueStructure, marshalValue.ValueStructure);
        }

        public KeyValuePair<byte[], byte[]> MoveToFirstValueAfter(byte[] key, byte[] value)
        {
            using (var marshalKey = new MarshalValueStructure(key))
            using (var marshalValue = new MarshalValueStructure(value))
                return this.Get(CursorOperation.GetBothRange, marshalKey.ValueStructure, marshalValue.ValueStructure);
        }

        public KeyValuePair<byte[], byte[]> MoveToFirstAfter(byte[] key)
        {
            using(var marshalKey = new MarshalValueStructure(key))
                return this.Get(CursorOperation.SetRange, marshalKey.ValueStructure, null);
        }

        //What difference from CursorOperation.Set
        //public KeyValuePair<byte[], byte[]> MoveTo(ValueStructure key)
        //{
        //    return this.Get(CursorOperation.SetKey, key, null);
        //}

        public KeyValuePair<byte[], byte[]> MoveToFirst()
        {
            return this.Get(CursorOperation.First);
        }

        public KeyValuePair<byte[], byte[]> MoveToFirstDuplicate()
        {
            return this.Get(CursorOperation.FirstDuplicate);
        }

        public KeyValuePair<byte[], byte[]> MoveToLast()
        {
            return this.Get(CursorOperation.Last);
        }

        public KeyValuePair<byte[], byte[]> MoveToLastDuplicate()
        {
            return this.Get(CursorOperation.LastDuplicate);
        }

        public KeyValuePair<byte[], byte[]> GetCurrent()
        {
            return this.Get(CursorOperation.GetCurrent);
        }

        //Not sure what it should do and if the wrapper is correct
        public byte[] GetMultiple()
        {
            return this.Get(CursorOperation.GetMultiple)
                .Value;
        }

        public KeyValuePair<byte[], byte[]> MoveNext()
        {
            return this.Get(CursorOperation.Next);
        }

        public KeyValuePair<byte[], byte[]> MoveNextDuplicate()
        {
            return this.Get(CursorOperation.NextDuplicate);
        }

        public KeyValuePair<byte[], byte[]> MoveNextNoDuplicate()
        {
            return this.Get(CursorOperation.NextNoDuplicate);
        }

        //Not sure what it should do and if the wrapper is correct
        public byte[] MoveNextMultiple()
        {
            return this.Get(CursorOperation.NextMultiple)
                .Value;
        }

        public KeyValuePair<byte[], byte[]> MovePrev()
        {
            return this.Get(CursorOperation.Previous);
        }

        public KeyValuePair<byte[], byte[]> MovePrevDuplicate()
        {
            return this.Get(CursorOperation.PreviousDuplicate);
        }

        public KeyValuePair<byte[], byte[]> MovePrevNoDuplicate()
        {
            return this.Get(CursorOperation.PreviousNoDuplicate);
        }

        //TODO: tests
        private KeyValuePair<byte[], byte[]> Get(CursorOperation operation, ValueStructure? key = null, ValueStructure? value = null)
        {
            var keyStruct = key.GetValueOrDefault();
            var valueStruct = value.GetValueOrDefault();

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
            this.Renew(this.Transaction);
        }

        //TODO: tests
        public void Renew(LightningTransaction txn)
        {
            if(txn == null)
                throw new ArgumentNullException("txn");

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

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
