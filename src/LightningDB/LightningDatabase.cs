using System;
using System.Text;
using LightningDB.Factories;
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
        
        private readonly string _name;
        private readonly LightningTransaction _transaction;

        /// <summary>
        /// Creates a LightningDatabase instance.
        /// </summary>
        /// <param name="name">Database name.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="entry">The handle cache for database entries.</param>
        /// <param name="encoding">Default strings encoding.</param>
        internal LightningDatabase(string name, LightningTransaction transaction, DatabaseHandleCacheEntry entry, Encoding encoding)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            _name = name;
            _transaction = transaction;
            _handle = entry.Handle;
                        
            Encoding = encoding;
            OpenFlags = entry.OpenFlags;
        }

        internal bool IsReleased => _handle == default(uint);

        /// <summary>
        /// Is database opened.
        /// </summary>
        public bool IsOpened => Environment.DatabaseManager.IsOpen(this);

        /// <summary>
        /// Database name.
        /// </summary>
        public string Name => _name;

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
        public void Drop(bool truncateDataOnly = false)
        {
            mdb_drop(_transaction._handle, _handle, !truncateDataOnly);
            if (truncateDataOnly)
                return;
            Environment.DatabaseManager.Close(this, false);
            _handle = default(uint);
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        /// <param name="disposing">true if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == default(uint))
                return;

            Environment.DatabaseManager.Close(this, true);
            _handle = default(uint);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
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
