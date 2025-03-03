using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

public static partial class Lmdb
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates an LMDB environment handle.
    /// </summary>
    /// <param name="env">The address where the new handle will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_create(out nint env);

    /// <summary>
    /// Closes the environment and releases the memory map.
    /// </summary>
    /// <param name="env">The environment handle to close</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void mdb_env_close(nint env);

    /// <summary>
    /// Opens an environment handle with the specified path, flags, and permissions.
    /// </summary>
    /// <param name="env">The environment handle to open</param>
    /// <param name="path">The directory in which the database files reside</param>
    /// <param name="flags">Special options for this environment</param>
    /// <param name="mode">The UNIX permissions to set on created files</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_open(nint env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);

    /// <summary>
    /// Sets the size of the memory map to use for this environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="size">The size in bytes</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_mapsize(nint env, nint size);

    /// <summary>
    /// Gets the maximum number of threads/reader slots for the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="readers">Address where the number of readers will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_get_maxreaders(nint env, out uint readers);

    /// <summary>
    /// Sets the maximum number of threads/reader slots for the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="readers">The number of reader slots to allocate</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_maxreaders(nint env, uint readers);

    /// <summary>
    /// Sets the maximum number of named databases for the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="dbs">The maximum number of databases</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_maxdbs(nint env, uint dbs);

    /// <summary>
    /// Sets environment flags.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="flags">The flags to set</param>
    /// <param name="onoff">A boolean to turn the flags on or off</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_flags(nint env, uint flags, [MarshalAs(UnmanagedType.I1)] bool onoff);

    /// <summary>
    /// Gets environment flags.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="flags">Address where the flags will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_get_flags(nint env, out uint flags);

    /// <summary>
    /// Returns the path that was used in mdb_env_open.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="path">Address where the path will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_get_path(nint env, out nint path);

    /// <summary>
    /// Returns the file descriptor for the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="fd">Address where the descriptor will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_get_fd(nint env, out nint fd);

    /// <summary>
    /// Returns the maximum size of keys and MDB_DUPSORT data we can write.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <returns>The maximum size of a key we can write</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_env_get_maxkeysize(nint env);

    /// <summary>
    /// Set application information associated with the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="ctx">An arbitrary pointer for your application's use</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_userctx(nint env, nint ctx);

    /// <summary>
    /// Get the application information associated with the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <returns>The pointer set by mdb_env_set_userctx</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint mdb_env_get_userctx(nint env);

    /// <summary>
    /// Set or reset the assert callback for the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="func">A callback function to run when an assertion fails</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_set_assert(nint env, nint func);

    /// <summary>
    /// Opens a database in the environment.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="name">The name of the database to open (NULL for the main DB)</param>
    /// <param name="flags">Special options for this database</param>
    /// <param name="db">Address where the database handle will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_dbi_open(nint txn, string name, DatabaseOpenFlags flags, out uint db);

    /// <summary>
    /// Closes a database handle in the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="dbi">A database handle returned by mdb_dbi_open</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void mdb_dbi_close(nint env, uint dbi);

    /// <summary>
    /// Retrieves the flags for a database handle.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="flags">Address where the flags will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_dbi_flags(nint txn, uint dbi, out uint flags);

    /// <summary>
    /// Empties or deletes a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="del">If true, delete the DB from the environment; otherwise just empty it</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_drop(nint txn, uint dbi, [MarshalAs(UnmanagedType.I1)] bool del);

    /// <summary>
    /// Creates a transaction for use with the environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="parent">Parent transaction (for nested transactions; NULL for none)</param>
    /// <param name="flags">Special transaction flags</param>
    /// <param name="txn">Address where the new transaction handle will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_txn_begin(nint env, nint parent, TransactionBeginFlags flags, out nint txn);

    /// <summary>
    /// Returns the transaction's environment.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <returns>The environment handle</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint mdb_txn_env(nint txn);

    /// <summary>
    /// Returns the transaction's ID.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <returns>The transaction ID</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_txn_id(nint txn);

    /// <summary>
    /// Commits all the operations of a transaction into the database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_txn_commit(nint txn);

    /// <summary>
    /// Abandons all the operations of the transaction instead of saving them.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void mdb_txn_abort(nint txn);

    /// <summary>
    /// Resets a read-only transaction.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void mdb_txn_reset(nint txn);

    /// <summary>
    /// Renews a read-only transaction that was previously reset.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_txn_renew(nint txn);

    /// <summary>
    /// Returns the LMDB library version and version information.
    /// </summary>
    /// <param name="major">The library major version number</param>
    /// <param name="minor">The library minor version number</param>
    /// <param name="patch">The library patch version number</param>
    /// <returns>Pointer to a version string</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint mdb_version(out int major, out int minor, out int patch);

    /// <summary>
    /// Returns a string describing a given error code.
    /// </summary>
    /// <param name="err">The error code</param>
    /// <returns>A pointer to the error string</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint mdb_strerror(int err);

    /// <summary>
    /// Returns statistics about a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="stat">Address where the database statistics will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_stat(nint txn, uint dbi, out MDBStat stat);

    /// <summary>
    /// Copies an LMDB environment to the specified path.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="path">The directory in which the backup will reside</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_copy(nint env, string path);

    /// <summary>
    /// Copies an LMDB environment to the specified file descriptor.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="fd">The file descriptor to write to</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_copyfd(nint env, nint fd);

    /// <summary>
    /// Copies an LMDB environment to the specified path, with options for compaction.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="path">The directory in which the backup will reside</param>
    /// <param name="copyFlags">Special options for this copy operation</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_copy2(nint env, string path, EnvironmentCopyFlags copyFlags);

    /// <summary>
    /// Copies an LMDB environment to the specified file descriptor, with options for compaction.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="fd">The file descriptor to write to</param>
    /// <param name="copyFlags">Special options for this copy operation</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_copyfd2(nint env, nint fd, EnvironmentCopyFlags copyFlags);

    /// <summary>
    /// Returns information about the LMDB environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="stat">Address where the environment info will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_info(nint env, out MDBEnvInfo stat);

    /// <summary>
    /// Returns statistics about the LMDB environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="stat">Address where the environment statistics will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_stat(nint env, out MDBStat stat);

    /// <summary>
    /// Flushes all data to the environment's data file.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="force">If true, force a synchronous flush; otherwise if the environment has MDB_NOSYNC or MDB_MAPASYNC, it will not be synchronous</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_env_sync(nint env, [MarshalAs(UnmanagedType.I1)] bool force);

    /// <summary>
    /// Retrieves items from a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to search for</param>
    /// <param name="data">Address where the retrieved data will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_get(nint txn, uint dbi, ref MDBValue key, out MDBValue data);

    /// <summary>
    /// Returns count of duplicates for the current key in a cursor.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <param name="countp">Address where the count will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_count(nint cursor, out int countp);

    /// <summary>
    /// Stores key/data pairs in a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to store</param>
    /// <param name="data">The data to store</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_put(nint txn, uint dbi, ref MDBValue key, ref MDBValue data, PutOptions flags);

    /// <summary>
    /// Deletes items from a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to delete</param>
    /// <param name="data">The data to delete (only needed for MDB_DUPSORT)</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, ref MDBValue data);

    /// <summary>
    /// Deletes items from a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to delete</param>
    /// <param name="data">NULL pointer to delete all of the data items for the key</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, nint data);

    /// <summary>
    /// Creates a cursor handle for the specified transaction and database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="cursor">Address where the new cursor handle will be stored</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_open(nint txn, uint dbi, out nint cursor);

    /// <summary>
    /// Closes a cursor handle.
    /// </summary>
    /// <param name="cursor">A cursor handle to close</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void mdb_cursor_close(nint cursor);

    /// <summary>
    /// Renews a cursor handle.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="cursor">A cursor handle to renew</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_renew(nint txn, nint cursor);

    /// <summary>
    /// Returns the cursor's transaction handle.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <returns>The transaction handle</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint mdb_cursor_txn(nint cursor);

    /// <summary>
    /// Returns the cursor's database handle.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <returns>The database handle</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint mdb_cursor_dbi(nint cursor);

    /// <summary>
    /// Retrieves key/data pairs from the database using a cursor.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <param name="key">The key for the current item</param>
    /// <param name="data">The data for the current item</param>
    /// <param name="op">The cursor operation to perform</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_get(nint cursor, ref MDBValue key, ref MDBValue data, CursorOperation op);

    /// <summary>
    /// Stores key/data pairs into the database using a cursor.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <param name="key">The key to store</param>
    /// <param name="mdbValue">The data to store</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref MDBValue mdbValue, CursorPutOptions flags);

    /// <summary>
    /// Deletes the current key/data pair to which the cursor refers.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_cursor_del(nint cursor, CursorDeleteOption flags);

    /// <summary>
    /// Sets a custom key comparison function for a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="cmp">The comparison function to set</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_set_compare(nint txn, uint dbi, CompareFunction cmp);

    /// <summary>
    /// Sets a custom data comparison function for a database with MDB_DUPSORT.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="cmp">The comparison function to set</param>
    /// <returns>A result code indicating success or failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial MDBResultCode mdb_set_dupsort(nint txn, uint dbi, CompareFunction cmp);

    /// <summary>
    /// Compares two data items according to a database's key comparison function.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="a">The first item to compare</param>
    /// <param name="b">The second item to compare</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_cmp(nint txn, uint dbi, ref MDBValue a, ref MDBValue b);

    /// <summary>
    /// Compares two data items according to a database's data comparison function.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="a">The first item to compare</param>
    /// <param name="b">The second item to compare</param>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_dcmp(nint txn, uint dbi, ref MDBValue a, ref MDBValue b);

    /// <summary>
    /// Lists all the readers in the environment and the transaction they're holding.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="func">A function to call for each reader</param>
    /// <param name="ctx">Arbitrary context data to pass to the function</param>
    /// <returns>Number of readers that were found</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_reader_list(nint env, nint func, nint ctx);

    /// <summary>
    /// Checks for stale readers in the environment reader table.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="dead">Address where the number of stale readers cleaned up will be stored</param>
    /// <returns>0 on success, non-zero on failure</returns>
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int mdb_reader_check(nint env, out int dead);
#endif
}

