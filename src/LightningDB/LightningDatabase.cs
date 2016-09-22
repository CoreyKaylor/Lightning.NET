using System;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// Lightning database.
    /// </summary>
    public class LightningDatabase : IDisposable
    {
        private uint _handle;
        private readonly DatabaseConfiguration _configuration;
        private readonly IDisposable _pinnedConfig;

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
            Environment.Disposing += Dispose;
            mdb_dbi_open(transaction.Handle(), name, _configuration.Flags, out _handle);
            _pinnedConfig = _configuration.ConfigureDatabase(transaction, this);
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
        public void Drop(LightningTransaction transaction)
        {
            mdb_drop(transaction.Handle(), _handle, true);
            IsOpened = false;
            _handle = default(uint);
        }

        /// <summary>
        /// Truncates all data from the database.
        /// </summary>
        public void Truncate(LightningTransaction transaction)
        {
            mdb_drop(transaction.Handle(), _handle, false);
        }

        /// <summary>
        /// Deallocates resources opened by the database.
        /// </summary>
        /// <param name="disposing">true if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == default(uint))
                return;

            if(!disposing)
                throw new InvalidOperationException("The LightningDatabase was not disposed and cannot be reliably dealt with from the finalizer");

            Environment.Disposing -= Dispose;
            IsOpened = false;
            _pinnedConfig.Dispose();
            mdb_dbi_close(Environment.Handle(), _handle);
            GC.SuppressFinalize(this);
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
