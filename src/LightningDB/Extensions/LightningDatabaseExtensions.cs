using System;
using System.Collections.Generic;

namespace LightningDB.Extensions
{
    public static class LightningDatabaseExtensions
    {
        private static readonly LightningCRUDIntKeyProvider _intProvider = new LightningCRUDIntKeyProvider();

        private static readonly LightningCRUDStringKeyProvider _stringProvider = new LightningCRUDStringKeyProvider();

        public static LightningCRUDProvider<int> IntKeyProvider { get { return _intProvider; } }

        public static LightningCRUDProvider<string> StringKeyProvider { get { return _stringProvider; } }

        public static TValue Get<TValue>(this LightningDatabase db, string key)
        {
            return _stringProvider.Get<TValue>(db, key);
        }

        public static TValue Get<TValue>(this LightningDatabase db, Int32 key)
        {
            return _intProvider.Get<TValue>(db, key);
        }

        public static void Delete<TValue>(this LightningDatabase db, string key, TValue value)
        {
            _stringProvider.Delete(db, key, value);
        }

        public static void Delete<TValue>(this LightningDatabase db, Int32 key, TValue value)
        {
            _intProvider.Delete(db, key, value);
        }

        public static void Delete<TValue>(this LightningDatabase db, string key)
        {
            _stringProvider.Delete(db, key);
        }

        public static void Delete<TValue>(this LightningDatabase db, Int32 key)
        {
            _intProvider.Delete(db, key);
        }

        public static void Put<TValue>(this IPutter db, string key, TValue value, PutOptions options = PutOptions.None)
        {
            _stringProvider.Put(db, key, value, options);
        }

        public static void Put<TValue>(this IPutter db, Int32 key, TValue value, PutOptions options = PutOptions.None)
        {
            _intProvider.Put(db, key, value, options);
        }

        public static KeyValuePair<Int32, TValue> GetWithIntKey<TValue>(LightningCursor cursor, CursorOperation operation)
        {
            return _intProvider.Get<TValue>(cursor, operation);
        }

        public static KeyValuePair<string, TValue> GetWithStringKey<TValue>(LightningCursor cursor, CursorOperation operation)
        {
            return _stringProvider.Get<TValue>(cursor, operation);
        }
    }
}