public static partial class Lmdb
{
    private const string MDB_DLL_NAME = "lmdb";

    /// <summary>
    /// Sets the size of the memory map to use for this environment.
    /// </summary>
    /// <param name="env">The environment handle</param>
    /// <param name="size">The size in bytes</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_env_set_mapsize(nint env, long size)
    {
        return mdb_env_set_mapsize(env, (nint)size);
    }

    /// <summary>
    /// Stores key/data pairs in a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to store</param>
    /// <param name="value">The data to store</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_put(nint txn, uint dbi, MDBValue key, MDBValue value, PutOptions flags)
    {
        return mdb_put(txn, dbi, ref key, ref value, flags);
    }

    /// <summary>
    /// Deletes items from a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to delete</param>
    /// <param name="value">The data to delete</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_del(nint txn, uint dbi, MDBValue key, MDBValue value)
    {
        return mdb_del(txn, dbi, ref key, ref value);
    }

    /// <summary>
    /// Deletes items from a database.
    /// </summary>
    /// <param name="txn">A transaction handle</param>
    /// <param name="dbi">A database handle</param>
    /// <param name="key">The key to delete</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_del(nint txn, uint dbi, MDBValue key)
    {
        return mdb_del(txn, dbi, ref key, 0);
    }

    /// <summary>
    /// Stores key/data pairs into the database using a cursor.
    /// </summary>
    /// <param name="cursor">A cursor handle</param>
    /// <param name="key">The key to store</param>
    /// <param name="value">The data to store</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_cursor_put(nint cursor, MDBValue key, MDBValue value, CursorPutOptions flags)
    {
        return mdb_cursor_put(cursor, ref key, ref value, flags);
    }

