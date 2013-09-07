using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB
{
    public static class LightningDatabaseExtensions
    {
        public static GetByOperation GetBy<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var valueBytes = txn.GetRawValue(db, key);
            return new GetByOperation(db, valueBytes);
        }

        public static bool ContainsKey<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            return txn.ContainsKey(db, keyBytes);
        }

        public static bool TryGetBy<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key, out GetByOperation value)
        {
            byte[] valueBytes;

            var keyBytes = db.ToBytes(key);
            var result = txn.TryGet(db, keyBytes, out valueBytes);

            value = result
                ? new GetByOperation(db, valueBytes)
                : null;

            return result;
        }

        public static bool TryGet<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, out TValue value)
        {
            GetByOperation operation;
            var result = txn.TryGetBy(db, key, out operation);

            value = result
                ? operation.Value<TValue>()
                : default(TValue);

            return result;
        }

        public static TType Get<TType>(this LightningTransaction txn, LightningDatabase db, TType key)
        {
            return txn.Get<TType, TType>(db, key);
        }

        public static byte[] GetRawValue<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            return txn.Get(db, keyBytes);
        }

        public static TValue Get<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = txn.Get(db, keyBytes);
            return db.FromBytes<TValue>(valueBytes);
        }

        public static void Delete<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            txn.Delete(db, keyBytes);
        }

        public static void Delete<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, TValue value)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            txn.Delete(db, keyBytes, valueBytes);
        }

        public static void Put<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, TValue value, PutOptions options = PutOptions.None)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            txn.Put(db, keyBytes, valueBytes, options);
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

            var buffer = new byte[valueStructure.size.ToInt32()];
            Marshal.Copy(valueStructure.data, buffer, 0, buffer.Length);

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
