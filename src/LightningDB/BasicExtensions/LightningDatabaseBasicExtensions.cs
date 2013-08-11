using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB.BasicExtensions
{
    public static class LightningDatabaseBasicExtensions
    {
        internal static byte[] GetKey(IDatabaseAttributesProvider db, string key)
        {
            if (db.OpenFlags.HasFlag(DatabaseOpenFlags.IntegerKey))
                throw new NotSupportedException("Database " + db.Name + " supports only integer keys");

            return db.Encoding.GetBytes(key);
        }

        internal static byte[] GetKey(IDatabaseAttributesProvider db, Int32 key)
        {
            if (!db.OpenFlags.HasFlag(DatabaseOpenFlags.IntegerKey))
                throw new NotSupportedException("Database " + db.Name + " supports only string keys");

            return BitConverter.GetBytes(key);
        }

        internal static string GetStringKey(IDatabaseAttributesProvider db, byte[] key)
        {
            if (db.OpenFlags.HasFlag(DatabaseOpenFlags.IntegerKey))
                throw new NotSupportedException("Database " + db.Name + " supports only integer keys");

            return db.Encoding.GetString(key);
        }

        internal static Int32 GetIntKey(IDatabaseAttributesProvider db, byte[] key)
        {
            if (!db.OpenFlags.HasFlag(DatabaseOpenFlags.IntegerKey))
                throw new NotSupportedException("Database " + db.Name + " supports only string keys");

            return BitConverter.ToInt32(key, 0);
        }

        public static byte[] Get(this LightningDatabase db, string key)
        {
            var bytes = GetKey(db, key);
            return db.Get(bytes);
        }

        public static byte[] Get(this LightningDatabase db, Int32 key)
        {
            var bytes = GetKey(db, key);
            return db.Get(bytes);
        }

        public static void Put(this IPutter db, string key, byte[] value, PutOptions options)
        {
            var bytes = GetKey(db, key);

            db.Put(bytes, value, options);
        }

        public static void Put(this IPutter db, Int32 key, byte[] value, PutOptions options)
        {
            var bytes = GetKey(db, key);

            db.Put(bytes, value, options);
        }

        public static void Delete(this LightningDatabase db, byte[] key)
        {
            db.Delete(key, null);
        }

        public static void Delete(this LightningDatabase db, string key)
        {
            var bytes = GetKey(db, key);
            db.Delete(bytes, null);
        }

        public static void Delete(this LightningDatabase db, string key, byte[] value)
        {
            var bytes = GetKey(db, key);
            db.Delete(bytes, value);
        }

        public static void Delete(this LightningDatabase db, Int32 key)
        {
            var bytes = GetKey(db, key);
            db.Delete(bytes, null);
        }

        public static void Delete(this LightningDatabase db, Int32 key, byte[] value)
        {
            var bytes = GetKey(db, key);
            db.Delete(bytes, value);
        }

        internal static byte[] ToByteArray(this ValueStructure valueStructure, int resultCode)
        {
            if (resultCode == Native.MDB_NOTFOUND)
                return null;

            var buffer = new byte[valueStructure.size];
            Marshal.Copy(valueStructure.data, buffer, 0, valueStructure.size);

            return buffer;
        }
    }
}
