﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using LightningDB.Native;
using static LightningDB.Native.NativeMethods;

namespace LightningDB.Factories
{
    class DatabaseManager
    {
        private readonly IntPtr _environmentHandle;

        private readonly Object _syncObject;
        
        private readonly ConcurrentDictionary<string, DatabaseHandleCacheEntry> _openedDatabases;
        private readonly HashSet<uint> _databasesForReuse;

        public DatabaseManager(IntPtr environmentHandle)
        {
            _environmentHandle = environmentHandle;

            _syncObject = new Object();

            _openedDatabases = new ConcurrentDictionary<string, DatabaseHandleCacheEntry>();
            _databasesForReuse = new HashSet<uint>();
        }

        private static string ToInternalDatabaseName(string name)
        {
            return name ?? LightningDatabase.DefaultDatabaseName;
        }

        private static string FromInternalDatabaseName(string name)
        {
            if (LightningDatabase.DefaultDatabaseName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return null;

            return name;
        }

        private DatabaseHandleCacheEntry OpenDatabaseHandle(string name, LightningTransaction tran, DatabaseOpenFlags flags)
        {
            name = FromInternalDatabaseName(name);

            uint handle;
            mdb_dbi_open(tran._handle, name, flags, out handle);

            return new DatabaseHandleCacheEntry(handle, flags);
        }               

        public LightningDatabase OpenDatabase(string name, LightningTransaction tran, DatabaseOpenFlags flags, Encoding encoding)
        {
            var internalName = ToInternalDatabaseName(name);

            var cacheEntry = _openedDatabases.AddOrUpdate(
                internalName,
                key =>
                {
                    var entry = OpenDatabaseHandle(name, tran, flags);                    

                    return entry;
                },
                (key, entry) =>
                {
                    if (entry.OpenFlags != flags)
                        entry = OpenDatabaseHandle(name, tran, flags);

                    return entry;
                });

            _databasesForReuse.Add(cacheEntry.Handle);

            return new LightningDatabase(internalName, tran, cacheEntry, encoding);
        }

        public bool IsOpen(LightningDatabase db)
        {
            DatabaseHandleCacheEntry entry;
            return _openedDatabases.TryGetValue(db.Name, out entry) && entry.Handle == db._handle;
        }

        public bool IsReleased(LightningDatabase db)
        {
            return !_databasesForReuse.Contains(db._handle);
        }

        public void Close(LightningDatabase db, bool releaseHandle)
        {
            lock (_syncObject)
            {
                if (IsReleased(db))
                    return;

                Reuse(db);

                if (!releaseHandle)
                    return;

                try
                {
                    mdb_dbi_close(_environmentHandle, db._handle);
                }
                finally
                {
                    Release(db);
                }
            }
        }


        public void CloseAll()
        {
            foreach (var hdb in _databasesForReuse)
                mdb_dbi_close(_environmentHandle, hdb);
        }

        private void Reuse(LightningDatabase db)
        {
            DatabaseHandleCacheEntry entry;
            _openedDatabases.TryRemove(db.Name, out entry);
        }

        private void Release(LightningDatabase db)
        {
            _databasesForReuse.Remove(db._handle);
        }
    }
}
