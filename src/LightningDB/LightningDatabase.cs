using System;
using System.Text;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// Lightning database.
    /// </summary>
    public class LightningDatabase : IDisposable
    {
        /// <summary>
        /// Database name by default.
        /// </summary>
        public const string DefaultDatabaseName = "master";

        internal uint _handle;

        private readonly LightningTransaction _transaction;

        /// <summary>
        /// Creates a LightningDatabase instance.
        /// </summary>
        /// <param name="name">Database name.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="encoding">Default strings encoding.</param>
        /// <param name="openFlags">The open flag options for the database.</param>
        internal LightningDatabase(string name, LightningTransaction transaction, Encoding encoding, DatabaseOpenFlags openFlags)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            Name = name;
            _transaction = transaction;
            _transaction.Environment.Disposing += Dispose;
            mdb_dbi_open(transaction._handle, name, openFlags, out _handle);
            IsOpened = true;
                        
            Encoding = encoding;
            OpenFlags = openFlags;
        }

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
        /// Default strings encoding.
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// Environment in which the database was opened.
        /// </summary>
        public LightningEnvironment Environment => _transaction.Environment;

        /// <summary>
        /// Flags with which the database was opened.
        /// </summary>
        public DatabaseOpenFlags OpenFlags { get; private set; }

        /// <summary>
        /// Drops the database.
        /// </summary>
        public void Drop()
        {
            mdb_drop(_transaction._handle, _handle, true);
            IsOpened = false;
            _handle = default(uint);
        }

        /// <summary>
        /// Truncates all data from the database.
        /// </summary>
        public void Truncate()
        {
            mdb_drop(_transaction._handle, _handle, false);
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        /// <param name="disposing">true if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == default(uint))
                return;

            _transaction.Environment.Disposing -= Dispose;
            IsOpened = false;
            if (disposing)
            {
                //From finalizer, this will likely throw
                mdb_dbi_close(_transaction.Environment._handle, _handle);
                GC.SuppressFinalize(this);
            }
            _handle = default(uint);
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~LightningDatabase()
        {
            Dispose(false);
        }
    }
}
