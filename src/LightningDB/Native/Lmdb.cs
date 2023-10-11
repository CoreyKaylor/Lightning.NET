using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
public static partial class Lmdb
{
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_create(out nint env); 
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial void mdb_env_close(nint env);
        
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_open(nint env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_set_mapsize(nint env, nint size);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_get_maxreaders(nint env, out uint readers);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_set_maxreaders(nint env, uint readers);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_set_maxdbs(nint env, uint dbs);
        
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_dbi_open(nint txn, string name, DatabaseOpenFlags flags, out uint db);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial void mdb_dbi_close(nint env, uint dbi);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_drop(nint txn, uint dbi, [MarshalAs(UnmanagedType.I1)] bool del);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_txn_begin(nint env, nint parent, TransactionBeginFlags flags, out nint txn);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_txn_commit(nint txn);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial void mdb_txn_abort(nint txn);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial void mdb_txn_reset(nint txn);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_txn_renew(nint txn);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial nint mdb_version(out int major, out int minor, out int patch);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial nint mdb_strerror(int err);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_stat(nint txn, uint dbi, out MDBStat stat);
        
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_copy(nint env, string path);
        
    [LibraryImport(MDB_DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_copy2(nint env, string path, EnvironmentCopyFlags copyFlags);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_info(nint env, out MDBEnvInfo stat);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_stat(nint env, out MDBStat stat);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_env_sync(nint env, [MarshalAs(UnmanagedType.I1)] bool force);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_get(nint txn, uint dbi, ref MDBValue key, out MDBValue data);
        
    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_count(nint cursor, out int countp);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_put(nint txn, uint dbi, ref MDBValue key, ref MDBValue data, PutOptions flags);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, ref MDBValue data);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, nint data);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_open(nint txn, uint dbi, out nint cursor);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial void mdb_cursor_close(nint cursor);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_renew(nint txn, nint cursor);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_get(nint cursor, ref MDBValue key, ref MDBValue data, CursorOperation op);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref MDBValue mdbValue, CursorPutOptions flags);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_del(nint cursor, CursorDeleteOption flags);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_set_compare(nint txn, uint dbi, CompareFunction cmp);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_set_dupsort(nint txn, uint dbi, CompareFunction cmp);
        
    [LibraryImport(MDB_DLL_NAME)]
    [UnmanagedCallConv(CallConvs = new []{typeof(CallConvCdecl)})]
    public static partial MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, MDBValue[] value, CursorPutOptions flags);
}
#endif
    
public static partial class Lmdb
{
    private const string MDB_DLL_NAME = "lmdb";
    /// <summary>
    /// Duplicate keys may be used in the database. (Or, from another perspective, keys may have multiple data items, stored in sorted order.) By default keys must be unique and may have only a single data item.
    /// </summary>
    public const int MDB_DUPSORT = 0x04;

    /// <summary>
    /// This flag may only be used in combination with MDB_DUPSORT. This option tells the library that the data items for this database are all the same size, which allows further optimizations in storage and retrieval. When all data items are the same size, the MDB_GET_MULTIPLE and MDB_NEXT_MULTIPLE cursor operations may be used to retrieve multiple items at once.
    /// </summary>
    public const int MDB_DUPFIXED = 0x10;

    public static MDBResultCode mdb_env_set_mapsize(nint env, long size)
    {
        return mdb_env_set_mapsize(env, (nint)size);
    }

    public static MDBResultCode mdb_put(nint txn, uint dbi, MDBValue key, MDBValue value, PutOptions flags)
    {
        return mdb_put(txn, dbi, ref key, ref value, flags);
    }

    public static MDBResultCode mdb_del(nint txn, uint dbi, MDBValue key, MDBValue value)
    {
        return mdb_del(txn, dbi, ref key, ref value);
    }

    public static MDBResultCode mdb_del(nint txn, uint dbi, MDBValue key)
    {
        return mdb_del(txn, dbi, ref key,0);
    }

    public static MDBResultCode mdb_cursor_put(nint cursor, MDBValue key, MDBValue value, CursorPutOptions flags)
    {
        return mdb_cursor_put(cursor, ref key, ref value, flags);
    }

