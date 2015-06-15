using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal static class LmdbMethods
    {
#pragma warning disable 649
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_create_delegate(out IntPtr env);
        public static mdb_env_create_delegate mdb_env_create;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mdb_env_close_delegate(IntPtr env);
        public static mdb_env_close_delegate mdb_env_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_open_delegate(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);
        public static mdb_env_open_delegate mdb_env_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_set_mapsize_delegate(IntPtr env, IntPtr size);
        public static mdb_env_set_mapsize_delegate mdb_env_set_mapsize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_get_maxreaders_delegate(IntPtr env, out uint readers);
        public static mdb_env_get_maxreaders_delegate mdb_env_get_maxreaders;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_set_maxreaders_delegate(IntPtr env, uint readers);
        public static mdb_env_set_maxreaders_delegate mdb_env_set_maxreaders;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_set_maxdbs_delegate(IntPtr env, uint dbs);
        public static mdb_env_set_maxdbs_delegate mdb_env_set_maxdbs;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_dbi_open_delegate(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db);
        public static mdb_dbi_open_delegate mdb_dbi_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mdb_dbi_close_delegate(IntPtr env, uint dbi);
        public static mdb_dbi_close_delegate mdb_dbi_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_drop_delegate(IntPtr txn, uint dbi, bool del);
        public static mdb_drop_delegate mdb_drop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_txn_begin_delegate(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);
        public static mdb_txn_begin_delegate mdb_txn_begin;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_txn_commit_delegate(IntPtr txn);
        public static mdb_txn_commit_delegate mdb_txn_commit;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mdb_txn_abort_delegate(IntPtr txn);
        public static mdb_txn_abort_delegate mdb_txn_abort;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mdb_txn_reset_delegate(IntPtr txn);
        public static mdb_txn_reset_delegate mdb_txn_reset;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_txn_renew_delegate(IntPtr txn);
        public static mdb_txn_renew_delegate mdb_txn_renew;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr mdb_version_delegate(out IntPtr major, out IntPtr minor, out IntPtr patch);
        public static mdb_version_delegate mdb_version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr mdb_strerror_delegate(int err);
        public static mdb_strerror_delegate mdb_strerror;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_stat_delegate(IntPtr txn, uint dbi, out MDBStat stat);
        public static mdb_stat_delegate mdb_stat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_copy_delegate(IntPtr env, string path);
        public static mdb_env_copy_delegate mdb_env_copy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_copy2_delegate(IntPtr env, string path, EnvironmentCopyFlags copyFlags);
        public static mdb_env_copy2_delegate mdb_env_copy2;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_info_delegate(IntPtr env, out MDBEnvInfo stat);
        public static mdb_env_info_delegate mdb_env_info;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_stat_delegate(IntPtr env, out MDBStat stat);
        public static mdb_env_stat_delegate mdb_env_stat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_env_sync_delegate(IntPtr env, bool force);
        public static mdb_env_sync_delegate mdb_env_sync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_get_delegate(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data);
        public static mdb_get_delegate mdb_get;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_put_delegate(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);
        public static mdb_put_delegate mdb_put;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_del_delegate(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data);
        public static mdb_del_delegate mdb_del;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_cursor_open_delegate(IntPtr txn, uint dbi, out IntPtr cursor);
        public static mdb_cursor_open_delegate mdb_cursor_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mdb_cursor_close_delegate(IntPtr cursor);
        public static mdb_cursor_close_delegate mdb_cursor_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_cursor_renew_delegate(IntPtr txn, IntPtr cursor);
        public static mdb_cursor_renew_delegate mdb_cursor_renew;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_cursor_get_delegate(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);
        public static mdb_cursor_get_delegate mdb_cursor_get;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_cursor_put_delegate(IntPtr cursor, ref ValueStructure key, ref ValueStructure value, CursorPutOptions flags);
        public static mdb_cursor_put_delegate mdb_cursor_put;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_cursor_del_delegate(IntPtr cursor, CursorDeleteOption flags);
        public static mdb_cursor_del_delegate mdb_cursor_del;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_set_compare_delegate(IntPtr txn, uint dbi, CompareFunction cmp);
        public static mdb_set_compare_delegate mdb_set_compare;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mdb_set_dupsort_delegate(IntPtr txn, uint dbi, CompareFunction cmp);
        public static mdb_set_dupsort_delegate mdb_set_dupsort;

        public static class Overloads
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int mdb_cursor_put_delegate(IntPtr cursor, ref ValueStructure key, ValueStructure[] value, CursorPutOptions flags);
            public static mdb_cursor_put_delegate mdb_cursor_put;
        }
#pragma warning restore 649
    }
}