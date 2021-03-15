using System;

namespace LightningDB.Native
{
    public interface ILmdb
    {
        void mdb_cursor_close(IntPtr cursor);
        MDBResultCode mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);
        MDBResultCode mdb_cursor_get(IntPtr cursor, ref MDBValue key, ref MDBValue value, CursorOperation op);
        MDBResultCode mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor);
        MDBResultCode mdb_cursor_put(IntPtr cursor, MDBValue key, MDBValue value, CursorPutOptions flags);
        MDBResultCode mdb_cursor_put(IntPtr cursor, ref MDBValue key, ref Span<MDBValue> data, CursorPutOptions flags);
        MDBResultCode mdb_cursor_renew(IntPtr txn, IntPtr cursor);
        void mdb_dbi_close(IntPtr env, uint dbi);
        MDBResultCode mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db);
        MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key);
        MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key, MDBValue value);
        MDBResultCode mdb_drop(IntPtr txn, uint dbi, bool del);
        void mdb_env_close(IntPtr env);
        MDBResultCode mdb_env_copy(IntPtr env, string path);
        MDBResultCode mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags);
        MDBResultCode mdb_env_create(out IntPtr env);
        MDBResultCode mdb_env_get_maxreaders(IntPtr env, out uint readers);
        MDBResultCode mdb_env_info(IntPtr env, out MDBEnvInfo stat);
        MDBResultCode mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);
        MDBResultCode mdb_env_set_mapsize(IntPtr env, long size);
        MDBResultCode mdb_env_set_maxdbs(IntPtr env, uint dbs);
        MDBResultCode mdb_env_set_maxreaders(IntPtr env, uint readers);
        MDBResultCode mdb_env_stat(IntPtr env, out MDBStat stat);
        MDBResultCode mdb_env_sync(IntPtr env, bool force);
        MDBResultCode mdb_get(IntPtr txn, uint dbi, ref MDBValue key, out MDBValue value);
        MDBResultCode mdb_put(IntPtr txn, uint dbi, MDBValue key, MDBValue value, PutOptions flags);
        MDBResultCode mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp);
        MDBResultCode mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp);
        MDBResultCode mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);
        void mdb_txn_abort(IntPtr txn);
        MDBResultCode mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);
        MDBResultCode mdb_txn_commit(IntPtr txn);
        MDBResultCode mdb_txn_renew(IntPtr txn);
        void mdb_txn_reset(IntPtr txn);
        IntPtr mdb_version(out int major, out int minor, out int patch);
    }
}