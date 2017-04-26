using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    public static class Lmdb
    {
        /// <summary>
        /// Txn has too many dirty pages
        /// </summary>
        public const int MDB_TXN_FULL = -30788;

        /// <summary>
        /// Environment mapsize reached
        /// </summary>
        public const int MDB_MAP_FULL = -30792;

        /// <summary>
        /// File is not a valid MDB file.
        /// </summary>
        public const int MDB_INVALID = -30793;

        /// <summary>
        /// Environment version mismatch.
        /// </summary>
        public const int MDB_VERSION_MISMATCH = -30794;

        /// <summary>
        /// Update of meta page failed, probably I/O error
        /// </summary>
        public const int MDB_PANIC = -30795;

        /// <summary>
        /// Database contents grew beyond environment mapsize
        /// </summary>
        public const int MDB_MAP_RESIZED = -30785;

        /// <summary>
        /// Environment maxreaders reached
        /// </summary>
        public const int MDB_READERS_FULL = -30790;

        /// <summary>
        /// Environment maxdbs reached
        /// </summary>
        public const int MDB_DBS_FULL = -30791;

        /// <summary>
        /// key/data pair not found (EOF)
        /// </summary>
        public const int MDB_NOTFOUND = -30798;

        /// <summary>
        /// Duplicate keys may be used in the database. (Or, from another perspective, keys may have multiple data items, stored in sorted order.) By default keys must be unique and may have only a single data item.
        /// </summary>
        public const int MDB_DUPSORT = 0x04;

        /// <summary>
        /// This flag may only be used in combination with MDB_DUPSORT. This option tells the library that the data items for this database are all the same size, which allows further optimizations in storage and retrieval. When all data items are the same size, the MDB_GET_MULTIPLE and MDB_NEXT_MULTIPLE cursor operations may be used to retrieve multiple items at once.
        /// </summary>
        public const int MDB_DUPFIXED = 0x10;

        static int check(int statusCode)
        {
            if (statusCode != 0)
            {
                var message = mdb_strerror(statusCode);
                throw new LightningException(message, statusCode);
            }
            return statusCode;
        }

        static int checkRead(int statusCode)
        {
            return statusCode == MDB_NOTFOUND ? statusCode : check(statusCode);
        }

        public static int mdb_env_create(out IntPtr env)
        {
            return check(LmdbMethods.mdb_env_create(out env));
        }

        public static void mdb_env_close(IntPtr env)
        {
            LmdbMethods.mdb_env_close(env);
        }

        public static int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return check(LmdbMethods.mdb_env_open(env, path, flags, mode));
        }

        public static int mdb_env_set_mapsize(IntPtr env, long size)
        {
            return check(LmdbMethods.mdb_env_set_mapsize(env, new IntPtr(size)));
        }

        public static int mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return check(LmdbMethods.mdb_env_get_maxreaders(env, out readers));
        }

        public static int mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return check(LmdbMethods.mdb_env_set_maxreaders(env, readers));
        }

        public static int mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return check(LmdbMethods.mdb_env_set_maxdbs(env, dbs));
        }

        public static int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            var statusCode = LmdbMethods.mdb_dbi_open(txn, name, flags, out db);
            if(statusCode == MDB_NOTFOUND)
                throw new LightningException($"Error opening database {name}: {mdb_strerror(statusCode)}", statusCode);
            return check(statusCode);
        }

        public static void mdb_dbi_close(IntPtr env, uint dbi)
        {
            LmdbMethods.mdb_dbi_close(env, dbi);
        }

        public static int mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return check(LmdbMethods.mdb_drop(txn, dbi, del));
        }

        public static int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return check(LmdbMethods.mdb_txn_begin(env, parent, flags, out txn));
        }

        public static int mdb_txn_commit(IntPtr txn)
        {
            return check(LmdbMethods.mdb_txn_commit(txn));
        }

        public static void mdb_txn_abort(IntPtr txn)
        {
            LmdbMethods.mdb_txn_abort(txn);
        }

        public static void mdb_txn_reset(IntPtr txn)
        {
            LmdbMethods.mdb_txn_reset(txn);
        }

        public static int mdb_txn_renew(IntPtr txn)
        {
            return check(LmdbMethods.mdb_txn_renew(txn));
        }

        public static IntPtr mdb_version(out int major, out int minor, out int patch)
        {
            return LmdbMethods.mdb_version(out major, out minor, out patch);
        }

        public static string mdb_strerror(int err)
        {
            var ptr = LmdbMethods.mdb_strerror(err);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
            return check(LmdbMethods.mdb_stat(txn, dbi, out stat));
        }

        public static int mdb_env_copy(IntPtr env, string path)
        {
            return check(LmdbMethods.mdb_env_copy(env, path));
        }

        public static int mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags)
        {
            return check(LmdbMethods.mdb_env_copy2(env, path, copyFlags));
        }

        public static int mdb_env_info(IntPtr env, out MDBEnvInfo stat)
        {
            return check(LmdbMethods.mdb_env_info(env, out stat));
        }

        public static int mdb_env_stat(IntPtr env, out MDBStat stat)
        {
            return check(LmdbMethods.mdb_env_stat(env, out stat));
        }

        public static int mdb_env_sync(IntPtr env, bool force)
        {
            return check(LmdbMethods.mdb_env_sync(env, force));
        }

        public static int mdb_get(IntPtr txn, uint dbi, byte[] key, out byte[] data)
        {
            using (var marshal = new MarshalValueStructure(key))
            {
                ValueStructure value;
                var result = checkRead(LmdbMethods.mdb_get(txn, dbi, ref marshal.Key, out value));
                if (result == MDB_NOTFOUND)
                {
                    data = null;
                    return result;
                }
                data = value.GetBytes();
                return result;
            }
        }

        public static int mdb_put(IntPtr txn, uint dbi, byte[] key, byte[] value, PutOptions flags)
        {
            using(var marshal = new MarshalValueStructure(key, value))
                return check(LmdbMethods.mdb_put(txn, dbi, ref marshal.Key, ref marshal.Value, flags));
        }

        public static int mdb_del(IntPtr txn, uint dbi, byte[] key, byte[] value)
        {
            using(var marshal = new MarshalValueStructure(key, value))
                return check(LmdbMethods.mdb_del(txn, dbi, ref marshal.Key, ref marshal.Value));
        }

        public static int mdb_del(IntPtr txn, uint dbi, byte[] key)
        {
            using(var marshal = new MarshalValueStructure(key))
                return check(LmdbMethods.mdb_del(txn, dbi, ref marshal.Key, IntPtr.Zero));
        }

        public static int mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return check(LmdbMethods.mdb_cursor_open(txn, dbi, out cursor));
        }

        public static void mdb_cursor_close(IntPtr cursor)
        {
            LmdbMethods.mdb_cursor_close(cursor);
        }

        public static int mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return check(LmdbMethods.mdb_cursor_renew(txn, cursor));
        }

        public static int mdb_cursor_get(IntPtr cursor, byte[] key, out ValueStructure keyStructure, out ValueStructure valueStructure, CursorOperation op)
        {
            valueStructure = default(ValueStructure);
            using (var marshal = new MarshalValueStructure(key))
            {
                keyStructure = marshal.Key;
                return checkRead(LmdbMethods.mdb_cursor_get(cursor, ref keyStructure, ref valueStructure, op));
            }
        }
        public static int mdb_cursor_get(IntPtr cursor, byte[] key, byte[] value, CursorOperation op)
        {
            using(var marshal = new MarshalValueStructure(key, value))
                return checkRead(LmdbMethods.mdb_cursor_get(cursor, ref marshal.Key, ref marshal.Value, op));
        }

        public static int mdb_cursor_get(IntPtr cursor, out ValueStructure key, out ValueStructure value, CursorOperation op)
        {
            key = value = default(ValueStructure);
            return checkRead(LmdbMethods.mdb_cursor_get(cursor, ref key, ref value, op));
        }

        public static int mdb_cursor_get_multiple(IntPtr cursor, ref ValueStructure key, ref ValueStructure value, CursorOperation op)
        {
            return checkRead(LmdbMethods.mdb_cursor_get(cursor, ref key, ref value, op));
        }

        public static int mdb_cursor_put(IntPtr cursor, byte[] key, byte[] value, CursorPutOptions flags)
        {
            using(var marshal = new MarshalValueStructure(key, value))
                return check(LmdbMethods.mdb_cursor_put(cursor, ref marshal.Key, ref marshal.Value, flags));
        }

        public static int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ValueStructure[] data, CursorPutOptions flags)
        {
            return check(LmdbMethods.mdb_cursor_put(cursor, ref key, data, flags));
        }

        public static int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return check(LmdbMethods.mdb_cursor_del(cursor, flags));
        }

        public static int mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return check(LmdbMethods.mdb_set_compare(txn, dbi, cmp));
        }

        public static int mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return check(LmdbMethods.mdb_set_dupsort(txn, dbi, cmp));
        }
    }
}
