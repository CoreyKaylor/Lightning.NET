using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
#if DNXCORE50 || DNX451
    public class DnxLibraryFacade : INativeLibraryFacade
    {
        public DnxLibraryFacade(DnxLibraryLoader libraryLoader, string path)
        {
            if (libraryLoader.IsWindows)
            {
                path = Path.Combine(path, "Binaries", IntPtr.Size == 4 ? "lmdb32.dll" : "lmdb64.dll");
            }
            else if (libraryLoader.IsDarwin)
            {
                path = Path.Combine(path, "Binaries", "liblmdb.dylib");
            }
            else
            {
                path = "lmdb.so";
            }
            var module = libraryLoader.LoadLibrary(path);
            if (module == IntPtr.Zero)
                throw new InvalidOperationException("Unable to load lmdb.");

            foreach (var field in GetType().GetTypeInfo().DeclaredFields)
            {
                var procAddress = libraryLoader.GetProcAddress(module, field.Name.TrimStart('_').TrimStart('_'));
                if (procAddress == IntPtr.Zero)
                    continue;

                var value = Marshal.GetDelegateForFunctionPointer(procAddress, field.FieldType);
                field.SetValue(this, value);
            }
        }

#pragma warning disable 649
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_create(out IntPtr env);
        mdb_env_create _mdb_env_create;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void mdb_env_close(IntPtr env);
        mdb_env_close _mdb_env_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode);
        mdb_env_open _mdb_env_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_set_mapsize(IntPtr env, IntPtr size);
        mdb_env_set_mapsize _mdb_env_set_mapsize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_get_maxreaders(IntPtr env, out UInt32 readers);
        mdb_env_get_maxreaders _mdb_env_get_maxreaders;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_set_maxreaders(IntPtr env, UInt32 readers);
        mdb_env_set_maxreaders _mdb_env_set_maxreaders;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_set_maxdbs(IntPtr env, UInt32 dbs);
        mdb_env_set_maxdbs _mdb_env_set_maxdbs;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out UInt32 db);
        mdb_dbi_open _mdb_dbi_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void mdb_dbi_close(IntPtr env, UInt32 dbi);
        mdb_dbi_close _mdb_dbi_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_drop(IntPtr txn, UInt32 dbi, bool del);
        mdb_drop _mdb_drop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);
        mdb_txn_begin _mdb_txn_begin;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_txn_commit(IntPtr txn);
        mdb_txn_commit _mdb_txn_commit;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void mdb_txn_abort(IntPtr txn);
        mdb_txn_abort _mdb_txn_abort;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void mdb_txn_reset(IntPtr txn);
        mdb_txn_reset _mdb_txn_reset;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_txn_renew(IntPtr txn);
        mdb_txn_renew _mdb_txn_renew;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch);
        mdb_version _mdb_version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr mdb_strerror(int err);
        mdb_strerror _mdb_strerror;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);
        mdb_stat _mdb_stat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_copy(IntPtr env, string path);
        mdb_env_copy _mdb_env_copy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags);
        mdb_env_copy2 _mdb_env_copy2;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_info(IntPtr env, out MDBEnvInfo stat);
        mdb_env_info _mdb_env_info;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_stat(IntPtr env, out MDBStat stat);
        mdb_env_stat _mdb_env_stat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_env_sync(IntPtr env, bool force);
        mdb_env_sync _mdb_env_sync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_get(IntPtr txn, UInt32 dbi, ref ValueStructure key, out ValueStructure data);
        mdb_get _mdb_get;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_put(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags);
        mdb_put _mdb_put;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_del(IntPtr txn, UInt32 dbi, ref ValueStructure key, ref ValueStructure data);
        mdb_del _mdb_del;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_del_ptr(IntPtr txn, UInt32 dbi, ref ValueStructure key, IntPtr data);
        mdb_del_ptr __mdb_del;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_open(IntPtr txn, UInt32 dbi, out IntPtr cursor);
        mdb_cursor_open _mdb_cursor_open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void mdb_cursor_close(IntPtr cursor);
        mdb_cursor_close _mdb_cursor_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_renew(IntPtr txn, IntPtr cursor);
        mdb_cursor_renew _mdb_cursor_renew;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op);
        mdb_cursor_get _mdb_cursor_get;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags);
        mdb_cursor_put _mdb_cursor_put;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_put_multi(IntPtr cursor, ref ValueStructure key, ValueStructure[] data, CursorPutOptions flags);
        mdb_cursor_put_multi __mdb_cursor_put;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);
        mdb_cursor_del _mdb_cursor_del;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp);
        mdb_set_compare _mdb_set_compare;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp);
        mdb_set_dupsort _mdb_set_dupsort;
