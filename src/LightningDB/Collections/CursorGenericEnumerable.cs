using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Collections
{
    class CursorEnumerable : IEnumerable<CursorGetByOperation>
    {
        private LightningTransaction _tx;
        private LightningDatabase _db;

        public CursorEnumerable(LightningTransaction tx, LightningDatabase db)
        {
            if (tx == null)
                throw new ArgumentNullException("tx");

            if (db == null)
                throw new ArgumentNullException("db");

            _tx = tx;
            _db = db;
        }

        #region IEnumerable<CursorGetByOperation> Members

        public IEnumerator<CursorGetByOperation> GetEnumerator()
        {
            var cur = _tx.CreateCursor(_db);

            return new CursorEnumerator(cur);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
