using System;
using System.Collections;
using System.Collections.Generic;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// Cursor to iterate over a database
    /// </summary>
    public class LightningCursor : IEnumerator<KeyValuePair<byte[], byte[]>>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        private IntPtr _handle;
        private ValueStructure _currentKeyStructure;
        private ValueStructure _currentValueStructure;

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

            mdb_cursor_open(txn.Handle(), db.Handle(), out _handle);

            Transaction = txn;
            Transaction.Disposing += Dispose;
        }

        public IntPtr Handle()
        {
            return _handle;
        }

        /// <summary>
        /// Cursor's transaction.
        /// </summary>
        public LightningTransaction Transaction { get; }

        public void Reset()
        {
            Renew();
        }

        object IEnumerator.Current => Current;

        /// <summary>
        /// The current item the cursor is pointed to, or default KeyValuePair&lt;byte[], byte[]&gt;
        /// </summary>
        public KeyValuePair<byte[], byte[]> Current
        {
            get
            {
                if (_currentKeyStructure.size == IntPtr.Zero)
                    return default(KeyValuePair<byte[], byte[]>);

                return new KeyValuePair<byte[], byte[]>(_currentKeyStructure.GetBytes(),
                    _currentValueStructure.GetBytes());
            }
        }

        /// <summary>
        /// Position at specified key, Current will not be populated.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the key was found.</returns>
        public bool MoveTo(byte[] key)
        {
            return Get(CursorOperation.Set, key);
        }

        /// <summary>
        /// Moves to the key and populates Current with the values stored.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the key was found.</returns>
        public bool MoveToAndGet(byte[] key)
        {
            return Get(CursorOperation.SetKey, key);
        }

        /// <summary>
        /// Position at key/data pair. Only for MDB_DUPSORT
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the key/value pair was found.</returns>
        public bool MoveTo(byte[] key, byte[] value)
        {
            return Get(CursorOperation.GetBoth, key, value);
        }

        /// <summary>
        /// position at key, nearest data. Only for MDB_DUPSORT
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the key/value pair is found.</returns>
        public bool MoveToFirstValueAfter(byte[] key, byte[] value)
        {
            return Get(CursorOperation.GetBothRange, key, value);
        }

        /// <summary>
        /// Position at first key greater than or equal to specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the key is found and had one more item after it to advance to.</returns>
        public bool MoveToFirstAfter(byte[] key)
        {
            return Get(CursorOperation.SetRange, key);
        }

        /// <summary>
        /// Position at first key/data item
        /// </summary>
        /// <returns>True if first pair is found.</returns>
        public bool MoveToFirst()
        {
            return Get(CursorOperation.First);
        }

        /// <summary>
        /// Position at first data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>True if first duplicate is found.</returns>
        public bool MoveToFirstDuplicate()
        {
            return Get(CursorOperation.FirstDuplicate);
        }

        /// <summary>
        /// Position at last key/data item
        /// </summary>
        /// <returns>True if last pair is found.</returns>
        public bool MoveToLast()
        {
            return Get(CursorOperation.Last);
        }

        /// <summary>
        /// Position at last data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>True if last duplicate is found.</returns>
        public bool MoveToLastDuplicate()
        {
            return Get(CursorOperation.LastDuplicate);
        }

        /// <summary>
        /// Return key/data at current cursor position
        /// </summary>
        /// <returns>Key/data at current cursor position</returns>
        public KeyValuePair<byte[], byte[]> GetCurrent()
        {
            Get(CursorOperation.GetCurrent);
            return Current;
        }

        /// <summary>
        /// Position at next data item
        /// </summary>
        /// <returns>True if next item exists.</returns>
        public bool MoveNext()
        {
            return Get(CursorOperation.Next);
        }

        /// <summary>
        /// Position at next data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>True if next duplicate exists.</returns>
        public bool MoveNextDuplicate()
        {
            return Get(CursorOperation.NextDuplicate);
        }

        /// <summary>
        /// Position at first data item of next key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>True if items exists without duplicates.</returns>
        public bool MoveNextNoDuplicate()
        {
            return Get(CursorOperation.NextNoDuplicate);
        }

        /// <summary>
        /// Return up to a page of duplicate data items at the next cursor position. Only for MDB_DUPFIXED
        /// It is assumed you know the array size to break up a single byte[] into byte[][].
        /// </summary>
        /// <returns>Returns true if duplicates are found.</returns>
        public bool MoveNextMultiple()
        {
            return GetMultiple(CursorOperation.NextMultiple);
        }

        /// <summary>
        /// Position at previous data item.
        /// </summary>
        /// <returns>Returns true if previous item is found.</returns>
        public bool MovePrev()
        {
            return Get(CursorOperation.Previous);
        }

        /// <summary>
        /// Position at previous data item of current key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public bool MovePrevDuplicate()
        {
            return Get(CursorOperation.PreviousDuplicate);
        }

        /// <summary>
        /// Position at last data item of previous key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>True if previous entry without duplicate is found.</returns>
        public bool MovePrevNoDuplicate()
        {
            return Get(CursorOperation.PreviousNoDuplicate);
        }
        
        private bool Get(CursorOperation operation)
        {
            _currentKeyStructure = default(ValueStructure);
            _currentValueStructure = default(ValueStructure);
            
            return mdb_cursor_get(_handle, out _currentKeyStructure, out _currentValueStructure, operation) == 0;
        }

        private bool Get(CursorOperation operation, byte[] key)
        {
            _currentValueStructure = default(ValueStructure);
            return mdb_cursor_get(_handle, key, out _currentKeyStructure, out _currentValueStructure, operation) == 0;
        }

        private bool Get(CursorOperation operation, byte[] key, byte[] value)
        {
            return mdb_cursor_get(_handle, key, value, operation) == 0;
        }

        private bool GetMultiple(CursorOperation operation)
        {
            return mdb_cursor_get_multiple(_handle, ref _currentKeyStructure, ref _currentValueStructure, operation) == 0;
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
        public void Put(byte[] key, byte[] value, CursorPutOptions options)
        {
            mdb_cursor_put(_handle, key, value, options);
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
            using (var marshal = new MarshalMultipleValueStructure(key, values))
                mdb_cursor_put(_handle, ref marshal.Key, marshal.Values, CursorPutOptions.MultipleData);
        }

        /// <summary>
        /// Return up to a page of the duplicate data items at the current cursor position. Only for MDB_DUPFIXED
        /// It is assumed you know the array size to break up a single byte[] into byte[][].
        /// </summary>
        /// <returns>True if key and multiple items are found.</returns>
        public bool GetMultiple()
        {
            return GetMultiple(CursorOperation.GetMultiple);
        }

        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="option">Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     MDB_NODUPDATA - delete all of the data items for the current key. This flag may only be specified if the database was opened with MDB_DUPSORT.</param>
        private void Delete(CursorDeleteOption option)
        {
            mdb_cursor_del(_handle, option);
        }

        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data range for which duplicates are found.
        /// </summary>
        public void DeleteDuplicates()
        {
            Delete(CursorDeleteOption.NoDuplicateData);
        }

        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        public void Delete()
        {
            Delete(CursorDeleteOption.None);
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
            Renew(Transaction);
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
                throw new ArgumentNullException(nameof(txn));

            if (!txn.IsReadOnly)
                throw new InvalidOperationException("Can't renew cursor on non-readonly transaction");

            mdb_cursor_renew(txn.Handle(), _handle);
        }

        /// <summary>
        /// Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero)
                return;

            if (!disposing)
                throw new InvalidOperationException("The LightningCursor was not disposed and cannot be reliably dealt with from the finalizer");

            mdb_cursor_close(_handle);
            _handle = IntPtr.Zero;

            Transaction.Disposing -= Dispose;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~LightningCursor()
        {
            Dispose(false);
        }
    }
}
