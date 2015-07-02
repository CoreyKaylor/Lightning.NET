using System.Collections;
using System.Collections.Generic;

namespace LightningDB
{
    public class LightningCursorMultiple : IEnumerator<KeyValuePair<byte[], byte[][]>>
    {
        private readonly LightningCursor _cursor;

        public LightningCursorMultiple(LightningCursor cursor)
        {
            _cursor = cursor;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
            _cursor.Reset();
        }

        public KeyValuePair<byte[], byte[][]> Current { get; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}