    /// <summary>
    /// store multiple contiguous data elements in a single request.
    /// May only be used with MDB_DUPFIXED.
    /// </summary>
    /// <param name="cursor">Pointer to cursor</param>
    /// <param name="key"><see cref="MDBValue"/> key</param>
    /// <param name="data">This span must be pinned or stackalloc memory</param>
    /// <param name="flags"><see cref="CursorPutOptions"/></param>
    public static MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref Span<MDBValue> data,
        CursorPutOptions flags)
    {
        ref var dataRef = ref MemoryMarshal.GetReference(data);
        return mdb_cursor_put(cursor, ref key, ref dataRef, flags);
    }

#if !NET7_0_OR_GREATER
        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_create(out nint env);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_env_close(nint env);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern MDBResultCode mdb_env_open(nint env, byte[] path, EnvironmentOpenFlags flags,
            UnixAccessMode mode);

        internal static MDBResultCode mdb_env_open(nint env, string path, EnvironmentOpenFlags flags,
            UnixAccessMode mode)
        {
            var bytes = Encoding.UTF8.GetBytes(path);
            return mdb_env_open(env, bytes, flags, mode);
        }

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_mapsize(nint env, nint size);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_get_maxreaders(nint env, out uint readers);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_maxreaders(nint env, uint readers);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_set_maxdbs(nint env, uint dbs);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_dbi_open(nint txn, string name, DatabaseOpenFlags flags, out uint db);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_dbi_close(nint env, uint dbi);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_drop(nint txn, uint dbi, bool del);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_begin(nint env, nint parent, TransactionBeginFlags flags, out nint txn);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_commit(nint txn);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_abort(nint txn);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_reset(nint txn);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_txn_renew(nint txn);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_version(out int major, out int minor, out int patch);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint mdb_strerror(int err);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_stat(nint txn, uint dbi, out MDBStat stat);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern MDBResultCode mdb_env_copy(nint env, byte[] path);

        public static MDBResultCode mdb_env_copy(nint env, string path)
        {
            var bytes = Encoding.UTF8.GetBytes(path);
            return mdb_env_copy(env, bytes);
        }

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern MDBResultCode mdb_env_copy2(nint env, byte[] path, EnvironmentCopyFlags copyFlags);

        public static MDBResultCode mdb_env_copy2(nint env, string path, EnvironmentCopyFlags copyFlags)
        {
            var bytes = Encoding.UTF8.GetBytes(path);
            return mdb_env_copy2(env, bytes, copyFlags);
        }

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_info(nint env, out MDBEnvInfo stat);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_stat(nint env, out MDBStat stat);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_env_sync(nint env, bool force);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_get(nint txn, uint dbi, ref MDBValue key, out MDBValue data);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_count(nint cursor, out int countp);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_put(nint txn, uint dbi, ref MDBValue key, ref MDBValue data, PutOptions flags);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, ref MDBValue data);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_del(nint txn, uint dbi, ref MDBValue key, nint data);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_open(nint txn, uint dbi, out nint cursor);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_cursor_close(nint cursor);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_renew(nint txn, nint cursor);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_get(nint cursor, ref MDBValue key, ref MDBValue data, CursorOperation op);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, ref MDBValue mdbValue, CursorPutOptions flags);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_del(nint cursor, CursorDeleteOption flags);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_set_compare(nint txn, uint dbi, CompareFunction cmp);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_set_dupsort(nint txn, uint dbi, CompareFunction cmp);

        [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MDBResultCode mdb_cursor_put(nint cursor, ref MDBValue key, MDBValue[] value, CursorPutOptions flags);
#endif 

#if NETCOREAPP3_1_OR_GREATER

    private static bool _shouldSetDllImportResolver = true;
    private static readonly object _syncRoot = new();

    public static void LoadWindowsAutoResizeLibrary()
    {
        if (!_shouldSetDllImportResolver) return;
        lock (_syncRoot)
        {
            if (!_shouldSetDllImportResolver) return;
            NativeLibrary.SetDllImportResolver(System.Reflection.Assembly.GetExecutingAssembly(), DllImportResolver);
            _shouldSetDllImportResolver = false;
        }
    }

    private static nint DllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName == MDB_DLL_NAME 
            ? NativeLibrary.Load($"{MDB_DLL_NAME}autoresize", assembly, searchPath) : 0;
    }
#endif
}