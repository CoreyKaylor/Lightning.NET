using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Collections
{
    class CursorGenericEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private CursorEnumerator _enumerator;

        public CursorGenericEnumerator(LightningCursor cur)
        {
            _enumerator = new CursorEnumerator(cur);
        }

        #region IEnumerator<KeyValuePair<TKey,TValue>> Members

        public KeyValuePair<TKey, TValue> Current { get { return _enumerator.Current.Pair<TKey, TValue>(); } }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current { get { return this.Current;  } }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        #endregion
    }
}
