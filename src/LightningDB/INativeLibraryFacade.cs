using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    interface INativeLibraryFacade
    {
        /// <summary>
        /// Create an MDB environment handle.
        /// This function allocates memory for a MDB_env structure. 
        /// To release the allocated memory and discard the handle, call mdb_env_close(). 
        /// Before the handle may be used, it must be opened using mdb_env_open(). 
        /// Various other options may also need to be set before opening the handle, e.g. mdb_env_set_mapsize(), mdb_env_set_maxreaders(), mdb_env_set_maxdbs(), depending on usage requirements.
        /// </summary>
        /// <param name="env">The address where the new handle will be stored</param>
        /// <returns>A non-zero error value on failure and 0 on success.</returns>
        int mdb_env_create(out IntPtr env);

        /// <summary>
        /// Close the environment and release the memory map.
        /// Only a single thread may call this function. All transactions, databases, and cursors must already be closed before calling this function. 
        /// Attempts to use any such handles after calling this function will cause a SIGSEGV. 
        /// The environment handle will be freed and must not be used again after this call.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        void mdb_env_close(IntPtr env);

        /// <summary>
        /// Open an environment handle.
        /// If this function fails, mdb_env_close() must be called to discard the MDB_env handle.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="path">The directory in which the database files reside. This directory must already exist and be writable.</param>
        /// <param name="flags">
        /// Special options for this environment. This parameter must be set to 0 or by bitwise OR'ing together one or more of the values described here. Flags set by mdb_env_set_flags() are also used.
        ///     MDB_FIXEDMAP use a fixed address for the mmap region. This flag must be specified when creating the environment, and is stored persistently in the environment. If successful, the memory map will always reside at the same virtual address and pointers used to reference data items in the database will be constant across multiple invocations. This option may not always work, depending on how the operating system has allocated memory to shared libraries and other uses. The feature is highly experimental.
        ///     MDB_NOSUBDIR By default, MDB creates its environment in a directory whose pathname is given in path, and creates its data and lock files under that directory. With this option, path is used as-is for the database main data file. The database lock file is the path with "-lock" appended.
        ///     MDB_RDONLY Open the environment in read-only mode. No write operations will be allowed. MDB will still modify the lock file - except on read-only filesystems, where MDB does not use locks.
        ///     MDB_WRITEMAP Use a writeable memory map unless MDB_RDONLY is set. This is faster and uses fewer mallocs, but loses protection from application bugs like wild pointer writes and other bad updates into the database. Incompatible with nested transactions.
        ///     MDB_NOMETASYNC Flush system buffers to disk only once per transaction, omit the metadata flush. Defer that until the system flushes files to disk, or next non-MDB_RDONLY commit or mdb_env_sync(). This optimization maintains database integrity, but a system crash may undo the last committed transaction. I.e. it preserves the ACI (atomicity, consistency, isolation) but not D (durability) database property. This flag may be changed at any time using mdb_env_set_flags().
        ///     MDB_NOSYNC Don't flush system buffers to disk when committing a transaction. This optimization means a system crash can corrupt the database or lose the last transactions if buffers are not yet flushed to disk. The risk is governed by how often the system flushes dirty buffers to disk and how often mdb_env_sync() is called. However, if the filesystem preserves write order and the MDB_WRITEMAP flag is not used, transactions exhibit ACI (atomicity, consistency, isolation) properties and only lose D (durability). I.e. database integrity is maintained, but a system crash may undo the final transactions. Note that (MDB_NOSYNC | MDB_WRITEMAP) leaves the system with no hint for when to write transactions to disk, unless mdb_env_sync() is called. (MDB_MAPASYNC | MDB_WRITEMAP) may be preferable. This flag may be changed at any time using mdb_env_set_flags().
        ///     MDB_MAPASYNC When using MDB_WRITEMAP, use asynchronous flushes to disk. As with MDB_NOSYNC, a system crash can then corrupt the database or lose the last transactions. Calling mdb_env_sync() ensures on-disk database integrity until next commit. This flag may be changed at any time using mdb_env_set_flags().
        /// </param>
        /// <param name="mode">The UNIX permissions to set on created files. This parameter is ignored on Windows.</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_VERSION_MISMATCH - the version of the MDB library doesn't match the version that created the database environment.
        ///     MDB_INVALID - the environment file headers are corrupted.
        ///     ENOENT - the directory specified by the path parameter doesn't exist.
        ///     EACCES - the user didn't have permission to access the environment files.
        ///     EAGAIN - the environment was locked by another process.
        /// </returns>
        int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode); //OK

        /// <summary>
        /// Open an environment handle.
        /// If this function fails, mdb_env_close() must be called to discard the MDB_env handle.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="path">The directory in which the database files reside. This directory must already exist and be writable.</param>
        /// <param name="flags">
        /// Special options for this environment. This parameter must be set to 0 or by bitwise OR'ing together one or more of the values described here. Flags set by mdb_env_set_flags() are also used.
        ///     MDB_FIXEDMAP use a fixed address for the mmap region. This flag must be specified when creating the environment, and is stored persistently in the environment. If successful, the memory map will always reside at the same virtual address and pointers used to reference data items in the database will be constant across multiple invocations. This option may not always work, depending on how the operating system has allocated memory to shared libraries and other uses. The feature is highly experimental.
        ///     MDB_NOSUBDIR By default, MDB creates its environment in a directory whose pathname is given in path, and creates its data and lock files under that directory. With this option, path is used as-is for the database main data file. The database lock file is the path with "-lock" appended.
        ///     MDB_RDONLY Open the environment in read-only mode. No write operations will be allowed. MDB will still modify the lock file - except on read-only filesystems, where MDB does not use locks.
        ///     MDB_WRITEMAP Use a writeable memory map unless MDB_RDONLY is set. This is faster and uses fewer mallocs, but loses protection from application bugs like wild pointer writes and other bad updates into the database. Incompatible with nested transactions.
        ///     MDB_NOMETASYNC Flush system buffers to disk only once per transaction, omit the metadata flush. Defer that until the system flushes files to disk, or next non-MDB_RDONLY commit or mdb_env_sync(). This optimization maintains database integrity, but a system crash may undo the last committed transaction. I.e. it preserves the ACI (atomicity, consistency, isolation) but not D (durability) database property. This flag may be changed at any time using mdb_env_set_flags().
        ///     MDB_NOSYNC Don't flush system buffers to disk when committing a transaction. This optimization means a system crash can corrupt the database or lose the last transactions if buffers are not yet flushed to disk. The risk is governed by how often the system flushes dirty buffers to disk and how often mdb_env_sync() is called. However, if the filesystem preserves write order and the MDB_WRITEMAP flag is not used, transactions exhibit ACI (atomicity, consistency, isolation) properties and only lose D (durability). I.e. database integrity is maintained, but a system crash may undo the final transactions. Note that (MDB_NOSYNC | MDB_WRITEMAP) leaves the system with no hint for when to write transactions to disk, unless mdb_env_sync() is called. (MDB_MAPASYNC | MDB_WRITEMAP) may be preferable. This flag may be changed at any time using mdb_env_set_flags().
        ///     MDB_MAPASYNC When using MDB_WRITEMAP, use asynchronous flushes to disk. As with MDB_NOSYNC, a system crash can then corrupt the database or lose the last transactions. Calling mdb_env_sync() ensures on-disk database integrity until next commit. This flag may be changed at any time using mdb_env_set_flags().
        /// </param>
        /// <param name="mode">The UNIX permissions to set on created files. This parameter is ignored on Windows.</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_VERSION_MISMATCH - the version of the MDB library doesn't match the version that created the database environment.
        ///     MDB_INVALID - the environment file headers are corrupted.
        ///     ENOENT - the directory specified by the path parameter doesn't exist.
        ///     EACCES - the user didn't have permission to access the environment files.
        ///     EAGAIN - the environment was locked by another process.
        /// </returns>
        int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode);

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Set the size of the memory map to use for this environment.
        /// The size should be a multiple of the OS page size. 
        /// The default is 10485760 bytes. 
        /// The size of the memory map is also the maximum size of the database. 
        /// The value should be chosen as large as possible, to accommodate future growth of the database. 
        /// This function may only be called after mdb_env_create() and before mdb_env_open(). 
        /// The size may be changed by closing and reopening the environment. 
        /// Any attempt to set a size smaller than the space already consumed by the environment will be silently changed to the current size of the used space.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="size">The size in bytes</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified, or the environment is already open.
        /// </returns>
        int mdb_env_set_mapsize(IntPtr env, Int32 size); //OK

        /// <summary>
        /// Get the maximum number of threads for the environment.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="readers">Address of an integer to store the number of readers</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_env_get_maxreaders(IntPtr env, out UInt32 readers); //OK

        /// <summary>
        /// Set the maximum number of threads for the environment.
        /// This defines the number of slots in the lock table that is used to track readers in the the environment. 
        /// The default is 126. 
        /// This function may only be called after mdb_env_create() and before mdb_env_open().
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="readers">The maximum number of threads</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified, or the environment is already open.
        /// </returns>
        int mdb_env_set_maxreaders(IntPtr env, UInt32 readers); //OK

        /// <summary>
        /// Set the maximum number of named databases for the environment.
        /// This function is only needed if multiple databases will be used in the environment. 
        /// Simpler applications that use the environment as a single unnamed database can ignore this option. 
        /// This function may only be called after mdb_env_create() and before mdb_env_open().
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="dbs">The maximum number of databases</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified, or the environment is already open.
        /// </returns>
        int mdb_env_set_maxdbs(IntPtr env, UInt32 dbs); //OK

        /// <summary>
        /// Open a database in the environment.
        /// The database handle may be discarded by calling mdb_dbi_close(). 
        /// It denotes the name and parameters of a database, independently of whether such a database exists. 
        /// It will not exist if the transaction which created it aborted, nor if another process deleted it. 
        /// The database handle resides in the shared environment, it is not owned by the given transaction. 
        /// Only one thread should call this function; it is not mutex-protected in a read-only transaction. 
        /// Preexisting transactions, other than the current transaction and any parents, must not use the new handle. 
        /// Nor must their children. To use named databases (with name != NULL), mdb_env_set_maxdbs() must be called before opening the environment.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="name">The name of the database to open. If only a single database is needed in the environment, this value may be NULL.</param>
        /// <param name="flags">
        /// Special options for this database. This parameter must be set to 0 or by bitwise OR'ing together one or more of the values described here.
        ///     MDB_REVERSEKEY Keys are strings to be compared in reverse order, from the end of the strings to the beginning. By default, Keys are treated as strings and compared from beginning to end.
        ///     MDB_DUPSORT Duplicate keys may be used in the database. (Or, from another perspective, keys may have multiple data items, stored in sorted order.) By default keys must be unique and may have only a single data item.
        ///     MDB_INTEGERKEY Keys are binary integers in native byte order. Setting this option requires all keys to be the same size, typically sizeof(int) or sizeof(size_t).
        ///     MDB_DUPFIXED This flag may only be used in combination with MDB_DUPSORT. This option tells the library that the data items for this database are all the same size, which allows further optimizations in storage and retrieval. When all data items are the same size, the MDB_GET_MULTIPLE and MDB_NEXT_MULTIPLE cursor operations may be used to retrieve multiple items at once.
        ///     MDB_INTEGERDUP This option specifies that duplicate data items are also integers, and should be sorted as such.
        ///     MDB_REVERSEDUP This option specifies that duplicate data items should be compared as strings in reverse order.
        ///     MDB_CREATE Create the named database if it doesn't exist. This option is not allowed in a read-only transaction or a read-only environment.
        /// </param>
        /// <param name="db">Address where the new MDB_dbi handle will be stored</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        /// MDB_NOTFOUND (-30798) - the specified database doesn't exist in the environment and MDB_CREATE was not specified.
        /// MDB_DBS_FULL (-30791) - too many databases have been opened. See mdb_env_set_maxdbs().
        /// </returns>
        int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out UInt32 db); //OK

        /// <summary>
        /// Close a database handle.
        /// This call is not mutex protected. 
        /// Handles should only be closed by a single thread, and only if no other threads are going to reference the database handle or one of its cursors any further. 
        /// Do not close a handle if an existing transaction has modified its database.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        void mdb_dbi_close(IntPtr env, UInt32 dbi); //OK

        /// <summary>
        /// Delete a database and/or free all its pages.
        /// If the del parameter is 1, the DB handle will be closed and the DB will be deleted.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="del">1 to delete the DB from the environment, 0 to just free its pages.</param>
        /// <returns>A non-zero error value on failure and 0 on success.</returns>
        int mdb_drop(IntPtr txn, UInt32 dbi, bool del); //OK

        /// <summary>
        /// Create a transaction for use with the environment.
        /// The transaction handle may be discarded using mdb_txn_abort() or mdb_txn_commit().
        /// Note:
        /// Transactions may not span threads; a transaction must only be used by a single thread. Also, a thread may only have a single transaction.
        /// Cursors may not span transactions; each cursor must be opened and closed within a single transaction.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="parent">
        /// If this parameter is non-NULL, the new transaction will be a nested transaction, with the transaction indicated by parent as its parent. 
        /// Transactions may be nested to any level. 
        /// A parent transaction may not issue any other operations besides mdb_txn_begin, mdb_txn_abort, or mdb_txn_commit while it has active child transactions.
        /// </param>
        /// <param name="flags">
        /// Special options for this transaction. 
        /// This parameter must be set to 0 or by bitwise OR'ing together one or more of the values described here.
        ///     MDB_RDONLY This transaction will not perform any write operations.
        /// </param>
        /// <param name="txn">Address where the new MDB_txn handle will be stored</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_PANIC (-30795) - a fatal error occurred earlier and the environment
        ///         * must be shut down.
        ///     MDB_MAP_RESIZED (-30785) - another process wrote data beyond this MDB_env's mapsize and the environment must be shut down.
        ///     MDB_READERS_FULL (-30790) - a read-only transaction was requested and the reader lock table is full. See mdb_env_set_maxreaders().
        ///     ENOMEM - out of memory.
        /// </returns>
        int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn); //OK

        /// <summary>
        /// Commit all the operations of a transaction into the database.
        /// All cursors opened within the transaction will be closed by this call. 
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified.
        ///     ENOSPC - no more disk space.
        ///     EIO - a low-level I/O error occurred while writing.
        ///     ENOMEM - out of memory.
        /// </returns>
        int mdb_txn_commit(IntPtr txn); //OK

        /// <summary>
        /// Abandon all the operations of the transaction instead of saving them.
        /// All cursors opened within the transaction will be closed by this call.
        /// The cursors and transaction handle will be freed and must not be used again after this call.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        void mdb_txn_abort(IntPtr txn); //OK

        /// <summary>
        /// Reset a read-only transaction.
        /// This releases the current reader lock but doesn't free the transaction handle, allowing it to be used again later by mdb_txn_renew(). 
        /// It otherwise has the same effect as mdb_txn_abort() but saves some memory allocation/deallocation overhead if a thread is going to start a new read-only transaction again soon. 
        /// All cursors opened within the transaction must be closed before the transaction is reset. 
        /// Reader locks generally don't interfere with writers, but they keep old versions of database pages allocated. 
        /// Thus they prevent the old pages from being reused when writers commit new data, and so under heavy load the database size may grow much more rapidly than otherwise.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        void mdb_txn_reset(IntPtr txn); //OK

        /// <summary>
        /// Renew a read-only transaction.
        /// This acquires a new reader lock for a transaction handle that had been released by mdb_txn_reset(). 
        /// It must be called before a reset transaction may be used again.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_PANIC - a fatal error occurred earlier and the environment must be shut down.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_txn_renew(IntPtr txn); //OK

        /// <summary>
        /// Return the mdb library version information.
        /// </summary>
        /// <param name="major">if non-NULL, the library major version number is copied here</param>
        /// <param name="minor">if non-NULL, the library minor version number is copied here</param>
        /// <param name="patch">if non-NULL, the library patch version number is copied here</param>
        /// <returns>The library version as a string</returns>
        string mdb_version(out int major, out int minor, out int patch); //OK

        /// <summary>
        /// Return a string describing a given error code.
        /// This function is a superset of the ANSI C X3.159-1989 (ANSI C) strerror(3) function. 
        /// If the error code is greater than or equal to 0, then the string returned by the system function strerror(3) is returned. 
        /// If the error code is less than 0, an error string corresponding to the MDB library error is returned. 
        /// See Return Codes for a list of MDB-specific error codes.
        /// </summary>
        /// <param name="err">The error code</param>
        /// <returns>The description of the error</returns>
        IntPtr mdb_strerror(int err); //OK

        /// <summary>
        /// Copy an MDB environment to the specified path.
        /// This function may be used to make a backup of an existing environment.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create(). It must have already been opened successfully.</param>
        /// <param name="path">The directory in which the copy will reside. This directory must already exist and be writable but must otherwise be empty.</param>
        /// <returns>A non-zero error value on failure and 0 on success.</returns>
        int mdb_env_copy(IntPtr env, string path); //OK

        /// <summary>
        /// Flush the data buffers to disk. 
        /// Data is always written to disk when mdb_txn_commit() is called, but the operating system may keep it buffered. 
        /// MDB always flushes the OS buffers upon commit as well, unless the environment was opened with MDB_NOSYNC or in part MDB_NOMETASYNC.
        /// </summary>
        /// <param name="env">An environment handle returned by mdb_env_create()</param>
        /// <param name="force">If non-zero, force a synchronous flush. Otherwise if the environment has the MDB_NOSYNC flag set the flushes will be omitted, and with MDB_MAPASYNC they will be asynchronous.</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified.
        ///     EIO - an error occurred during synchronization.
        /// </returns>
        int mdb_env_sync(IntPtr env, bool force); //OK

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Get items from a database.
        /// This function retrieves key/data pairs from the database. 
        /// The address and length of the data associated with the specified key are returned in the structure to which data refers. 
        /// If the database supports duplicate keys (MDB_DUPSORT) then the first data item for the key will be returned. 
        /// Retrieval of other items requires the use of mdb_cursor_get().
        /// Note:
        ///     The memory pointed to by the returned values is owned by the database. 
        ///     The caller need not dispose of the memory, and may not modify it in any way. 
        ///     For values returned in a read-only transaction any modification attempts will cause a SIGSEGV.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="key">The key to search for in the database</param>
        /// <param name="data">The data corresponding to the key</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_NOTFOUND - the key was not in the database.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_get(IntPtr txn, UInt32 dbi, ref ValueStructure key, out ValueStructure data); //OK

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Store items into a database.
        /// This function stores key/data pairs in the database. 
        /// The default behavior is to enter the new key/data pair, replacing any previously existing key if duplicates are disallowed, or adding a duplicate data item if duplicates are allowed (MDB_DUPSORT).
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="key">The key to store in the database</param>
        /// <param name="data">The data to store</param>
        /// <param name="flags">
        /// Special options for this operation. 
        /// This parameter must be set to 0 or by bitwise OR'ing together one or more of the values described here.
        ///     MDB_NODUPDATA - enter the new key/data pair only if it does not already appear in the database. This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST if the key/data pair already appears in the database.
        ///     MDB_NOOVERWRITE - enter the new key/data pair only if the key does not already appear in the database. The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports duplicates (MDB_DUPSORT). The data parameter will be set to point to the existing item.
        ///     MDB_RESERVE - reserve space for data of the given size, but don't copy the given data. Instead, return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data is being generated later.
        ///     MDB_APPEND - append the given key/data pair to the end of the database. No key comparisons are performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading unsorted keys with this flag will cause data corruption.
        ///     MDB_APPENDDUP - as above, but for sorted dup data.
        /// </param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_MAP_FULL - the database is full, see mdb_env_set_mapsize().
        ///     MDB_TXN_FULL - the transaction has too many dirty pages.
        ///     EACCES - an attempt was made to write in a read-only transaction.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_put(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags); //OK

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Delete items from a database.
        /// This function removes key/data pairs from the database. 
        /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored. 
        /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted. 
        /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted. 
        /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="key">The key to delete from the database</param>
        /// <param name="data">The data to delete</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EACCES - an attempt was made to write in a read-only transaction.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data);

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Delete items from a database.
        /// This function removes key/data pairs from the database. 
        /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored. 
        /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted. 
        /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted. 
        /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="key">The key to delete from the database</param>
        /// <param name="data">The data to delete</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EACCES - an attempt was made to write in a read-only transaction.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, IntPtr data);

        /// <summary>
        /// Create a cursor handle.
        /// Cursors are associated with a specific transaction and database and may not span threads.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open()</param>
        /// <param name="cursor">Address where the new MDB_cursor handle will be stored</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_cursor_open(IntPtr txn, UInt32 dbi, out IntPtr cursor); //OK

        /// <summary>
        /// Close a cursor handle.
        /// The cursor handle will be freed and must not be used again after this call.
        /// </summary>
        /// <param name="cursor">A cursor handle returned by mdb_cursor_open()</param>
        void mdb_cursor_close(IntPtr cursor); //OK

        /// <summary>
        /// Renew a cursor handle.
        /// Cursors are associated with a specific transaction and database and may not span threads. 
        /// Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead. 
        /// The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was created with.
        /// </summary>
        /// <param name="txn">A transaction handle returned by mdb_txn_begin()</param>
        /// <param name="cursor">A cursor handle returned by mdb_cursor_open()</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_cursor_renew(IntPtr txn, IntPtr cursor); //OK

        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Retrieve by cursor.
        /// This function retrieves key/data pairs from the database. 
        /// The address and length of the key are returned in the object to which key refers (except for the case of the MDB_SET option, in which the key object is unchanged), and the address and length of the data are returned in the object to which data refers.
        /// </summary>
        /// <param name="cursor">A cursor handle returned by mdb_cursor_open()</param>
        /// <param name="key">The key for a retrieved item</param>
        /// <param name="data">The data of a retrieved item</param>
        /// <param name="op">A cursor operation MDB_cursor_op</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_NOTFOUND - no matching key found.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);


        /// <summary>
        /// Use with native library built for 32-bit systems.
        /// Store by cursor.
        /// This function stores key/data pairs into the database. 
        /// If the function fails for any reason, the state of the cursor will be unchanged. 
        /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
        /// </summary>
        /// <param name="cursor">A cursor handle returned by mdb_cursor_open()</param>
        /// <param name="key">The key operated on.</param>
        /// <param name="data">The data operated on.</param>
        /// <param name="flags">
        /// Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     MDB_CURRENT - overwrite the data of the key/data pair to which the cursor refers with the specified data item. The key parameter is ignored.
        ///     MDB_NODUPDATA - enter the new key/data pair only if it does not already appear in the database. This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST if the key/data pair already appears in the database.
        ///     MDB_NOOVERWRITE - enter the new key/data pair only if the key does not already appear in the database. The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports duplicates (MDB_DUPSORT).
        ///     MDB_RESERVE - reserve space for data of the given size, but don't copy the given data. Instead, return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data is being generated later.
        ///     MDB_APPEND - append the given key/data pair to the end of the database. No key comparisons are performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading unsorted keys with this flag will cause data corruption.
        ///     MDB_APPENDDUP - as above, but for sorted dup data.
        /// </param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     MDB_MAP_FULL - the database is full, see mdb_env_set_mapsize().
        ///     MDB_TXN_FULL - the transaction has too many dirty pages.
        ///     EACCES - an attempt was made to modify a read-only database.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_cursor_put(IntPtr cursor, ValueStructure key, ValueStructure data, PutOptions flags); //OK

        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="cursor">A cursor handle returned by mdb_cursor_open()</param>
        /// <param name="flags">Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     MDB_NODUPDATA - delete all of the data items for the current key. This flag may only be specified if the database was opened with MDB_DUPSORT.</param>
        /// <returns>
        /// A non-zero error value on failure and 0 on success. Some possible errors are:
        ///     EACCES - an attempt was made to modify a read-only database.
        ///     EINVAL - an invalid parameter was specified.
        /// </returns>
        int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags); //OK
    }
}
