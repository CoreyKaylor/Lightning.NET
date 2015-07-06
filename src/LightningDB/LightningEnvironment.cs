using System;
using System.IO;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    /// <summary>
    /// LMDB Environment.
    /// </summary>
    public class LightningEnvironment : IDisposable
    {
        private readonly IDisposable _binding;
        private readonly EnvironmentConfiguration _config;

        private IntPtr _handle;

        public event Action Disposing;

        /// <summary>
        /// Creates a new instance of LightningEnvironment.
        /// </summary>
        /// <param name="path">Directory for storing database files.</param>
        /// <param name="configuration">Configuration for the environment.</param>
        public LightningEnvironment(string path, EnvironmentConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Invalid directory name");

            _binding = NativeBinding();

            mdb_env_create(out _handle);

            Path = path;

            _config = configuration ?? new EnvironmentConfiguration();
            _config.Configure(this);
        }

        internal IntPtr Handle()
        {
            return _handle;
        }

        private MDBStat GetStat()
        {
            EnsureOpened();

            MDBStat stat;
            mdb_env_stat(_handle, out stat);

            return stat;
        }

        /// <summary>
        /// Whether the environment is opened.
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// Current lmdb version.
        /// </summary>
        public LightningVersionInfo Version => LightningVersionInfo.Get();

        /// Set the size of the memory map to use for this environment.
        /// The size should be a multiple of the OS page size. 
        /// The default is 10485760 bytes. 
        /// The size of the memory map is also the maximum size of the database. 
        /// The value should be chosen as large as possible, to accommodate future growth of the database. 
        /// This function may only be called before the environment is opened. 
        /// The size may be changed by closing and reopening the environment. 
        /// Any attempt to set a size smaller than the space already consumed by the environment will be silently changed to the current size of the used space.
        public long MapSize
        {
            get { return _config.MapSize; }
            set
            {
                if (value == _config.MapSize) 
                    return;

                if (_config.AutoReduceMapSizeIn32BitProcess && IntPtr.Size == 4)
                    _config.MapSize = int.MaxValue;
                else
                    _config.MapSize = value;

                mdb_env_set_mapsize(_handle, _config.MapSize);
            }
        }

        public uint PageSize => GetStat().ms_psize;

        public long UsedSize
        {
            get
            {
                MDBEnvInfo envInfo;
                mdb_env_info(_handle, out envInfo);

                return envInfo.me_last_pgno.ToInt64() * PageSize;
            }
        }

        /// <summary>
        /// Get the maximum number of threads for the environment.
        /// </summary>
        public int MaxReaders
        {
            get
            {
                return _config.MaxReaders;
            }
            set
            {
                if (IsOpened)
                    throw new InvalidOperationException("Can't change MaxReaders of opened environment");

                mdb_env_set_maxreaders(_handle, (uint)value);

                _config.MaxReaders = value;
            }
        }

        /// <summary>
        /// Set the maximum number of named databases for the environment.
        /// This function is only needed if multiple databases will be used in the environment. 
        /// Simpler applications that use the environment as a single unnamed database can ignore this option. 
        /// This function may only be called before the environment is opened.
        /// </summary>
        public int MaxDatabases
        {
            get { return _config.MaxDatabases; }
            set
            {
                if (IsOpened)
                    throw new InvalidOperationException("Can't change MaxDatabases of opened environment");

                if (value == _config.MaxDatabases) 
                    return;

                mdb_env_set_maxdbs(_handle, (uint)value);

                _config.MaxDatabases = value;
            }
        }

        /// <summary>
        /// Will return the count of names for each database stored in the default database.
        /// </summary>
        public long EntriesCount => GetStat().ms_entries.ToInt64();

        /// <summary>
        /// Directory path to store database files.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Open the environment.
        /// </summary>
        public void Open(EnvironmentOpenFlags openFlags = EnvironmentOpenFlags.None, UnixAccessMode accessMode = UnixAccessMode.Default)
        {
            if(IsOpened)
                throw new InvalidOperationException("Environment is already opened.");

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            mdb_env_open(_handle, Path, openFlags, accessMode);

            IsOpened = true;
        }

        /// <summary>
        /// Create a transaction for use with the environment.
        /// The transaction handle may be discarded using Abort() or Commit().
        /// Note:
        /// Transactions may not span threads; a transaction must only be used by a single thread. Also, a thread may only have a single transaction.
        /// Cursors may not span transactions; each cursor must be opened and closed within a single transaction.
        /// </summary>
        /// <param name="parent">
        /// If this parameter is non-NULL, the new transaction will be a nested transaction, with the transaction indicated by parent as its parent. 
        /// Transactions may be nested to any level. 
        /// A parent transaction may not issue any other operations besides BeginTransaction, Abort, or Commit while it has active child transactions.
        /// </param>
        /// <param name="beginFlags">
        /// Special options for this transaction. 
        /// </param>
        /// <returns>
        /// New LightningTransaction
        /// </returns>
        public LightningTransaction BeginTransaction(LightningTransaction parent, TransactionBeginFlags beginFlags)
        {
            return new LightningTransaction(this, parent, beginFlags);
        }

        /// <summary>
        /// Create a transaction for use with the environment.
        /// The transaction handle may be discarded usingAbort() or Commit().
        /// Note:
        /// Transactions may not span threads; a transaction must only be used by a single thread. Also, a thread may only have a single transaction.
        /// Cursors may not span transactions; each cursor must be opened and closed within a single transaction.
        /// </summary>
        /// <param name="beginFlags">
        /// Special options for this transaction. 
        /// </param>
        /// <returns>
        /// New LightningTransaction
        /// </returns>
        public LightningTransaction BeginTransaction(TransactionBeginFlags beginFlags)
        {
            return BeginTransaction(null, beginFlags);
        }

        /// <summary>
        /// Create a transaction for use with the environment.
        /// The transaction handle may be discarded using Abort() or Commit().
        /// Note:
        /// Transactions may not span threads; a transaction must only be used by a single thread. Also, a thread may only have a single transaction.
        /// Cursors may not span transactions; each cursor must be opened and closed within a single transaction.
        /// </summary>        
        /// <returns>
        /// New LightningTransaction
        /// </returns>
        public LightningTransaction BeginTransaction()
        {
            return BeginTransaction(null, LightningTransaction.DefaultTransactionBeginFlags);
        }

        /// <summary>
        /// Copy an MDB environment to the specified path.
        /// This function may be used to make a backup of an existing environment.
        /// </summary>
        /// <param name="path">The directory in which the copy will reside. This directory must already exist and be writable but must otherwise be empty.</param>
        /// <param name="compact">Omit empty pages when copying.</param>
        public void CopyTo(string path, bool compact = false)
        {
            EnsureOpened();

            var flags = compact 
                ? EnvironmentCopyFlags.Compact 
                : EnvironmentCopyFlags.None;
            
            mdb_env_copy2(_handle, path, flags);
        }

        //TODO: tests
        /// <summary>
        /// Flush the data buffers to disk. 
        /// Data is always written to disk when LightningTransaction.Commit is called, but the operating system may keep it buffered. 
        /// MDB always flushes the OS buffers upon commit as well, unless the environment was opened with EnvironmentOpenFlags.NoSync or in part EnvironmentOpenFlags.NoMetaSync.
        /// </summary>
        /// <param name="force">If true, force a synchronous flush. Otherwise if the environment has the EnvironmentOpenFlags.NoSync flag set the flushes will be omitted, and with MDB_MAPASYNC they will be asynchronous.</param>
        public void Flush(bool force)
        {
            mdb_env_sync(_handle, force);
        }

        private void EnsureOpened()
        {
            if (!IsOpened)
                throw new InvalidOperationException("Environment should be opened");
        }

        /// <summary>
        /// Disposes the environment and deallocates all resources associated with it.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero)
                return;

            var copy = Disposing;
            copy?.Invoke();

            if (IsOpened)
            {
                mdb_env_close(_handle);
                IsOpened = false;
            }

            _binding.Dispose();
            _handle = IntPtr.Zero;
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Dispose the environment and release the memory map.
        /// Only a single thread may call this function. All transactions, databases, and cursors must already be closed before calling this function. 
        /// Attempts to use any such handles after calling this function will cause a SIGSEGV. 
        /// The environment handle will be freed and must not be used again after this call.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~LightningEnvironment()
        {
            Dispose(false);
        }
    }
}