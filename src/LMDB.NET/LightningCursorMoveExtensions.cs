using System;
using System.Collections.Generic;

namespace LMDB
{
    public static class LightningCursorMoveExtensions
    {
        private static CursorGetByOperation CursorMoveBy(LightningCursor cur, Func<KeyValuePair<byte[], byte[]>?> mover)
        {
            return new CursorGetByOperation(cur, mover.Invoke());
        }

        private static bool CursorMove<TKey, TValue>(LightningCursor cur, Func<KeyValuePair<byte[], byte[]>?> mover, out KeyValuePair<TKey, TValue> pair)
        {
            var op = CursorMoveBy(cur, mover);

            if (!op.PairExists)
            {
                pair = default(KeyValuePair<TKey, TValue>);
                return false;
            }
            else
            {
                pair = op.Pair<TKey, TValue>();
                return true;
            }
        }
        
        public static CursorGetByOperation MoveToFirstBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToFirst);
        }

        public static bool MoveToFirst<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToFirst, out pair);
        }
        
        public static CursorGetByOperation MoveToFirstDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToFirstDuplicate);
        }

        public static bool MoveToFirstDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToFirstDuplicate, out pair);
        }
        
        public static CursorGetByOperation MoveToLastBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToLast);
        }

        public static bool MoveToLast<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToLast, out pair);
        }
        
        public static CursorGetByOperation MoveToLastDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToLastDuplicate);
        }

        public static bool MoveToLastDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToLastDuplicate, out pair);
        }
        
        public static CursorGetByOperation GetCurrentBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.GetCurrent);
        }

        public static bool GetCurrent<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.GetCurrent, out pair);
        }
        
        public static CursorGetByOperation MoveNextBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNext);
        }

        public static bool MoveNext<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNext, out pair);
        }
        
        public static CursorGetByOperation MoveNextDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNextDuplicate);
        }

        public static bool MoveNextDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNextDuplicate, out pair);
        }
        
        public static CursorGetByOperation MoveNextNoDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNextNoDuplicate);
        }

        public static bool MoveNextNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNextNoDuplicate, out pair);
        }
        
        public static CursorGetByOperation MovePrevBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrev);
        }

        public static bool MovePrev<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrev, out pair);
        }
        
        public static CursorGetByOperation MovePrevDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrevDuplicate);
        }

        public static bool MovePrevDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrevDuplicate, out pair);
        }
        
        public static CursorGetByOperation MovePrevNoDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrevNoDuplicate);
        }

        public static bool MovePrevNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrevNoDuplicate, out pair);
        }
            }
}
