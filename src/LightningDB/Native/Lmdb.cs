using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    public static class Lmdb
    {
        /// <summary>
        /// Duplicate keys may be used in the database. (Or, from another perspective, keys may have multiple data items, stored in sorted order.) By default keys must be unique and may have only a single data item.
        /// </summary>
        public const int MDB_DUPSORT = 0x04;

        /// <summary>
        /// This flag may only be used in combination with MDB_DUPSORT. This option tells the library that the data items for this database are all the same size, which allows further optimizations in storage and retrieval. When all data items are the same size, the MDB_GET_MULTIPLE and MDB_NEXT_MULTIPLE cursor operations may be used to retrieve multiple items at once.
        /// </summary>
        public const int MDB_DUPFIXED = 0x10;

        public static MDBResultCode mdb_env_create(out IntPtr env)
        {
            return LmdbMethods.mdb_env_create(out env);
        }

        public static void mdb_env_close(IntPtr env)
        {
            LmdbMethods.mdb_env_close(env);
        }

        public static MDBResultCode mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return LmdbMethods.mdb_env_open(env, path, flags, mode);
        }

        public static MDBResultCode mdb_env_set_mapsize(IntPtr env, long size)
        {
            return LmdbMethods.mdb_env_set_mapsize(env, new IntPtr(size));
        }

        public static MDBResultCode mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return LmdbMethods.mdb_env_get_maxreaders(env, out readers);
        }

        public static MDBResultCode mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return LmdbMethods.mdb_env_set_maxreaders(env, readers);
        }

        public static MDBResultCode mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return LmdbMethods.mdb_env_set_maxdbs(env, dbs);
        }

        public static MDBResultCode mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            return LmdbMethods.mdb_dbi_open(txn, name, flags, out db);
        }

        public static void mdb_dbi_close(IntPtr env, uint dbi)
        {
            LmdbMethods.mdb_dbi_close(env, dbi);
        }

        public static MDBResultCode mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return LmdbMethods.mdb_drop(txn, dbi, del);
        }

        public static MDBResultCode mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return LmdbMethods.mdb_txn_begin(env, parent, flags, out txn);
        }

        public static MDBResultCode mdb_txn_commit(IntPtr txn)
        {
            return LmdbMethods.mdb_txn_commit(txn);
        }

        public static void mdb_txn_abort(IntPtr txn)
        {
            LmdbMethods.mdb_txn_abort(txn);
        }

        public static void mdb_txn_reset(IntPtr txn)
        {
            LmdbMethods.mdb_txn_reset(txn);
        }

        public static MDBResultCode mdb_txn_renew(IntPtr txn)
        {
            return LmdbMethods.mdb_txn_renew(txn);
        }

        public static IntPtr mdb_version(out int major, out int minor, out int patch)
        {
            return LmdbMethods.mdb_version(out major, out minor, out patch);
        }

        public static MDBResultCode mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
            return LmdbMethods.mdb_stat(txn, dbi, out stat);
        }

        public static MDBResultCode mdb_env_copy(IntPtr env, string path)
        {
            return LmdbMethods.mdb_env_copy(env, path);
        }

        public static MDBResultCode mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags)
        {
            return LmdbMethods.mdb_env_copy2(env, path, copyFlags);
        }

        public static MDBResultCode mdb_env_info(IntPtr env, out MDBEnvInfo stat)
        {
            return LmdbMethods.mdb_env_info(env, out stat);
        }

        public static MDBResultCode mdb_env_stat(IntPtr env, out MDBStat stat)
        {
            return LmdbMethods.mdb_env_stat(env, out stat);
        }

        public static MDBResultCode mdb_env_sync(IntPtr env, bool force)
        {
            return LmdbMethods.mdb_env_sync(env, force);
        }

        public static MDBResultCode mdb_get(IntPtr txn, uint dbi, ref MDBValue key, out MDBValue value)
        {
            return LmdbMethods.mdb_get(txn, dbi, ref key, out value);
        }

        public static MDBResultCode mdb_put(IntPtr txn, uint dbi, MDBValue key, MDBValue value, PutOptions flags)
        {
            return LmdbMethods.mdb_put(txn, dbi, ref key, ref value, flags);
        }

        public static MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key, MDBValue value)
        {
            return LmdbMethods.mdb_del(txn, dbi, ref key, ref value);
        }

        public static MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key)
        {
            return LmdbMethods.mdb_del(txn, dbi, ref key, IntPtr.Zero);
        }

        public static MDBResultCode mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return LmdbMethods.mdb_cursor_open(txn, dbi, out cursor);
        }

        public static void mdb_cursor_close(IntPtr cursor)
        {
            LmdbMethods.mdb_cursor_close(cursor);
        }

        public static MDBResultCode mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return LmdbMethods.mdb_cursor_renew(txn, cursor);
        }

        public static MDBResultCode mdb_cursor_get(IntPtr cursor, ref MDBValue key, ref MDBValue value, CursorOperation op)
        {
            return LmdbMethods.mdb_cursor_get(cursor, ref key, ref value, op);
        }

        public static MDBResultCode mdb_cursor_put(IntPtr cursor, MDBValue key, MDBValue value, CursorPutOptions flags)
        {
            return LmdbMethods.mdb_cursor_put(cursor, ref key, ref value, flags);
        }

        /// <summary>
        /// store multiple contiguous data elements in a single request.
        /// May only be used with MDB_DUPFIXED.
        /// </summary>
        /// <param name="data">This span must be pinned or stackalloc memory</param>
        public static MDBResultCode mdb_cursor_put(IntPtr cursor, ref MDBValue key, ref Span<MDBValue> data, CursorPutOptions flags)
        {
            ref var dataRef = ref MemoryMarshal.GetReference(data);
            return LmdbMethods.mdb_cursor_put(cursor, ref key, ref dataRef, flags);
        }

        public static MDBResultCode mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return LmdbMethods.mdb_cursor_del(cursor, flags);
        }

        public static MDBResultCode mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return LmdbMethods.mdb_set_compare(txn, dbi, cmp);
        }

        public static MDBResultCode mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return LmdbMethods.mdb_set_dupsort(txn, dbi, cmp);
        }
    }
}
