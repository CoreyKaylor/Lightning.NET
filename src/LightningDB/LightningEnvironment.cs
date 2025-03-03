using System;
using System.IO;
using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
/// Represents a managed environment for lightning-fast database storage and retrieval. Typically, your application
/// will have one instance of LightningEnvironment. Also, it is possible for separate processes to read from the
/// same environment when set to read-only.
/// </summary>
public sealed class LightningEnvironment : IDisposable
{
    private readonly EnvironmentConfiguration _config = new();
    private bool _disposed;

    internal nint _handle;

    /// <summary>
    /// Creates a new instance of LightningEnvironment.
    /// </summary>
    /// <param name="path">Directory for storing database files.</param>
    /// <param name="configuration">Configuration for the environment.</param>
    public LightningEnvironment(string path, EnvironmentConfiguration configuration = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid directory name");

        var config = configuration ?? _config;

        mdb_env_create(out _handle).ThrowOnError();
        config.Configure(this);
        _config = config;

        Path = path;

    }

    /// <summary>
    /// Whether the environment is opened.
    /// </summary>
    public bool IsOpened { get; private set; }

    /// <summary>
    /// Current lmdb version.
    /// </summary>
    public LightningVersionInfo Version => LightningVersionInfo.Get();


    /// <summary>
    /// Gets or Sets the size of the memory map to use for this environment.
    /// The size of the memory map is also the maximum size of the database.
    /// The size should be a multiple of the OS page size.
    /// The default is 10485760 bytes.
    /// </summary>
    /// <remarks>
    /// The value should be chosen as large as possible, to accommodate future growth of the database.
    /// This function may only be called before the environment is opened.
    /// The size may be changed by closing and reopening the environment.
    /// Any attempt to set a size smaller than the space already consumed by the environment will be silently changed to the current size of the used space.
    /// </remarks>
    public unsafe long MapSize
    {
        get => _config.MapSize;
        set
        {
            if (value == _config.MapSize)
                return;

            if (_config.AutoReduceMapSizeIn32BitProcess && sizeof(nint) == 4)
                _config.MapSize = int.MaxValue;
            else
                _config.MapSize = value;

            mdb_env_set_mapsize(_handle, _config.MapSize).ThrowOnError();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of threads/reader slots for the environment.
    /// </summary>
    /// <remarks>
    /// This defines the number of slots in the lock table that is used to track readers in the
    /// environment. The default is 126. Starting a read-only transaction normally ties a lock table
    /// slot to the current thread until the environment closes or the thread exits. If
    /// <see cref="LightningTransaction.Reset"/> is called on the transaction, the slot can be reused immediately.
    ///
    /// This function may only be called before the environment is opened.
    ///
    /// If this function is never called, the default value will be used.
    ///
    /// Note that the lock table size is also the maximum number of concurrent read-only transactions
    /// that can be in progress at once. Any attempt to start a read-only transaction when the lock
    /// table is full will result in an MDB_READERS_FULL error.
    /// </remarks>
    public int MaxReaders
    {
        get
        {
            mdb_env_get_maxreaders(_handle, out var readers).ThrowOnError();
            return (int) readers;
        }
        set
        {
            if (IsOpened)
                throw new InvalidOperationException("Can't change MaxReaders of opened environment");

            mdb_env_set_maxreaders(_handle, (uint)value).ThrowOnError();

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
        get => _config.MaxDatabases;
        set
        {
            if (IsOpened)
                throw new InvalidOperationException("Can't change MaxDatabases of opened environment");

            if (value == _config.MaxDatabases)
                return;

            mdb_env_set_maxdbs(_handle, (uint)value).ThrowOnError();

            _config.MaxDatabases = value;
        }
    }

    /// <summary>
    /// Get statistics about the LMDB environment.
    /// </summary>
    public Stats EnvironmentStats
    {
        get
        {
            mdb_env_stat(_handle, out var nativeStat);
            return new Stats
            {
                BranchPages = nativeStat.ms_branch_pages,
                BTreeDepth = nativeStat.ms_depth,
                Entries = nativeStat.ms_entries,
                LeafPages = nativeStat.ms_leaf_pages,
                OverflowPages = nativeStat.ms_overflow_pages,
                PageSize = nativeStat.ms_psize
            };
        }
    }

    /// <summary>
    /// Gets information about the LMDB environment.
    /// </summary>
    public EnvironmentInfo Info
    {
        get
        {
            mdb_env_info(_handle, out var nativeInfo);
            return new EnvironmentInfo
            {
                MapSize = nativeInfo.me_mapsize,
                LastPageNumber = nativeInfo.me_last_pgno,
                LastTransactionId = nativeInfo.me_last_txnid
            };
        }
    }

    /// <summary>
    /// Gets the maximum size of keys and MDB_DUPSORT data we can write.
    /// </summary>
    /// <remarks>
    /// This is the maximum size for a key in a database, or the data for a
    /// database with MDB_DUPSORT. This property returns a size in bytes.
    /// Attempting to write keys or data larger than this size will result
    /// in an MDB_BAD_VALSIZE error.
    /// </remarks>
    public int MaxKeySize
    {
        get
        {
            EnsureOpened();
            return mdb_env_get_maxkeysize(_handle);
        }
    }

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

        if (!openFlags.HasFlag(EnvironmentOpenFlags.NoSubDir) && !Directory.Exists(Path))
            Directory.CreateDirectory(Path);

        try
        {
            mdb_env_open(_handle, Path, openFlags, accessMode).ThrowOnError();
        }
        catch(Exception ex)
        {
            throw new LightningException($"Failed to open environment at path {Path}", ex);
        }

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
    public LightningTransaction BeginTransaction(LightningTransaction parent = null, TransactionBeginFlags beginFlags = LightningTransaction.DefaultTransactionBeginFlags)
    {
        if (!IsOpened)
            throw new InvalidOperationException("Environment must be opened before starting a transaction");

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
    /// Copy an MDB environment to the specified path.
    /// This function may be used to make a backup of an existing environment.
    /// </summary>
    /// <param name="path">The directory in which the copy will reside. This directory must already exist and be writable but must otherwise be empty.</param>
    /// <param name="compact">Omit empty pages when copying.</param>
    /// <remarks>
    /// This call can trigger a significant file size growth if run in parallel with write transactions,
    /// because it employs a read-only transaction. If the environment has a large proportion of
    /// free pages, this can cause the backup to be larger than the actual data set.
    ///
    /// It is possible to run this function with writeable transactions in progress. The copy will be
    /// a consistent snapshot of the environment at the time the copy is made, using a read-only transaction.
    ///
    /// When the compact parameter is true (using the MDB_CP_COMPACT flag), the copy will omit free pages
    /// and use smaller page sizes when possible, resulting in a smaller data file.
    /// </remarks>
    public MDBResultCode CopyTo(string path, bool compact = false)
    {
        EnsureOpened();

        var flags = compact
            ? EnvironmentCopyFlags.Compact
            : EnvironmentCopyFlags.None;

        return mdb_env_copy2(_handle, path, flags);
    }

    /// <summary>
    /// Flush the data buffers to disk.
    /// Data is always written to disk when LightningTransaction.Commit is called, but the operating system may keep it buffered.
    /// MDB always flushes the OS buffers upon commit as well, unless the environment was opened with EnvironmentOpenFlags.NoSync or in part EnvironmentOpenFlags.NoMetaSync.
    /// </summary>
    /// <param name="force">If true, force a synchronous flush. Otherwise if the environment has the EnvironmentOpenFlags.NoSync flag set the flushes will be omitted, and with MDB_MAPASYNC they will be asynchronous.</param>
    /// <remarks>
    /// This call is not needed if your code does not care if/when data is physically written to the disk.
    /// It is the application's responsibility to ensure the persistence of critical data. The
    /// actual decision to use synchronous or asynchronous flushes typically depends on:
    ///
    /// - Safety: Synchronous flushes guarantee that data has been written to disk before continuing.
    /// - Performance: Asynchronous flushes allow the OS to schedule disk writes optimally, which can be much faster.
    ///
    /// If your environment was opened with <see cref="EnvironmentOpenFlags.NoSync"/>, calling Flush(false) will have no effect,
    /// while Flush(true) will still force a synchronous flush.
    ///
    /// If your environment was opened with <see cref="EnvironmentOpenFlags.MapAsync"/>, calling Flush(false) will perform an
    /// asynchronous flush, while Flush(true) will still force a synchronous flush.
    ///
    /// Even when this method is not called, the data will be persisted to disk when the OS flushes
    /// its buffers or when the environment is closed properly.
    /// </remarks>
    public MDBResultCode Flush(bool force)
    {
        return mdb_env_sync(_handle, force);
    }

    /// <summary>
    /// Gets or sets the environment flags.
    /// When setting flags, they will be either set or cleared based on the value.
    /// This is a direct wrapper around the mdb_env_get_flags and mdb_env_set_flags functions.
    /// </summary>
    /// <remarks>
    /// Only certain flags can be changed after the environment is opened:
    /// <list type="bullet">
    ///   <item><description>NoSync</description></item>
    ///   <item><description>NoMetaSync</description></item>
    ///   <item><description>MapAsync</description></item>
    ///   <item><description>NoReadAhead</description></item>
    ///   <item><description>NoMemoryInitialization</description></item>
    /// </list>
    /// </remarks>
    public EnvironmentOpenFlags Flags
    {
        get
        {
            EnsureOpened();
            mdb_env_get_flags(_handle, out var flags).ThrowOnError();
            return (EnvironmentOpenFlags)flags;
        }
        set
        {
            EnsureOpened();

            // Get current flags
            mdb_env_get_flags(_handle, out var currentFlags).ThrowOnError();

            // Determine which flags to turn on and which to turn off by converting to uint
            var newFlags = (uint)value;
            var oldFlags = currentFlags;

            var flagsToEnable = newFlags & ~oldFlags;   // Flags in new value but not in current
            var flagsToDisable = oldFlags & ~newFlags;  // Flags in current but not in new value

            // Turn on new flags
            if (flagsToEnable != 0)
            {
                mdb_env_set_flags(_handle, flagsToEnable, true).ThrowOnError();
            }

            // Turn off removed flags
            if (flagsToDisable != 0)
            {
                mdb_env_set_flags(_handle, flagsToDisable, false).ThrowOnError();
            }
        }
    }

    /// <summary>
    /// Checks for stale readers in the environment and cleans them up.
    /// </summary>
    /// <returns>The number of stale readers that were cleared</returns>
    /// <remarks>
    /// Reader slots are tied to a thread for the lifetime of a transaction, but if a thread
    /// terminates abnormally while a transaction is active, the slot remains "owned" by the thread
    /// and is unavailable for reuse. This function identifies such stale readers and makes their
    /// slots available for reuse.
    ///
    /// This function should be called periodically in long-running applications or when
    /// BeginTransaction returns <see cref="MDBResultCode.ReadersFull"/>.
    ///
    /// Readers don't use any locks, so stale readers won't cause writers to wait. However, they do
    /// take up reader slots which could prevent other processes from starting read transactions.
    /// </remarks>
    public int CheckStaleReaders()
    {
        EnsureOpened();
        var result = mdb_reader_check(_handle, out var deadReaders);
        if (result != 0)
            throw new LightningException($"Failed to check stale readers", result);
        return deadReaders;
    }

    /// <summary>
    /// Gets a read access FileStream for the environment's file descriptor.
    /// </summary>
    /// <returns>A FileStream that wraps the environment's file descriptor</returns>
    public FileStream GetFileStream()
    {
        EnsureOpened();

        // Get the raw file descriptor
        mdb_env_get_fd(_handle, out var fd).ThrowOnError();

        // Create a SafeFileHandle from the file descriptor
        // Note: We set ownsHandle to false because LMDB owns the file descriptor
        var safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(fd, ownsHandle: false);

        // Create a FileStream from the SafeFileHandle
        return new FileStream(safeHandle, FileAccess.Read);
    }

    /// <summary>
    /// Copies an LMDB environment to the specified FileStream.
    /// This method accepts a standard .NET FileStream and extracts the file descriptor.
    /// </summary>
    /// <param name="fileStream">The FileStream to write to</param>
    /// <param name="compact">Whether to compact the environment during copy</param>
    /// <returns>A result code indicating success or failure</returns>
    /// <exception cref="ArgumentNullException">Thrown when fileStream is null</exception>
    /// <exception cref="ArgumentException">Thrown when the FileStream is not writable</exception>
    /// <exception cref="NotSupportedException">Thrown when the platform doesn't support getting file handles from FileStream</exception>
    public MDBResultCode CopyToStream(FileStream fileStream, bool compact = false)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        if (!fileStream.CanWrite)
            throw new ArgumentException("FileStream must be writable", nameof(fileStream));

        EnsureOpened();

        // Get the SafeFileHandle from the FileStream
        var safeHandle = fileStream.SafeFileHandle;
        if (safeHandle == null || safeHandle.IsInvalid)
            throw new ArgumentException("Invalid file handle from FileStream", nameof(fileStream));

        // Get the file descriptor as IntPtr/nint
        var fd = safeHandle.DangerousGetHandle();

        return compact
            ? mdb_env_copyfd2(_handle, fd, EnvironmentCopyFlags.Compact)
            : mdb_env_copyfd(_handle, fd);
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
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        if(!disposing)
            throw new InvalidOperationException("The LightningEnvironment was not disposed and cannot be reliably dealt with from the finalizer");

        if (IsOpened)
        {
            mdb_env_close(_handle);
            IsOpened = false;
        }

        _handle = 0;
        GC.SuppressFinalize(this);
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
