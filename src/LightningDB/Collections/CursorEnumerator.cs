using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Collections
{
    class CursorEnumerator : IEnumerator<CursorGetByOperation>
    {
        private LightningCursor _cur;

        public CursorEnumerator(LightningCursor cur)
        {
            _cur = cur;
        }

        #region IEnumerator<CursorGetByOperation> Members

        public CursorGetByOperation Current { get; private set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_cur != null)
                _cur.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current { get { return this.Current; } }

        public bool MoveNext()
        {
            var next = _cur.MoveNextBy();

            this.Current = next.PairExists
                ? next
                : null;

            return next.PairExists;
        }

        public void Reset()
        {
            _cur.Renew();
        }

        #endregion
    }
}
