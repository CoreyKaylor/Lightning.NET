using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Extensions for LightningCursor's Move* methods
    /// </summary>
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

        /// <summary>
        /// Position at first key/data item
        /// </summary>
        /// <returns>First key/data item</returns>
        public static CursorGetByOperation MoveToFirstBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToFirst);
        }

        /// <summary>
        /// Position at first key/data item
        /// </summary>
        /// <returns>First key/data item</returns>
        public static bool MoveToFirst<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToFirst, out pair);
        }

        /// <summary>
        /// Position at first data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>First data item of current key. Only for MDB_DUPSORT</returns>
        public static CursorGetByOperation MoveToFirstDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToFirstDuplicate);
        }

        /// <summary>
        /// Position at first data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>First data item of current key. Only for MDB_DUPSORT</returns>
        public static bool MoveToFirstDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToFirstDuplicate, out pair);
        }

        /// <summary>
        /// Position at last key/data item
        /// </summary>
        /// <returns>Last key/data item</returns>
        public static CursorGetByOperation MoveToLastBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToLast);
        }

        /// <summary>
        /// Position at last key/data item
        /// </summary>
        /// <returns>Last key/data item</returns>
        public static bool MoveToLast<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToLast, out pair);
        }

        /// <summary>
        /// Position at last data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Last data item of current key</returns>
        public static CursorGetByOperation MoveToLastDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveToLastDuplicate);
        }

        /// <summary>
        /// Position at last data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Last data item of current key</returns>
        public static bool MoveToLastDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveToLastDuplicate, out pair);
        }

        /// <summary>
        /// Return key/data at current cursor position
        /// </summary>
        /// <returns>Key/data at current cursor position</returns>
        public static CursorGetByOperation GetCurrentBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.GetCurrent);
        }

        /// <summary>
        /// Return key/data at current cursor position
        /// </summary>
        /// <returns>Key/data at current cursor position</returns>
        public static bool GetCurrent<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.GetCurrent, out pair);
        }

        /// <summary>
        /// Position at next data item
        /// </summary>
        /// <returns>Next data item</returns>
        public static CursorGetByOperation MoveNextBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNext);
        }

        /// <summary>
        /// Position at next data item
        /// </summary>
        /// <returns>Next data item</returns>
        public static bool MoveNext<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNext, out pair);
        }

        /// <summary>
        /// Position at next data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Next data item of current key</returns>
        public static CursorGetByOperation MoveNextDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNextDuplicate);
        }

        /// <summary>
        /// Position at next data item of current key. Only for MDB_DUPSORT
        /// </summary>
        /// <returns>Next data item of current key</returns>
        public static bool MoveNextDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNextDuplicate, out pair);
        }

        /// <summary>
        /// Position at first data item of next key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>
        /// First data item of next key.
        /// </returns>
        public static CursorGetByOperation MoveNextNoDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MoveNextNoDuplicate);
        }

        /// <summary>
        /// Position at first data item of next key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>
        /// First data item of next key.
        /// </returns>
        public static bool MoveNextNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MoveNextNoDuplicate, out pair);
        }

        /// <summary>
        /// Position at previous data item.
        /// </summary>
        /// <returns>Previous data item.</returns>
        public static CursorGetByOperation MovePrevBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrev);
        }

        /// <summary>
        /// Position at previous data item.
        /// </summary>
        /// <returns>Previous data item.</returns>
        public static bool MovePrev<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrev, out pair);
        }

        /// <summary>
        /// Position at previous data item of current key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public static CursorGetByOperation MovePrevDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrevDuplicate);
        }

        /// <summary>
        /// Position at previous data item of current key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public static bool MovePrevDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrevDuplicate, out pair);
        }

        /// <summary>
        /// Position at last data item of previous key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public static CursorGetByOperation MovePrevNoDuplicateBy(this LightningCursor cur)
        {
            return CursorMoveBy(cur, cur.MovePrevNoDuplicate);
        }

        /// <summary>
        /// Position at last data item of previous key. Only for MDB_DUPSORT.
        /// </summary>
        /// <returns>Previous data item of current key.</returns>
        public static bool MovePrevNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return CursorMove<TKey, TValue>(cur, cur.MovePrevNoDuplicate, out pair);
        }
    }
}
