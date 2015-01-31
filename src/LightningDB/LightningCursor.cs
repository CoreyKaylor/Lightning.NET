using System;
using System.Collections.Generic;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Cursor to iterate over a database
    /// </summary>
    public class LightningCursor : IDisposable
    {
        internal readonly IntPtr _handle;
        private bool _shouldDispose;

        /// <summary>
        /// Creates new instance of LightningCursor
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="txn">Transaction</param>
        internal LightningCursor(LightningDatabase db, LightningTransaction txn, IntPtr handle)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (txn == null)
                throw new ArgumentNullException("txn");

            _handle = handle;

            this.Database = db;
            this.Transaction = txn;

            _shouldDispose = true;
        }

        /// <summary>
        /// Cursor's environment.
        /// </summary>
        public LightningEnvironment Environment { get { return this.Database.Environment; } }

        /// <summary>
        /// Cursor's database.
        /// </summary>
        public LightningDatabase Database { get; private set; }

        /// <summary>
        /// Cursor's transaction.
        /// </summary>
        public LightningTransaction Transaction { get; private set; }

        /// <summary>
        /// Position at specified key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Key-value pair for the specified key</returns>
        public KeyValuePair<byte[], byte[]>? MoveTo(byte[] key)
        {
            using (var marshalKey = new MarshalValueStructure(key))
                return this.Get(CursorOperation.Set, marshalKey.ValueStructure);
        }

        /// <summary>
        /// Position at key/data pair. Only for MDB_DUPSORT
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value</param>
        /// <returns>Current key/data pair.</returns>
        public KeyValuePair<byte[], byte[]>? MoveTo(byte[] key, byte[] value)
        {
            using (var marshalKey = new MarshalValueStructure(key))
            using (var marshalValue = new MarshalValueStructure(value))
                return this.Get(CursorOperation.GetBoth, marshalKey.ValueStructure, marshalValue.ValueStructure);
        }

        /// <summary>
        /// position at key, nearest data. Only for MDB_DUPSORT
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Nearest value and corresponding key</returns>
        public KeyValuePair<byte[], byte[]>? MoveToFirstValueAfter(byte[] key, byte[] value)
        {
            using (var marshalKey = new MarshalValueStructure(key))
            using (var marshalValue = new MarshalValueStructure(value))
                return this.Get(CursorOperation.GetBothRange, marshalKey.ValueStructure, marshalValue.ValueStructure);
        }

        /// <summary>
        /// Position at first key greater than or equal to specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>First key-value pair with a key greater than or equal to specified key.</returns>
        public KeyValuePair<byte[], byte[]>? MoveToFirstAfter(byte[] key)
        {
            using(var marshalKey = new MarshalValueStructure(key))
                return this.Get(CursorOperation.SetRange, marshalKey.ValueStructure);
        }

        //What is the difference from CursorOperation.Set?
        /*public KeyValuePair<byte[], byte[]> MoveTo(byte[] key)
        {
            using (var marshalKey = new MarshalValueStructure(key))
                return this.Get(CursorOperation.SetKey, marshalKey.ValueStructure, null);
        }*/

        /// <summary>
        /// Position at first key/data item
        /// </summary>
        /// <returns>First key/data item</returns>
        public KeyValuePair<byte[], byte[]>? MoveToFirst()
        {
            return this.Get(CursorOperation.First);
        }

        /// <summary>
        /// Position at first data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>First data item of current key. Only for MDB_DUPSORT</returns>
        public byte[] MoveToFirstDuplicate()
        {
            return this.GetValue(CursorOperation.FirstDuplicate);
        }

        /// <summary>
        /// Position at last key/data item
        /// </summary>
        /// <returns>Last key/data item</returns>
        public KeyValuePair<byte[], byte[]>? MoveToLast()
        {
            return this.Get(CursorOperation.Last);
        }

        /// <summary>
        /// Position at last data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Last data item of current key</returns>
        public byte[] MoveToLastDuplicate()
        {
            return this.GetValue(CursorOperation.LastDuplicate);
        }

        /// <summary>
        /// Return key/data at current cursor position
        /// </summary>
        /// <returns>Key/data at current cursor position</returns>
        public KeyValuePair<byte[], byte[]>? GetCurrent()
        {
            return this.Get(CursorOperation.GetCurrent);
        }

        /// <summary>
        /// Return all the duplicate data items at the current cursor position. Only for MDB_DUPFIXED
        /// </summary>
        /// <remarks>Not sure what it should do and if the wrapper is correct</remarks>
        /// <returns>All the duplicate data items at the current cursor position.</returns>
        public byte[] GetMultiple()
        {
            return this.GetValue(CursorOperation.GetMultiple);
        }

        /// <summary>
        /// Position at next data item
        /// </summary>
        /// <returns>Next data item</returns>
        public KeyValuePair<byte[], byte[]>? MoveNext()
        {
            return this.Get(CursorOperation.Next);
        }

        /// <summary>
        /// Position at next data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Next data item of current key</returns>
        public KeyValuePair<byte[], byte[]>? MoveNextDuplicate()
        {
            return this.Get(CursorOperation.NextDuplicate);
        }

        /// <summary>
        /// Position at first data item of next key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>
        /// First data item of next key.
        /// </returns>
        public KeyValuePair<byte[], byte[]>? MoveNextNoDuplicate()
        {
            return this.Get(CursorOperation.NextNoDuplicate);
        }

        /// <summary>
        /// Return all duplicate data items at the next cursor position. Only for MDB_DUPFIXED
        /// </summary>
        /// <remarks>Not sure what it should do and if the wrapper is correct</remarks>
        /// <returns>All duplicate data items at the next cursor position</returns>
        public KeyValuePair<byte[], byte[]>? MoveNextMultiple()
        {
            return this.Get(CursorOperation.NextMultiple);
        }

        /// <summary>
        /// Position at previous data item.
        /// </summary>
        /// <returns>Previous data item.</returns>
        public KeyValuePair<byte[], byte[]>? MovePrev()
        {
            return this.Get(CursorOperation.Previous);
        }

        /// <summary>
        /// Position at previous data item of current key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public KeyValuePair<byte[], byte[]>? MovePrevDuplicate()
        {
            return this.Get(CursorOperation.PreviousDuplicate);
        }

        /// <summary>
        /// Position at last data item of previous key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public KeyValuePair<byte[], byte[]>? MovePrevNoDuplicate()
        {
            return this.Get(CursorOperation.PreviousNoDuplicate);
        }
        
        private KeyValuePair<byte[], byte[]>? Get(CursorOperation operation, ValueStructure? key = null, ValueStructure? value = null)
        {
            var keyStruct = key.GetValueOrDefault();
            var valueStruct = value.GetValueOrDefault();

            var res = NativeMethods.Read(lib => lib.mdb_cursor_get(_handle, ref keyStruct, ref valueStruct, operation));

            return res == NativeMethods.MDB_NOTFOUND
                ? (KeyValuePair<byte[], byte[]>?) null
                : new KeyValuePair<byte[], byte[]>(keyStruct.ToByteArray(res), valueStruct.ToByteArray(res));
        }

        private byte[] GetValue(CursorOperation operation)
        {
            var keyStruct = new ValueStructure();
            var valueStruct = new ValueStructure();

            var res = NativeMethods.Read(lib => lib.mdb_cursor_get(_handle, ref keyStruct, ref valueStruct, operation));

            return res == NativeMethods.MDB_NOTFOUND
                ? null
                : valueStruct.ToByteArray(res);
        }

        /// <summary>
        /// Store by cursor.
        /// This function stores key/data pairs into the database. 
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
        public void Put(byte[] key, byte[] value, CursorPutOptions options)
        {
            using(var keyMarshalStruct = new MarshalValueStructure(key))
            using (var valueMarshalStruct = new MarshalValueStructure(value))
            {
                var keyStruct = keyMarshalStruct.ValueStructure;
                var valueStruct = valueMarshalStruct.ValueStructure;

                NativeMethods.Execute(lib => lib.mdb_cursor_put(_handle, ref keyStruct, ref valueStruct, options));
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
        public void PutMultiple(byte[] key, byte[][] values)
        {
            using (var keyMarshalStruct = new MarshalValueStructure(key))
            using (var valueMarshalStruct = new MarshalMultipleValueStructure(values))
            {
                var keyStruct = keyMarshalStruct.ValueStructure;
                var valueStruct = valueMarshalStruct.ValueStructures;

                NativeMethods.Execute(lib => lib.mdb_cursor_put(_handle, ref keyStruct, valueStruct, CursorPutOptions.MultipleData));
            }
        }

        //TODO: tests
        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="option">Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     MDB_NODUPDATA - delete all of the data items for the current key. This flag may only be specified if the database was opened with MDB_DUPSORT.</param>
        public void Delete(CursorDeleteOption option)
        {
            NativeMethods.Execute(lib => lib.mdb_cursor_del(_handle, option));
        }

        //TODO: tests
        /// <summary>
        /// Renew a cursor handle.
        /// Cursors are associated with a specific transaction and database and may not span threads. 
        /// Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead. 
        /// The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was created with.
        /// </summary>
        public void Renew()
        {
            this.Renew(this.Transaction);
        }

        //TODO: tests
        /// <summary>
        /// Renew a cursor handle.
        /// Cursors are associated with a specific transaction and database and may not span threads. 
        /// Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead. 
        /// The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was created with.
        /// </summary>
        /// <param name="txn">Transaction to renew in.</param>
        public void Renew(LightningTransaction txn)
        {
            if(txn == null)
                throw new ArgumentNullException("txn");

            if (!txn.IsReadOnly)
                throw new InvalidOperationException("Can't renew cursor on non-readonly transaction");

            NativeMethods.Execute(lib => lib.mdb_cursor_renew(txn._handle, _handle));
        }

        //TODO: tests
        /// <summary>
        /// Close a cursor handle.
        /// The cursor handle will be freed and must not be used again after this call.
        /// </summary>
        public void Close()
        {
            Transaction.CursorManager.CloseCursor(this);
        }

        /// <summary>
        /// Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        /// <param name="shouldDispose">True if not disposed yet.</param>
        protected virtual void Dispose(bool shouldDispose)
        {
            if (shouldDispose)
                this.Close();
        }

        /// <summary>
        /// Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
