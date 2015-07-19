using System;
using System.Collections;
using System.Collections.Generic;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// Lightning database.
    /// </summary>
    public class LightningDatabase : IDisposable, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        private uint _handle;
        private LightningTransaction _transaction;
        private readonly DatabaseConfiguration _configuration;

        /// <summary>
        /// Creates a LightningDatabase instance.
        /// </summary>
        /// <param name="name">Database name.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="configuration">Options for the database, like encoding, option flags, and comparison logic.</param>
        internal LightningDatabase(string name, LightningTransaction transaction, DatabaseConfiguration configuration)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Name = name;
            _configuration = configuration;
            Environment = transaction.Environment;
            _transaction = transaction;
            _transaction.DisposingComplete += Dispose;
            mdb_dbi_open(_transaction.Handle(), name, _configuration.Flags, out _handle);
            _configuration.ConfigureDatabase(_transaction, this);
            IsOpened = true;
        }

        public uint Handle()
        {
            return _handle;
        }

        /// <summary>
        /// Whether the database handle has been release from Dispose, or from unsuccessful OpenDatabase call.
        /// </summary>
        public bool IsReleased => _handle == default(uint);

        /// <summary>
        /// Is database opened.
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// Database name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Environment in which the database was opened.
        /// </summary>
        public LightningEnvironment Environment { get; }

        /// <summary>
        /// Flags with which the database was opened.
        /// </summary>
        public DatabaseOpenFlags OpenFlags { get; private set; }

        /// <summary>
        /// Drops the database.
        /// </summary>
        public void Drop()
        {
            mdb_drop(_transaction.Handle(), _handle, true);
            IsOpened = false;
            _handle = default(uint);
        }

        /// <summary>
        /// Truncates all data from the database.
        /// </summary>
        public void Truncate()
        {
            mdb_drop(_transaction.Handle(), _handle, false);
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> FindAllStartingWith(byte[] keyPrefix)
        {
            using (var cursor = _transaction.CreateCursor(this))
            {
                if (!cursor.MoveToFirstAfter(keyPrefix))
                {
                    yield break;
                }
                do
                {
                    var current = cursor.Current;
                    var currentKey = current.Key;
                    if(currentKey.StartsWith(keyPrefix))
                        yield return current;
                    else
                        yield break;
                } while (cursor.MoveNext());
            }
        }

        /// <summary>
        /// Deallocates resources opened by the database.
        /// </summary>
        /// <param name="disposing">true if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == default(uint))
                return;

            _transaction.DisposingComplete -= Dispose;
            IsOpened = false;
            if (disposing)
            {
                //From finalizer, this will likely throw
                mdb_dbi_close(Environment.Handle(), _handle);
                GC.SuppressFinalize(this);
            }
            _transaction = null;
            _handle = default(uint);
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Enumerates the database with the transaction used to open the database.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return _transaction.CreateCursor(this);
        }

        ~LightningDatabase()
        {
            Dispose(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
