using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal static class LmdbMethods
    {
        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_create(out IntPtr env);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_env_close(IntPtr env);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_set_mapsize(IntPtr env, IntPtr size);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_get_maxreaders(IntPtr env, out uint readers);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_set_maxreaders(IntPtr env, uint readers);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_set_maxdbs(IntPtr env, uint dbs);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_dbi_close(IntPtr env, uint dbi);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_drop(IntPtr txn, uint dbi, bool del);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_txn_commit(IntPtr txn);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_abort(IntPtr txn);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_txn_reset(IntPtr txn);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_txn_renew(IntPtr txn);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mdb_version(out int major, out int minor, out int patch);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mdb_strerror(int err);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_copy(IntPtr env, string path);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_info(IntPtr env, out MDBEnvInfo stat);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_stat(IntPtr env, out MDBStat stat);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_env_sync(IntPtr env, bool force);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_get(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_put(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, IntPtr data);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mdb_cursor_close(IntPtr cursor);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_renew(IntPtr txn, IntPtr cursor);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure value, CursorPutOptions flags);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp);

        [DllImport("lmdb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ValueStructure[] value, CursorPutOptions flags);
    }
}