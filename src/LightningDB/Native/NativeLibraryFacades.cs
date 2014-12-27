using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native
{
	
	class Native32BitLibraryFacade : INativeLibraryFacade
    {
        public const string LibraryName = "lmdb32";

        #region Native functions

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_create(out IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_env_close(IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_mapsize(IntPtr env, IntPtr size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_get_maxreaders(IntPtr env, out UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxreaders(IntPtr env, UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxdbs(IntPtr env, UInt32 dbs);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out UInt32 db);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_dbi_close(IntPtr env, UInt32 dbi);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_drop(IntPtr txn, UInt32 dbi, bool del);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_commit(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_abort(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_reset(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_renew(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_strerror(int err);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_copy(IntPtr env, string path);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_info (IntPtr env, out MDBEnvInfo stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_sync(IntPtr env, bool force);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_get(IntPtr txn, UInt32 dbi, ref ValueStructure key, out ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_put(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, IntPtr data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_open(IntPtr txn, UInt32 dbi, out IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_cursor_close(IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_renew(IntPtr txn, IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);

        #endregion

        int INativeLibraryFacade.mdb_env_create(out IntPtr env)
        {
            return Native32BitLibraryFacade.mdb_env_create(out env);
        }

        void INativeLibraryFacade.mdb_env_close(IntPtr env)
        {
            Native32BitLibraryFacade.mdb_env_close(env);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return Native32BitLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode)
        {
            return Native32BitLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_set_mapsize(IntPtr env, long size)
        {		
			IntPtr sizeValue;
			if (size > Int32.MaxValue)
			{
				if (LightningConfig.Environment.AutoReduceMapSizeIn32BitProcess)
					sizeValue = new IntPtr(Int32.MaxValue);
				else
					throw new InvalidOperationException("Can't set MapSize larger than Int32.MaxValue in 32-bit process");
			}
			else
			{
				sizeValue = new IntPtr((int) size);
			}
			            return Native32BitLibraryFacade.mdb_env_set_mapsize(env, sizeValue);
        }

        int INativeLibraryFacade.mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return Native32BitLibraryFacade.mdb_env_get_maxreaders(env, out readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return Native32BitLibraryFacade.mdb_env_set_maxreaders(env, readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return Native32BitLibraryFacade.mdb_env_set_maxdbs(env, dbs);
        }

        int INativeLibraryFacade.mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            return Native32BitLibraryFacade.mdb_dbi_open(txn, name, flags, out db);
        }

        void INativeLibraryFacade.mdb_dbi_close(IntPtr env, uint dbi)
        {
            Native32BitLibraryFacade.mdb_dbi_close(env, dbi);
        }

        int INativeLibraryFacade.mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return Native32BitLibraryFacade.mdb_drop(txn, dbi, del);
        }

        int INativeLibraryFacade.mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return Native32BitLibraryFacade.mdb_txn_begin(env, parent, flags, out txn);
        }

        int INativeLibraryFacade.mdb_txn_commit(IntPtr txn)
        {
            return Native32BitLibraryFacade.mdb_txn_commit(txn);
        }

        void INativeLibraryFacade.mdb_txn_abort(IntPtr txn)
        {
            Native32BitLibraryFacade.mdb_txn_abort(txn);
        }

        void INativeLibraryFacade.mdb_txn_reset(IntPtr txn)
        {
            Native32BitLibraryFacade.mdb_txn_reset(txn);
        }

        int INativeLibraryFacade.mdb_txn_renew(IntPtr txn)
        {
            return Native32BitLibraryFacade.mdb_txn_renew(txn);
        }

        IntPtr INativeLibraryFacade.mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch)
        {
            return Native32BitLibraryFacade.mdb_version(out major, out minor, out patch);
        }

        IntPtr INativeLibraryFacade.mdb_strerror(int err)
        {
            return Native32BitLibraryFacade.mdb_strerror(err);
        }

        int INativeLibraryFacade.mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
        return Native32BitLibraryFacade.mdb_stat(txn, dbi, out stat);
        }

        int INativeLibraryFacade.mdb_env_copy(IntPtr env, string path)
        {
            return Native32BitLibraryFacade.mdb_env_copy(env, path);
        }

        int INativeLibraryFacade.mdb_env_info (IntPtr env, out MDBEnvInfo stat)
        {
            return Native32BitLibraryFacade.mdb_env_info(env, out stat);
        }

        int INativeLibraryFacade.mdb_env_sync(IntPtr env, bool force)
        {
            return Native32BitLibraryFacade.mdb_env_sync(env, force);
        }

        int INativeLibraryFacade.mdb_get(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data)
        {
            return Native32BitLibraryFacade.mdb_get(txn, dbi, ref key, out data);
        }

        int INativeLibraryFacade.mdb_put(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags)
        {
            return Native32BitLibraryFacade.mdb_put(txn, dbi, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data)
        {
            return Native32BitLibraryFacade.mdb_del(txn, dbi, ref key, ref data);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, IntPtr data)
        {
            return Native32BitLibraryFacade.mdb_del(txn, dbi, ref key, data);
        }

        int INativeLibraryFacade.mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return Native32BitLibraryFacade.mdb_cursor_open(txn, dbi, out cursor);
        }

        void INativeLibraryFacade.mdb_cursor_close(IntPtr cursor)
        {
            Native32BitLibraryFacade.mdb_cursor_close(cursor);
        }

        int INativeLibraryFacade.mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return Native32BitLibraryFacade.mdb_cursor_renew(txn, cursor);
        }

        int INativeLibraryFacade.mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op)
        {
            return Native32BitLibraryFacade.mdb_cursor_get(cursor, ref key, ref data, op);
        }

        int INativeLibraryFacade.mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags)
        {
            return Native32BitLibraryFacade.mdb_cursor_put(cursor, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return Native32BitLibraryFacade.mdb_cursor_del(cursor, flags);
        }
	}
	
	class Native64BitLibraryFacade : INativeLibraryFacade
    {
        public const string LibraryName = "lmdb64";

        #region Native functions

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_create(out IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_env_close(IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_mapsize(IntPtr env, IntPtr size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_get_maxreaders(IntPtr env, out UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxreaders(IntPtr env, UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxdbs(IntPtr env, UInt32 dbs);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out UInt32 db);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_dbi_close(IntPtr env, UInt32 dbi);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_drop(IntPtr txn, UInt32 dbi, bool del);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_commit(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_abort(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_reset(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_renew(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_strerror(int err);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_copy(IntPtr env, string path);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_info (IntPtr env, out MDBEnvInfo stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_sync(IntPtr env, bool force);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_get(IntPtr txn, UInt32 dbi, ref ValueStructure key, out ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_put(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, IntPtr data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_open(IntPtr txn, UInt32 dbi, out IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_cursor_close(IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_renew(IntPtr txn, IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);

        #endregion

        int INativeLibraryFacade.mdb_env_create(out IntPtr env)
        {
            return Native64BitLibraryFacade.mdb_env_create(out env);
        }

        void INativeLibraryFacade.mdb_env_close(IntPtr env)
        {
            Native64BitLibraryFacade.mdb_env_close(env);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return Native64BitLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode)
        {
            return Native64BitLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_set_mapsize(IntPtr env, long size)
        {			var sizeValue = new IntPtr(size);
			            return Native64BitLibraryFacade.mdb_env_set_mapsize(env, sizeValue);
        }

        int INativeLibraryFacade.mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return Native64BitLibraryFacade.mdb_env_get_maxreaders(env, out readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return Native64BitLibraryFacade.mdb_env_set_maxreaders(env, readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return Native64BitLibraryFacade.mdb_env_set_maxdbs(env, dbs);
        }

        int INativeLibraryFacade.mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            return Native64BitLibraryFacade.mdb_dbi_open(txn, name, flags, out db);
        }

        void INativeLibraryFacade.mdb_dbi_close(IntPtr env, uint dbi)
        {
            Native64BitLibraryFacade.mdb_dbi_close(env, dbi);
        }

        int INativeLibraryFacade.mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return Native64BitLibraryFacade.mdb_drop(txn, dbi, del);
        }

        int INativeLibraryFacade.mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return Native64BitLibraryFacade.mdb_txn_begin(env, parent, flags, out txn);
        }

        int INativeLibraryFacade.mdb_txn_commit(IntPtr txn)
        {
            return Native64BitLibraryFacade.mdb_txn_commit(txn);
        }

        void INativeLibraryFacade.mdb_txn_abort(IntPtr txn)
        {
            Native64BitLibraryFacade.mdb_txn_abort(txn);
        }

        void INativeLibraryFacade.mdb_txn_reset(IntPtr txn)
        {
            Native64BitLibraryFacade.mdb_txn_reset(txn);
        }

        int INativeLibraryFacade.mdb_txn_renew(IntPtr txn)
        {
            return Native64BitLibraryFacade.mdb_txn_renew(txn);
        }

        IntPtr INativeLibraryFacade.mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch)
        {
            return Native64BitLibraryFacade.mdb_version(out major, out minor, out patch);
        }

        IntPtr INativeLibraryFacade.mdb_strerror(int err)
        {
            return Native64BitLibraryFacade.mdb_strerror(err);
        }

        int INativeLibraryFacade.mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
        return Native64BitLibraryFacade.mdb_stat(txn, dbi, out stat);
        }

        int INativeLibraryFacade.mdb_env_copy(IntPtr env, string path)
        {
            return Native64BitLibraryFacade.mdb_env_copy(env, path);
        }

        int INativeLibraryFacade.mdb_env_info (IntPtr env, out MDBEnvInfo stat)
        {
            return Native64BitLibraryFacade.mdb_env_info(env, out stat);
        }

        int INativeLibraryFacade.mdb_env_sync(IntPtr env, bool force)
        {
            return Native64BitLibraryFacade.mdb_env_sync(env, force);
        }

        int INativeLibraryFacade.mdb_get(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data)
        {
            return Native64BitLibraryFacade.mdb_get(txn, dbi, ref key, out data);
        }

        int INativeLibraryFacade.mdb_put(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags)
        {
            return Native64BitLibraryFacade.mdb_put(txn, dbi, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data)
        {
            return Native64BitLibraryFacade.mdb_del(txn, dbi, ref key, ref data);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, IntPtr data)
        {
            return Native64BitLibraryFacade.mdb_del(txn, dbi, ref key, data);
        }

        int INativeLibraryFacade.mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return Native64BitLibraryFacade.mdb_cursor_open(txn, dbi, out cursor);
        }

        void INativeLibraryFacade.mdb_cursor_close(IntPtr cursor)
        {
            Native64BitLibraryFacade.mdb_cursor_close(cursor);
        }

        int INativeLibraryFacade.mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return Native64BitLibraryFacade.mdb_cursor_renew(txn, cursor);
        }

        int INativeLibraryFacade.mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op)
        {
            return Native64BitLibraryFacade.mdb_cursor_get(cursor, ref key, ref data, op);
        }

        int INativeLibraryFacade.mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags)
        {
            return Native64BitLibraryFacade.mdb_cursor_put(cursor, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return Native64BitLibraryFacade.mdb_cursor_del(cursor, flags);
        }
	}
	
	class FallbackLibraryFacade : INativeLibraryFacade
    {
        public const string LibraryName = "lmdb";

        #region Native functions

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_create(out IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_env_close(IntPtr env);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_mapsize(IntPtr env, IntPtr size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_get_maxreaders(IntPtr env, out UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxreaders(IntPtr env, UInt32 readers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_set_maxdbs(IntPtr env, UInt32 dbs);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out UInt32 db);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_dbi_close(IntPtr env, UInt32 dbi);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_drop(IntPtr txn, UInt32 dbi, bool del);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_commit(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_abort(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_txn_reset(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_txn_renew(IntPtr txn);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mdb_strerror(int err);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_copy(IntPtr env, string path);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_info (IntPtr env, out MDBEnvInfo stat);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_env_sync(IntPtr env, bool force);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_get(IntPtr txn, UInt32 dbi, ref ValueStructure key, out ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_put(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, IntPtr data);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_open(IntPtr txn, UInt32 dbi, out IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void mdb_cursor_close(IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_renew(IntPtr txn, IntPtr cursor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);

        #endregion

        int INativeLibraryFacade.mdb_env_create(out IntPtr env)
        {
            return FallbackLibraryFacade.mdb_env_create(out env);
        }

        void INativeLibraryFacade.mdb_env_close(IntPtr env)
        {
            FallbackLibraryFacade.mdb_env_close(env);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return FallbackLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode)
        {
            return FallbackLibraryFacade.mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_set_mapsize(IntPtr env, long size)
        {		
			IntPtr sizeValue;
			if (!Environment.Is64BitProcess && size > Int32.MaxValue)
			{
				if (LightningConfig.Environment.AutoReduceMapSizeIn32BitProcess)
					sizeValue = new IntPtr(Int32.MaxValue);
				else
					throw new InvalidOperationException("Can't set MapSize larger than Int32.MaxValue in 32-bit process");
			}
			else
			{
				sizeValue = new IntPtr((int) size);
			}
			            return FallbackLibraryFacade.mdb_env_set_mapsize(env, sizeValue);
        }

        int INativeLibraryFacade.mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return FallbackLibraryFacade.mdb_env_get_maxreaders(env, out readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return FallbackLibraryFacade.mdb_env_set_maxreaders(env, readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return FallbackLibraryFacade.mdb_env_set_maxdbs(env, dbs);
        }

        int INativeLibraryFacade.mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            return FallbackLibraryFacade.mdb_dbi_open(txn, name, flags, out db);
        }

        void INativeLibraryFacade.mdb_dbi_close(IntPtr env, uint dbi)
        {
            FallbackLibraryFacade.mdb_dbi_close(env, dbi);
        }

        int INativeLibraryFacade.mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return FallbackLibraryFacade.mdb_drop(txn, dbi, del);
        }

        int INativeLibraryFacade.mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return FallbackLibraryFacade.mdb_txn_begin(env, parent, flags, out txn);
        }

        int INativeLibraryFacade.mdb_txn_commit(IntPtr txn)
        {
            return FallbackLibraryFacade.mdb_txn_commit(txn);
        }

        void INativeLibraryFacade.mdb_txn_abort(IntPtr txn)
        {
            FallbackLibraryFacade.mdb_txn_abort(txn);
        }

        void INativeLibraryFacade.mdb_txn_reset(IntPtr txn)
        {
            FallbackLibraryFacade.mdb_txn_reset(txn);
        }

        int INativeLibraryFacade.mdb_txn_renew(IntPtr txn)
        {
            return FallbackLibraryFacade.mdb_txn_renew(txn);
        }

        IntPtr INativeLibraryFacade.mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch)
        {
            return FallbackLibraryFacade.mdb_version(out major, out minor, out patch);
        }

        IntPtr INativeLibraryFacade.mdb_strerror(int err)
        {
            return FallbackLibraryFacade.mdb_strerror(err);
        }

        int INativeLibraryFacade.mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
        return FallbackLibraryFacade.mdb_stat(txn, dbi, out stat);
        }

        int INativeLibraryFacade.mdb_env_copy(IntPtr env, string path)
        {
            return FallbackLibraryFacade.mdb_env_copy(env, path);
        }

        int INativeLibraryFacade.mdb_env_info (IntPtr env, out MDBEnvInfo stat)
        {
            return FallbackLibraryFacade.mdb_env_info(env, out stat);
        }

        int INativeLibraryFacade.mdb_env_sync(IntPtr env, bool force)
        {
            return FallbackLibraryFacade.mdb_env_sync(env, force);
        }

        int INativeLibraryFacade.mdb_get(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data)
        {
            return FallbackLibraryFacade.mdb_get(txn, dbi, ref key, out data);
        }

        int INativeLibraryFacade.mdb_put(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags)
        {
            return FallbackLibraryFacade.mdb_put(txn, dbi, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data)
        {
            return FallbackLibraryFacade.mdb_del(txn, dbi, ref key, ref data);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, IntPtr data)
        {
            return FallbackLibraryFacade.mdb_del(txn, dbi, ref key, data);
        }

        int INativeLibraryFacade.mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return FallbackLibraryFacade.mdb_cursor_open(txn, dbi, out cursor);
        }

        void INativeLibraryFacade.mdb_cursor_close(IntPtr cursor)
        {
            FallbackLibraryFacade.mdb_cursor_close(cursor);
        }

        int INativeLibraryFacade.mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return FallbackLibraryFacade.mdb_cursor_renew(txn, cursor);
        }

        int INativeLibraryFacade.mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op)
        {
            return FallbackLibraryFacade.mdb_cursor_get(cursor, ref key, ref data, op);
        }

        int INativeLibraryFacade.mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags)
        {
            return FallbackLibraryFacade.mdb_cursor_put(cursor, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return FallbackLibraryFacade.mdb_cursor_del(cursor, flags);
        }
	}
	
}
