using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB
{
    public static class LightningCursorMoveExtensions
    {
        
        public static CursorGetByOperation MoveToFirstBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MoveToFirst);
        }

        public static bool MoveToFirst<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MoveToFirst, out pair);
        }
        
        public static CursorGetByOperation MoveToLastBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MoveToLast);
        }

        public static bool MoveToLast<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MoveToLast, out pair);
        }
        
        public static CursorGetByOperation GetCurrentBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.GetCurrent);
        }

        public static bool GetCurrent<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.GetCurrent, out pair);
        }
        
        public static CursorGetByOperation MoveNextBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MoveNext);
        }

        public static bool MoveNext<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MoveNext, out pair);
        }
        
        public static CursorGetByOperation MoveNextDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MoveNextDuplicate);
        }

        public static bool MoveNextDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MoveNextDuplicate, out pair);
        }
        
        public static CursorGetByOperation MoveNextNoDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MoveNextNoDuplicate);
        }

        public static bool MoveNextNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MoveNextNoDuplicate, out pair);
        }
        
        public static CursorGetByOperation MovePrevBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MovePrev);
        }

        public static bool MovePrev<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MovePrev, out pair);
        }
        
        public static CursorGetByOperation MovePrevDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MovePrevDuplicate);
        }

        public static bool MovePrevDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MovePrevDuplicate, out pair);
        }
        
        public static CursorGetByOperation MovePrevNoDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveBy(cur.MovePrevNoDuplicate);
        }

        public static bool MovePrevNoDuplicate<TKey, TValue>(this LightningCursor cur, out KeyValuePair<TKey, TValue> pair)
        {
            return cur.CursorMove<TKey, TValue>(cur.MovePrevNoDuplicate, out pair);
        }
                
        public static GetByOperation MoveToFirstDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveValueBy(cur.MoveToFirstDuplicate);
        }

        public static bool MoveToFirstDuplicate<TValue>(this LightningCursor cur, out TValue value)
        {
            return cur.CursorMoveValue<TValue>(cur.MoveToFirstDuplicate, out value);
        }
        
        public static GetByOperation MoveToLastDuplicateBy(this LightningCursor cur)
        {
            return cur.CursorMoveValueBy(cur.MoveToLastDuplicate);
        }

        public static bool MoveToLastDuplicate<TValue>(this LightningCursor cur, out TValue value)
        {
            return cur.CursorMoveValue<TValue>(cur.MoveToLastDuplicate, out value);
        }
            
    }
}
