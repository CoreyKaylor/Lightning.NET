using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB
{
    public static class LightningDatabaseExtensions
    {
        public static GetByOperation GetBy<TKey>(this LightningDatabase db, TKey key)
        {
            var valueBytes = db.GetRawValue(key);
            return new GetByOperation(db, valueBytes);
        }

        public static TType Get<TType>(this LightningDatabase db, TType key)
        {
            return db.Get<TType, TType>(key);
        }

        public static byte[] GetRawValue<TKey>(this LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            return db.Get(keyBytes);
        }

        public static TValue Get<TKey, TValue>(this LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.Get(keyBytes);
            return db.FromBytes<TValue>(valueBytes);
        }

        public static void Delete<TKey>(this LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            db.Delete(keyBytes);
        }

        public static void Delete<TKey, TValue>(this LightningDatabase db, TKey key, TValue value)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            db.Delete(keyBytes, valueBytes);
        }

        public static void Put<TKey, TValue>(this LightningDatabase db, TKey key, TValue value, PutOptions options = PutOptions.None)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            db.Put(keyBytes, valueBytes, options);
        }

        public static KeyValuePair<TKey, TValue> GetCurrent<TKey, TValue>(this LightningCursor cursor)
        {
            var pair = cursor.GetCurrent();

            var key = cursor.Database.FromBytes<TKey>(pair.Key);
            var value = cursor.Database.FromBytes<TValue>(pair.Value);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        internal static byte[] ToByteArray(this ValueStructure valueStructure, int resultCode)
        {
            if (resultCode == Native.MDB_NOTFOUND)
                return null;

            var buffer = new byte[valueStructure.size];
            Marshal.Copy(valueStructure.data, buffer, 0, valueStructure.size);

            return buffer;
        }

        internal static byte[] ToBytes<T>(this LightningDatabase db, T instance)
        {
            return db.Environment.ConverterStore
                .GetToBytes<T>().Convert(db, instance);
        }

        internal static T FromBytes<T>(this LightningDatabase db, byte[] bytes)
        {
            return db.Environment.ConverterStore
                .GetFromBytes<T>().Convert(db, bytes);
        }
    }
}
