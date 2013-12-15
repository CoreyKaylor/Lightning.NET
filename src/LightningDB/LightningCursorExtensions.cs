using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public static class LightningCursorExtensions
    {
        public static void Delete(this LightningCursor cur, bool removeAllDuplicateData = true)
        {
            cur.Delete(
                removeAllDuplicateData ? CursorDeleteOption.NoDuplicateData : CursorDeleteOption.None);
        }

        public static void Put<TKey, TValue>(this LightningCursor cur, TKey key, TValue value, PutOptions options = PutOptions.None)
        {
            var keyBytes = cur.ToBytes(key);
            var valueBytes = cur.ToBytes(value);
            cur.Put(keyBytes, valueBytes, options);
        }

        internal static byte[] ToBytes<T>(this LightningCursor cur, T instance)
        {
            return cur.Database.ToBytes(instance);
        }

        internal static T FromBytes<T>(this LightningCursor cur, byte[] bytes)
        {
            return cur.Database.FromBytes<T>(bytes);
        }
    }
}
