using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Factories
{
    struct DatabaseHandleCacheEntry
    {
        private uint _handle;
        private DatabaseOpenFlags _flags;

        public DatabaseHandleCacheEntry(uint handle, DatabaseOpenFlags flags)
        {
            _handle = handle;
            _flags = flags;
        }

        public uint Handle { get { return _handle; } }

        public DatabaseOpenFlags OpenFlags { get { return _flags; } }
    }
}
