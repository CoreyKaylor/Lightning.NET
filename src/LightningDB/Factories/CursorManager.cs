using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightningDB.Native;

namespace LightningDB.Factories
{
    class CursorManager
    {
        private readonly LightningTransaction _transaction;
        private readonly ConcurrentDictionary<IntPtr, bool> _cursors;
        
        public CursorManager(LightningTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            _transaction = transaction;

            _cursors = new ConcurrentDictionary<IntPtr, bool>();
        }

        private IntPtr CreateCursorHandle(uint dbHandle)
        {
            var handle = default(IntPtr);
            NativeMethods.Execute(lib => lib.mdb_cursor_open(_transaction._handle, dbHandle, out handle));

            return handle;
        }

        private void CloseCursor(IntPtr cursorHandle)
        {
            NativeMethods.Library.mdb_cursor_close(cursorHandle);
        }

        public LightningCursor OpenCursor(LightningDatabase db)
        {
            var handle = CreateCursorHandle(db._handle);
            _cursors.TryAdd(handle, true);

            return new LightningCursor(db, _transaction, handle);
        }

        public void CloseCursor(LightningCursor cursor)
        {
            try
            {
                CloseCursor(cursor._handle);
            }
            finally
            {
                bool value;
                _cursors.TryRemove(cursor._handle, out value);
            }
        }

        public void CloseAll()
        {
            foreach (var p in _cursors)
                CloseCursor(p.Key);

            _cursors.Clear();
        }
    }
}