    /// <summary>
    /// Stores multiple contiguous data elements in a single request with a cursor.
    /// May only be used with MDB_DUPFIXED.
    /// </summary>
    /// <param name="cursor">Pointer to cursor</param>
    /// <param name="key">The key to store</param>
    /// <param name="data">This span must be pinned or stackalloc memory</param>
    /// <param name="flags">Special options for this operation</param>
    /// <returns>A result code indicating success or failure</returns>
    public static MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref Span<MDBValue> data,
        CursorPutOptions flags)
    {
        ref var dataRef = ref MemoryMarshal.GetReference(data);
        return mdb_cursor_put(cursor, ref key, ref dataRef, flags);
    }

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Creates an LMDB environment handle.
        /// </summary>
        /// <param name="env">The address where the new handle will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_create(out nint env);

        /// <summary>
        /// Closes the environment and releases the memory map.
        /// </summary>
        /// <param name="env">The environment handle to close</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_env_close(nint env);

        /// <summary>
        /// Opens an environment handle with the specified path, flags, and permissions.
        /// </summary>
        /// <param name="env">The environment handle to open</param>
        /// <param name="path">The directory in which the database files reside</param>
        /// <param name="flags">Special options for this environment</param>
        /// <param name="mode">The UNIX permissions to set on created files</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_open(nint env, byte[] path, EnvironmentOpenFlags flags,
            UnixAccessMode mode);

        /// <summary>
        /// Opens an environment handle with the specified path, flags, and permissions.
        /// </summary>
        /// <param name="env">The environment handle to open</param>
        /// <param name="path">The directory in which the database files reside</param>
        /// <param name="flags">Special options for this environment</param>
        /// <param name="mode">The UNIX permissions to set on created files</param>
        /// <returns>A result code indicating success or failure</returns>
        internal static MDBResultCode mdb_env_open(nint env, string path, EnvironmentOpenFlags flags,
            UnixAccessMode mode)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(path);
            return mdb_env_open(env, bytes, flags, mode);
        }

        /// <summary>
        /// Sets the size of the memory map to use for this environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="size">The size in bytes</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_mapsize(nint env, nint size);

        /// <summary>
        /// Gets the maximum number of threads/reader slots for the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="readers">Address where the number of readers will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_get_maxreaders(nint env, out uint readers);

        /// <summary>
        /// Sets the maximum number of threads/reader slots for the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="readers">The number of reader slots to allocate</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_maxreaders(nint env, uint readers);

        /// <summary>
        /// Sets the maximum number of named databases for the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="dbs">The maximum number of databases</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_maxdbs(nint env, uint dbs);

        /// <summary>
        /// Sets environment flags.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="flags">The flags to set</param>
        /// <param name="onoff">A boolean to turn the flags on or off</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_flags(nint env, uint flags, bool onoff);

        /// <summary>
        /// Gets environment flags.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="flags">Address where the flags will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_get_flags(nint env, out uint flags);

        /// <summary>
        /// Returns the path that was used in mdb_env_open.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="path">Address where the path will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_get_path(nint env, out nint path);

        /// <summary>
        /// Returns the file descriptor for the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="fd">Address where the descriptor will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_get_fd(nint env, out nint fd);

        /// <summary>
        /// Returns the maximum size of keys and MDB_DUPSORT data we can write.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <returns>The maximum size of a key we can write</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_get_maxkeysize(nint env);

        /// <summary>
        /// Set application information associated with the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="ctx">An arbitrary pointer for your application's use</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_userctx(nint env, nint ctx);

        /// <summary>
        /// Get the application information associated with the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <returns>The pointer set by mdb_env_set_userctx</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_env_get_userctx(nint env);

        /// <summary>
        /// Set or reset the assert callback for the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="func">A callback function to run when an assertion fails</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_assert(nint env, nint func);

        /// <summary>
        /// Opens a database in the environment.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="name">The name of the database to open (NULL for the main DB)</param>
        /// <param name="flags">Special options for this database</param>
        /// <param name="db">Address where the database handle will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_dbi_open(nint txn, string name, DatabaseOpenFlags flags, out uint db);

        /// <summary>
        /// Closes a database handle in the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="dbi">A database handle returned by mdb_dbi_open</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_dbi_close(nint env, uint dbi);

        /// <summary>
        /// Retrieves the flags for a database handle.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="flags">Address where the flags will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_dbi_flags(nint txn, uint dbi, out uint flags);

        /// <summary>
        /// Empties or deletes a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="del">If true, delete the DB from the environment; otherwise just empty it</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_drop(nint txn, uint dbi, bool del);

        /// <summary>
        /// Creates a transaction for use with the environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="parent">Parent transaction (for nested transactions; NULL for none)</param>
        /// <param name="flags">Special transaction flags</param>
        /// <param name="txn">Address where the new transaction handle will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_begin(nint env, nint parent, TransactionBeginFlags flags, out nint txn);

        /// <summary>
        /// Returns the transaction's environment.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <returns>The environment handle</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_txn_env(nint txn);

        /// <summary>
        /// Returns the transaction's ID.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <returns>The transaction ID</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_txn_id(nint txn);

        /// <summary>
        /// Commits all the operations of a transaction into the database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_commit(nint txn);

        /// <summary>
        /// Abandons all the operations of the transaction instead of saving them.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_abort(nint txn);

        /// <summary>
        /// Resets a read-only transaction.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_reset(nint txn);

        /// <summary>
        /// Renews a read-only transaction that was previously reset.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_renew(nint txn);

        /// <summary>
        /// Returns the LMDB library version and version information.
        /// </summary>
        /// <param name="major">The library major version number</param>
        /// <param name="minor">The library minor version number</param>
        /// <param name="patch">The library patch version number</param>
        /// <returns>Pointer to a version string</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_version(out int major, out int minor, out int patch);

        /// <summary>
        /// Returns a string describing a given error code.
        /// </summary>
        /// <param name="err">The error code</param>
        /// <returns>A pointer to the error string</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_strerror(int err);

        /// <summary>
        /// Returns statistics about a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="stat">Address where the database statistics will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_stat(nint txn, uint dbi, out MDBStat stat);

        /// <summary>
        /// Copies an LMDB environment to the specified path.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="path">The directory in which the backup will reside</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_copy(nint env, byte[] path);

        /// <summary>
        /// Copies an LMDB environment to the specified path.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="path">The directory in which the backup will reside</param>
        /// <returns>A result code indicating success or failure</returns>
        public static MDBResultCode mdb_env_copy(nint env, string path)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(path);
            return mdb_env_copy(env, bytes);
        }

        /// <summary>
        /// Copies an LMDB environment to the specified file descriptor.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="fd">The file descriptor to write to</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_copyfd(nint env, nint fd);

        /// <summary>
        /// Copies an LMDB environment to the specified file descriptor, with options for compaction.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="fd">The file descriptor to write to</param>
        /// <param name="copyFlags">Special options for this copy operation</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_copyfd2(nint env, nint fd, EnvironmentCopyFlags copyFlags);

        /// <summary>
        /// Copies an LMDB environment to the specified path, with options for compaction.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="path">The directory in which the backup will reside</param>
        /// <param name="copyFlags">Special options for this copy operation</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_copy2(nint env, byte[] path, EnvironmentCopyFlags copyFlags);

        /// <summary>
        /// Copies an LMDB environment to the specified path, with options for compaction.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="path">The directory in which the backup will reside</param>
        /// <param name="copyFlags">Special options for this copy operation</param>
        /// <returns>A result code indicating success or failure</returns>
        public static MDBResultCode mdb_env_copy2(nint env, string path, EnvironmentCopyFlags copyFlags)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(path);
            return mdb_env_copy2(env, bytes, copyFlags);
        }

        /// <summary>
        /// Returns information about the LMDB environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="stat">Address where the environment info will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_info(nint env, out MDBEnvInfo stat);

        /// <summary>
        /// Returns statistics about the LMDB environment.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="stat">Address where the environment statistics will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_stat(nint env, out MDBStat stat);

        /// <summary>
        /// Flushes all data to the environment's data file.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="force">If true, force a synchronous flush; otherwise if the environment has MDB_NOSYNC or MDB_MAPASYNC, it will not be synchronous</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_sync(nint env, bool force);

        /// <summary>
        /// Retrieves items from a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="key">The key to search for</param>
        /// <param name="data">Address where the retrieved data will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_get(nint txn, uint dbi, ref MDBValue key, out MDBValue data);

        /// <summary>
        /// Returns count of duplicates for the current key in a cursor.
        /// </summary>
        /// <param name="cursor">A cursor handle</param>
        /// <param name="countp">Address where the count will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_count(nint cursor, out int countp);

        /// <summary>
        /// Stores key/data pairs in a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="key">The key to store</param>
        /// <param name="data">The data to store</param>
        /// <param name="flags">Special options for this operation</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_put(nint txn, uint dbi, ref MDBValue key, ref MDBValue data, PutOptions flags);

        /// <summary>
        /// Deletes items from a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="key">The key to delete</param>
        /// <param name="data">The data to delete (only needed for MDB_DUPSORT)</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, ref MDBValue data);

        /// <summary>
        /// Deletes items from a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="key">The key to delete</param>
        /// <param name="data">NULL pointer to delete all of the data items for the key</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, nint data);

        /// <summary>
        /// Creates a cursor handle for the specified transaction and database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="cursor">Address where the new cursor handle will be stored</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_open(nint txn, uint dbi, out nint cursor);

        /// <summary>
        /// Closes a cursor handle.
        /// </summary>
        /// <param name="cursor">A cursor handle to close</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_cursor_close(nint cursor);

        /// <summary>
        /// Renews a cursor handle.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="cursor">A cursor handle to renew</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_renew(nint txn, nint cursor);

        /// <summary>
        /// Retrieves key/data pairs from the database using a cursor.
        /// </summary>
        /// <param name="cursor">A cursor handle</param>
        /// <param name="key">The key for the current item</param>
        /// <param name="data">The data for the current item</param>
        /// <param name="op">The cursor operation to perform</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_get(nint cursor, ref MDBValue key, ref MDBValue data, CursorOperation op);

        /// <summary>
        /// Stores key/data pairs into the database using a cursor.
        /// </summary>
        /// <param name="cursor">A cursor handle</param>
        /// <param name="key">The key to store</param>
        /// <param name="mdbValue">The data to store</param>
        /// <param name="flags">Special options for this operation</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref MDBValue mdbValue, CursorPutOptions flags);

        /// <summary>
        /// Deletes the current key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="cursor">A cursor handle</param>
        /// <param name="flags">Special options for this operation</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_del(nint cursor, CursorDeleteOption flags);

        /// <summary>
        /// Sets a custom key comparison function for a database.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="cmp">The comparison function to set</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_set_compare(nint txn, uint dbi, CompareFunction cmp);

        /// <summary>
        /// Sets a custom data comparison function for a database with MDB_DUPSORT.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="cmp">The comparison function to set</param>
        /// <returns>A result code indicating success or failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_set_dupsort(nint txn, uint dbi, CompareFunction cmp);

        /// <summary>
        /// Compares two data items according to a database's key comparison function.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="a">The first item to compare</param>
        /// <param name="b">The second item to compare</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cmp(nint txn, uint dbi, ref MDBValue a, ref MDBValue b);

        /// <summary>
        /// Compares two data items according to a database's data comparison function.
        /// </summary>
        /// <param name="txn">A transaction handle</param>
        /// <param name="dbi">A database handle</param>
        /// <param name="a">The first item to compare</param>
        /// <param name="b">The second item to compare</param>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_dcmp(nint txn, uint dbi, ref MDBValue a, ref MDBValue b);

        /// <summary>
        /// Lists all the readers in the environment and the transaction they're holding.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="func">A function to call for each reader</param>
        /// <param name="ctx">Arbitrary context data to pass to the function</param>
        /// <returns>Number of readers that were found</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_reader_list(nint env, nint func, nint ctx);

        /// <summary>
        /// Checks for stale readers in the environment reader table.
        /// </summary>
        /// <param name="env">The environment handle</param>
        /// <param name="dead">Address where the number of stale readers cleaned up will be stored</param>
        /// <returns>0 on success, non-zero on failure</returns>
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_reader_check(nint env, out int dead);
#endif
}
