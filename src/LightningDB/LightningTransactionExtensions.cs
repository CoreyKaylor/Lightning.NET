using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Collections;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Extension methods for LightningTransaction objects.
    /// </summary>
    public static class LightningTransactionExtensions
    {
        /// <summary>
        /// Get value from a database.
        /// </summary>
        /// <param name="txn">A transaction</param>
        /// <param name="key">Key byte array.</param>
        /// <returns>Requested value's byte array if exists, or null if not.</returns>
        public static byte[] Get(this LightningTransaction txn, byte[] key)
        {
            return txn.Get(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Tries to get a value by its key.
        /// </summary>
        /// <param name="txn">A transaction</param>
        /// <param name="key">Key byte array.</param>
        /// <param name="value">Value byte array if exists.</param>
        /// <returns>True if key exists, false if not.</returns>
        public static bool TryGet(this LightningTransaction txn, byte[] key, out byte[] value)
        {
            return txn.TryGet(txn.OpenDatabase(), key, out value);
        }

        /// <summary>
        /// Check whether data exists in database.
        /// </summary>
        /// <param name="txn">A transaction</param>
        /// <param name="key">Key.</param>
        /// <returns>True if key exists, false if not.</returns>
        public static bool ContainsKey(this LightningTransaction txn, byte[] key)
        {
            return txn.ContainsKey(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Put data into a database.
        /// </summary>
        /// <param name="txn">A transaction</param>
        /// <param name="key">Key byte array.</param>
        /// <param name="value">Value byte array.</param>
        /// <param name="options">Operation options (optional).</param>
        public static void Put(this LightningTransaction txn, byte[] key, byte[] value, PutOptions options = PutOptions.None)
        {
            txn.Put(txn.OpenDatabase(), key, value, options);
        }

        /// <summary>
        /// Delete items from a database.
        /// This function removes key/data pairs from the database. 
        /// If the database does not support sorted duplicate data items (MDB_DUPSORT) the data parameter is ignored. 
        /// If the database supports sorted duplicates and the data parameter is NULL, all of the duplicate data items for the key will be deleted. 
        /// Otherwise, if the data parameter is non-NULL only the matching data item will be deleted. 
        /// This function will return MDB_NOTFOUND if the specified key/data pair is not in the database.
        /// </summary>
        /// <param name="txn">A transaction</param>
        /// <param name="key">The key to delete from the database</param>
        /// <param name="value">The data to delete (optional)</param>
        public static void Delete(this LightningTransaction txn, byte[] key, byte[] value = null)
        {
            txn.Delete(txn.OpenDatabase(), key, value);
        }

        /// <summary>
        /// Gets a value by its key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value converter or null if not exists.</returns>
        public static GetByOperation GetBy<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var valueBytes = txn.GetRawValue(db, key);
            return new GetByOperation(db, valueBytes);
        }

        /// <summary>
        /// Gets a value by its key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value converter or null if not exists.</returns>
        public static GetByOperation GetBy<TKey>(this LightningTransaction txn, TKey key)
        {
            return txn.GetBy(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Check whether data stored in databse.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>True if key exists, false, if not.</returns>
        public static bool ContainsKey<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            return txn.ContainsKey(db, keyBytes);
        }

        /// <summary>
        /// Check whether data stored in databse.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <returns>True if key exists, false, if not.</returns>
        public static bool ContainsKey<TKey>(this LightningTransaction txn, TKey key)
        {
            return txn.ContainsKey(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Tries obtaining a value by key
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <param name="txn">A transaction.</param>
        /// <param name="db">A database.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Obtained value converter if successful.</param>
        /// <returns>Returns true if key-value pair exists in database or false if not.</returns>
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

        /// <summary>
        /// Tries obtaining a value by key
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <param name="txn">A transaction.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Obtained value converter if successful.</param>
        /// <returns>Returns true if key-value pair exists in database or false if not.</returns>
        public static bool TryGetBy<TKey>(this LightningTransaction txn, TKey key, out GetByOperation value)
        {
            return txn.TryGetBy(txn.OpenDatabase(), key, out value);
        }

        /// <summary>
        /// Tries obtaining a value by key, converting it to a concrete type
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <typeparam name="TValue">Type to convert value to.</typeparam>
        /// <param name="txn">A transaction.</param>
        /// <param name="db">A database.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Obtained and converted value if successful.</param>
        /// <returns>Returns true if key-value pair exists in database or false if not.</returns>
        public static bool TryGet<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, out TValue value)
        {
            GetByOperation operation;
            var result = txn.TryGetBy(db, key, out operation);

            value = result
                ? operation.Value<TValue>()
                : default(TValue);

            return result;
        }

        /// <summary>
        /// Tries obtaining a value by key, converting it to a concrete type
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <typeparam name="TValue">Type to convert value to.</typeparam>
        /// <param name="txn">A transaction.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Obtained and converted value if successful.</param>
        /// <returns>Returns true if key-value pair exists in database or false if not.</returns>
        public static bool TryGet<TKey, TValue>(this LightningTransaction txn, TKey key, out TValue value)
        {
            return txn.TryGet(txn.OpenDatabase(), key, out value);
        }
        
        /// <summary>
        /// Gets and converts a value by its key for key-value pairs of a single type.
        /// </summary>
        /// <typeparam name="TType">Key and value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value or default(TType) if not exists.</returns>
        public static TType Get<TType>(this LightningTransaction txn, LightningDatabase db, TType key)
        {
            return txn.Get<TType, TType>(db, key);
        }

        /// <summary>
        /// Gets and converts a value by its key for key-value pairs of a single type.
        /// </summary>
        /// <typeparam name="TType">Key and value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value or default(TType) if not exists.</returns>
        public static TType Get<TType>(this LightningTransaction txn, TType key)
        {
            return txn.Get(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Gets a value by its key as a byte array.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value as a byte array or null if not exists.</returns>
        public static byte[] GetRawValue<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            return txn.Get(db, keyBytes);
        }

        /// <summary>
        /// Gets a value by its key as a byte array.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value as a byte array or null if not exists.</returns>
        public static byte[] GetRawValue<TKey>(this LightningTransaction txn, TKey key)
        {
            return txn.GetRawValue(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Gets a value by its key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value or default(TValue) if not exists.</returns>
        public static TValue Get<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = txn.Get(db, keyBytes);
            return db.FromBytes<TValue>(valueBytes);
        }

        /// <summary>
        /// Gets a value by its key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value or default(TValue) if not exists.</returns>
        public static TValue Get<TKey, TValue>(this LightningTransaction txn, TKey key)
        {
            return txn.Get<TKey, TValue>(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Deletes data from database by key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        public static void Delete<TKey>(this LightningTransaction txn, LightningDatabase db, TKey key)
        {
            var keyBytes = db.ToBytes(key);
            txn.Delete(db, keyBytes);
        }

        /// <summary>
        /// Deletes data from database by key.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        public static void Delete<TKey>(this LightningTransaction txn, TKey key)
        {
            txn.Delete(txn.OpenDatabase(), key);
        }

        /// <summary>
        /// Deletes a value from a database for duplicated key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public static void Delete<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, TValue value)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            txn.Delete(db, keyBytes, valueBytes);
        }

        /// <summary>
        /// Deletes a value from a database for duplicated key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public static void Delete<TKey, TValue>(this LightningTransaction txn, TKey key, TValue value)
        {
            txn.Delete(txn.OpenDatabase(), key, value);
        }

        /// <summary>
        /// Put a key-value pair into a database.
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <typeparam name="TValue">Type of a value.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="options">Operation options.</param>
        public static void Put<TKey, TValue>(this LightningTransaction txn, LightningDatabase db, TKey key, TValue value, PutOptions options = PutOptions.None)
        {
            var keyBytes = db.ToBytes(key);
            var valueBytes = db.ToBytes(value);
            txn.Put(db, keyBytes, valueBytes, options);
        }

        /// <summary>
        /// Put a key-value pair into a database.
        /// </summary>
        /// <typeparam name="TKey">Type of a key.</typeparam>
        /// <typeparam name="TValue">Type of a value.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="options">Operation options.</param>
        public static void Put<TKey, TValue>(this LightningTransaction txn, TKey key, TValue value, PutOptions options = PutOptions.None)
        {
            txn.Put(txn.OpenDatabase(), key, value, options);
        }

        /// <summary>
        /// Gets a cursor to enumerate a whole database.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <returns>A database cursor wrapped into IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.</returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateDatabase<TKey, TValue>(this LightningTransaction txn, LightningDatabase db)
        {
            return new CursorGenericEnumerable<TKey, TValue>(txn, db);
        }

        /// <summary>
        /// Gets a cursor to enumerate a whole database.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="txn">Transaction.</param>
        /// <returns>A database cursor wrapped into IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.</returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateDatabase<TKey, TValue>(this LightningTransaction txn)
        {
            return txn.EnumerateDatabase<TKey, TValue>(txn.OpenDatabase());
        }

        /// <summary>
        /// Gets a cursor to enumerate a whole database not converting key-value pair during the enumeration.
        /// </summary>
        /// <param name="txn">Transaction.</param>
        /// <param name="db">Database.</param>
        /// <returns>A database cursor wrapped into. IEnumerable&lt;CursorGetByOperation&gt;.</returns>
        public static IEnumerable<CursorGetByOperation> EnumerateDatabase(this LightningTransaction txn, LightningDatabase db)
        {
            return new CursorEnumerable(txn, db);
        }

        /// <summary>
        /// Gets a cursor to enumerate a whole database not converting key-value pair during the enumeration.
        /// </summary>
        /// <param name="txn">Transaction.</param>
        /// <returns>A database cursor wrapped into. IEnumerable&lt;CursorGetByOperation&gt;.</returns>
        public static IEnumerable<CursorGetByOperation> EnumerateDatabase(this LightningTransaction txn)
        {
            return txn.EnumerateDatabase(txn.OpenDatabase());
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
