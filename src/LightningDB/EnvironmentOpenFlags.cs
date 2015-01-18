using System;

namespace LightningDB
{
    /// <summary>
    /// Options to open LMDB environment
    /// </summary>
    [Flags]
    public enum EnvironmentOpenFlags
    {
        /// <summary>
        /// No special options.
        /// </summary>
        None = 0,

        /// <summary>
        /// MDB_FIXEDMAP. use a fixed address for the mmap region. 
        /// This flag must be specified when creating the environment, and is stored persistently in the environment. 
        /// If successful, the memory map will always reside at the same virtual address and pointers used to reference data items in the database will be constant across multiple invocations. 
        /// This option may not always work, depending on how the operating system has allocated memory to shared libraries and other uses. 
        /// The feature is highly experimental.
        /// </summary>
        FixedMap = 0x01,

        /// <summary>
        /// MDB_NOSUBDIR. By default, MDB creates its environment in a directory whose pathname is given in path, and creates its data and lock files under that directory. 
        /// With this option, path is used as-is for the database main data file. 
        /// The database lock file is the path with "-lock" appended.
        /// </summary>
        NoSubDir = 0x4000,

        /// <summary>
        /// MDB_NOSYNC. Don't flush system buffers to disk when committing a transaction. 
        /// This optimization means a system crash can corrupt the database or lose the last transactions if buffers are not yet flushed to disk. 
        /// The risk is governed by how often the system flushes dirty buffers to disk and how often mdb_env_sync() is called. 
        /// However, if the filesystem preserves write order and the MDB_WRITEMAP flag is not used, transactions exhibit ACI (atomicity, consistency, isolation) properties and only lose D (durability). 
        /// I.e. database integrity is maintained, but a system crash may undo the final transactions. 
        /// Note that (MDB_NOSYNC | MDB_WRITEMAP) leaves the system with no hint for when to write transactions to disk, unless mdb_env_sync() is called. 
        /// (MDB_MAPASYNC | MDB_WRITEMAP) may be preferable. 
        /// This flag may be changed at any time using mdb_env_set_flags().
        /// </summary>
        NoSync = 0x10000,
        
        /// <summary>
        /// MDB_RDONLY. Open the environment in read-only mode. 
        /// No write operations will be allowed. 
        /// MDB will still modify the lock file - except on read-only filesystems, where MDB does not use locks.
        /// </summary>
        ReadOnly = 0x20000,

        /// <summary>
        /// MDB_NOMETASYNC. Flush system buffers to disk only once per transaction, omit the metadata flush. 
        /// Defer that until the system flushes files to disk, or next non-MDB_RDONLY commit or mdb_env_sync(). 
        /// This optimization maintains database integrity, but a system crash may undo the last committed transaction. 
        /// I.e. it preserves the ACI (atomicity, consistency, isolation) but not D (durability) database property. 
        /// This flag may be changed at any time using mdb_env_set_flags().
        /// </summary>
        NoMetaSync = 0x40000,
        
        /// <summary>
        /// MDB_WRITEMAP Use a writeable memory map unless MDB_RDONLY is set. 
        /// This is faster and uses fewer mallocs, but loses protection from application bugs like wild pointer writes and other bad updates into the database. 
        /// Incompatible with nested transactions.
        /// </summary>
        WriteMap = 0x80000, 

        /// <summary>
        /// MDB_MAPASYNC. When using MDB_WRITEMAP, use asynchronous flushes to disk. 
        /// As with MDB_NOSYNC, a system crash can then corrupt the database or lose the last transactions. 
        /// Calling mdb_env_sync() ensures on-disk database integrity until next commit. 
        /// This flag may be changed at any time using mdb_env_set_flags().
        /// </summary>
        MapAsync = 0x100000,

        /// <summary>
        /// MDB_NOTLS. tie reader locktable slots to MDB_txn objects instead of to threads
        /// </summary>
        NoThreadLocalStorage = 0x200000,

        /// <summary>
        /// MDB_NOLOCK. don't do any locking, caller must manage their own locks
        /// </summary>
        NoLock = 0x400000,

        /// <summary>
        /// MDB_NORDAHEAD. don't do readahead (no effect on Windows)
        /// </summary>
        NoReadAhead = 0x800000,

        /// <summary>
        /// MDB_NOMEMINIT. don't initialize malloc'd memory before writing to datafile
        /// </summary>
        NoMemoryInitialization = 0x1000000

    }
}