#pragma warning restore 649


        int INativeLibraryFacade.mdb_env_create(out IntPtr env)
        {
            return _mdb_env_create(out env);
        }

        void INativeLibraryFacade.mdb_env_close(IntPtr env)
        {
            _mdb_env_close(env);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, UnixAccessMode mode)
        {
            return _mdb_env_open(env, path, flags, mode);
        }

        int INativeLibraryFacade.mdb_env_open(IntPtr env, string path, EnvironmentOpenFlags flags, int mode)
        {
            return _mdb_env_open(env, path, flags, (UnixAccessMode)mode);
        }

        int INativeLibraryFacade.mdb_env_set_mapsize(IntPtr env, long size)
        {
            IntPtr sizeValue;
            if (IntPtr.Size == 4 && size > Int32.MaxValue)
            {
                if (LightningConfig.Environment.AutoReduceMapSizeIn32BitProcess)
                    sizeValue = new IntPtr(Int32.MaxValue);
                else
                    throw new InvalidOperationException("Can't set MapSize larger than Int32.MaxValue in 32-bit process");
            }
            else
            {
                sizeValue = new IntPtr((int)size);
            }
            return _mdb_env_set_mapsize(env, sizeValue);
        }

        int INativeLibraryFacade.mdb_env_get_maxreaders(IntPtr env, out uint readers)
        {
            return _mdb_env_get_maxreaders(env, out readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxreaders(IntPtr env, uint readers)
        {
            return _mdb_env_set_maxreaders(env, readers);
        }

        int INativeLibraryFacade.mdb_env_set_maxdbs(IntPtr env, uint dbs)
        {
            return _mdb_env_set_maxdbs(env, dbs);
        }

        int INativeLibraryFacade.mdb_dbi_open(IntPtr txn, string name, DatabaseOpenFlags flags, out uint db)
        {
            return _mdb_dbi_open(txn, name, flags, out db);
        }

        void INativeLibraryFacade.mdb_dbi_close(IntPtr env, uint dbi)
        {
            _mdb_dbi_close(env, dbi);
        }

        int INativeLibraryFacade.mdb_drop(IntPtr txn, uint dbi, bool del)
        {
            return _mdb_drop(txn, dbi, del);
        }

        int INativeLibraryFacade.mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn)
        {
            return _mdb_txn_begin(env, parent, flags, out txn);
        }

        int INativeLibraryFacade.mdb_txn_commit(IntPtr txn)
        {
            return _mdb_txn_commit(txn);
        }

        void INativeLibraryFacade.mdb_txn_abort(IntPtr txn)
        {
            _mdb_txn_abort(txn);
        }

        void INativeLibraryFacade.mdb_txn_reset(IntPtr txn)
        {
            _mdb_txn_reset(txn);
        }

        int INativeLibraryFacade.mdb_txn_renew(IntPtr txn)
        {
            return _mdb_txn_renew(txn);
        }

        IntPtr INativeLibraryFacade.mdb_version(out IntPtr major, out IntPtr minor, out IntPtr patch)
        {
            return _mdb_version(out major, out minor, out patch);
        }

        IntPtr INativeLibraryFacade.mdb_strerror(int err)
        {
            return _mdb_strerror(err);
        }

        int INativeLibraryFacade.mdb_stat(IntPtr txn, uint dbi, out MDBStat stat)
        {
            return _mdb_stat(txn, dbi, out stat);
        }

        int INativeLibraryFacade.mdb_env_copy(IntPtr env, string path)
        {
            return _mdb_env_copy(env, path);
        }

        int INativeLibraryFacade.mdb_env_copy2(IntPtr env, string path, EnvironmentCopyFlags copyFlags)
        {
            return _mdb_env_copy2(env, path, copyFlags);
        }

        int INativeLibraryFacade.mdb_env_info(IntPtr env, out MDBEnvInfo stat)
        {
            return _mdb_env_info(env, out stat);
        }

        int INativeLibraryFacade.mdb_env_stat(IntPtr env, out MDBStat stat)
        {
            return _mdb_env_stat(env, out stat);
        }

        int INativeLibraryFacade.mdb_env_sync(IntPtr env, bool force)
        {
            return _mdb_env_sync(env, force);
        }

        int INativeLibraryFacade.mdb_get(IntPtr txn, uint dbi, ref ValueStructure key, out ValueStructure data)
        {
            return _mdb_get(txn, dbi, ref key, out data);
        }

        int INativeLibraryFacade.mdb_put(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data, PutOptions flags)
        {
            return _mdb_put(txn, dbi, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, ref ValueStructure data)
        {
            return _mdb_del(txn, dbi, ref key, ref data);
        }

        int INativeLibraryFacade.mdb_del(IntPtr txn, uint dbi, ref ValueStructure key, IntPtr data)
        {
            return __mdb_del(txn, dbi, ref key, data);
        }

        int INativeLibraryFacade.mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor)
        {
            return _mdb_cursor_open(txn, dbi, out cursor);
        }

        void INativeLibraryFacade.mdb_cursor_close(IntPtr cursor)
        {
            _mdb_cursor_close(cursor);
        }

        int INativeLibraryFacade.mdb_cursor_renew(IntPtr txn, IntPtr cursor)
        {
            return _mdb_cursor_renew(txn, cursor);
        }

        int INativeLibraryFacade.mdb_cursor_get(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorOperation op)
        {
            return _mdb_cursor_get(cursor, ref key, ref data, op);
        }

        int INativeLibraryFacade.mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ref ValueStructure data, CursorPutOptions flags)
        {
            return _mdb_cursor_put(cursor, ref key, ref data, flags);
        }

        int INativeLibraryFacade.mdb_cursor_put(IntPtr cursor, ref ValueStructure key, ValueStructure[] data, CursorPutOptions flags)
        {
            return __mdb_cursor_put(cursor, ref key, data, flags);
        }

        int INativeLibraryFacade.mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags)
        {
            return _mdb_cursor_del(cursor, flags);
        }

        int INativeLibraryFacade.mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return _mdb_set_compare(txn, dbi, cmp);
        }

        int INativeLibraryFacade.mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp)
        {
            return _mdb_set_dupsort(txn, dbi, cmp);
        }
    }
#endif
}
