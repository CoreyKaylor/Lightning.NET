using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB
{
    /// <summary>
    /// Extensions for LightningCursor
    /// </summary>
    public static class LightningCursorExtensions
    {
        /// <summary>
        /// Delete current key/data pair.
        /// This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="cur">A cursor.</param>
        /// <param name="removeAllDuplicateData">if true, delete all of the data items for the current key. This flag may only be specified if the database was opened with MDB_DUPSORT.</param>
        public static void Delete(this LightningCursor cur, bool removeAllDuplicateData = true)
        {
            cur.Delete(
                removeAllDuplicateData ? CursorDeleteOption.NoDuplicateData : CursorDeleteOption.None);
        }

        /// <summary>
        /// Store by cursor.
        /// This function stores key/data pairs into the database. 
        /// If the function fails for any reason, the state of the cursor will be unchanged. 
        /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
        /// </summary>
        /// <param name="cur">A cursor.</param>
        /// <param name="key">The key operated on.</param>
        /// <param name="value">The data operated on.</param>
        /// <param name="options">
        /// Options for this operation. This parameter must be set to 0 or one of the values described here. (optional)
        ///     CursorPutOptions.Current - overwrite the data of the key/data pair to which the cursor refers with the specified data item. The key parameter is ignored.
        ///     CursorPutOptions.NoDuplicateData - enter the new key/data pair only if it does not already appear in the database. This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST if the key/data pair already appears in the database.
        ///     CursorPutOptions.NoOverwrite - enter the new key/data pair only if the key does not already appear in the database. The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports duplicates (MDB_DUPSORT).
        ///     CursorPutOptions.ReserveSpace - reserve space for data of the given size, but don't copy the given data. Instead, return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data is being generated later.
        ///     CursorPutOptions.AppendData - append the given key/data pair to the end of the database. No key comparisons are performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading unsorted keys with this flag will cause data corruption.
        ///     CursorPutOptions.AppendDuplicateData - as above, but for sorted dup data.
        /// </param>
        public static void Put<TKey, TValue>(this LightningCursor cur, TKey key, TValue value, CursorPutOptions options = CursorPutOptions.None)
        {
            var keyBytes = cur.ToBytes(key);
            var valueBytes = cur.ToBytes(value);
            cur.Put(keyBytes, valueBytes, options);
        }

        /// <summary>
        /// Store by cursor.
        /// This function stores key/data pairs into the database. 
        /// If the function fails for any reason, the state of the cursor will be unchanged. 
        /// If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the newly inserted item.
        /// </summary>
        /// <param name="cur">A cursor.</param>
        /// <param name="key">The key operated on.</param>
        /// <param name="values">The data operated on.</param>
        public static void PutMultiple<TKey, TValue>(this LightningCursor cur, TKey key, TValue[] values)
        {
            var keyBytes = cur.ToBytes(key);
            var valueBytes = values.Select(v => cur.ToBytes(v)).ToArray();
            cur.PutMultiple(keyBytes, valueBytes);
        }

        public static MultipleGetByOperation GetMultipleBy<TValue>(this LightningCursor cur)
        {
            var bytes = cur.GetMultiple();
            if (bytes == null)
                return null;

            return new MultipleGetByOperation(cur.Database, bytes);
        }

        public static bool GetMultiple<TValue>(this LightningCursor cur, out TValue[] values)
        {
            var op = cur.GetMultipleBy<TValue>();
            if (op == null)
            {
                values = null;
                return false;
            }

            values = op.Values<TValue>();

            return true;
        }

        internal static byte[] ToBytes<T>(this LightningCursor cur, T instance)
        {
            return cur.Database.ToBytes(instance);
        }

        internal static T FromBytes<T>(this LightningCursor cur, byte[] bytes)
        {
            return cur.Database.FromBytes<T>(bytes);
        }

        internal static CursorGetByOperation CursorMoveBy(this LightningCursor cur, Func<KeyValuePair<byte[], byte[]>?> mover)
        {
            return new CursorGetByOperation(cur, mover.Invoke());
        }

        internal static GetByOperation CursorMoveValueBy(this LightningCursor cur, Func<byte[]> mover)
        {
            var value = mover.Invoke();
            if (value == null)
                return null;

            return new GetByOperation(cur.Database, value);
        }

        internal static bool CursorMove<TKey, TValue>(this LightningCursor cur, Func<KeyValuePair<byte[], byte[]>?> mover, out KeyValuePair<TKey, TValue> pair)
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

        internal static bool CursorMoveValue<TValue>(this LightningCursor cur, Func<byte[]> mover, out TValue value)
        {
            var op = CursorMoveValueBy(cur, mover);

            if (op == null)
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = op.Value<TValue>();
                return true;
            }
        }
    }
}
