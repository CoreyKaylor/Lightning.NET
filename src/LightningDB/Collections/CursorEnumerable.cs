using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Collections
{
    class CursorGenericEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private LightningTransaction _tx;
        private LightningDatabase _db;

        public CursorGenericEnumerable(LightningTransaction tx, LightningDatabase db)
        {
            if (tx == null)
                throw new ArgumentNullException("tx");

            if (db == null)
                throw new ArgumentNullException("db");

            _tx = tx;
            _db = db;
        }

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var cur = _tx.CreateCursor(_db);

            return new CursorGenericEnumerator<TKey, TValue>(cur);
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
