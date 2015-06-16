using System;

namespace LightningDB.Converters
{
    /// <summary>
    /// Class to setup default converters for LightningEnvironment
    /// </summary>
    public class DefaultConverters
    {
        /// <summary>
        /// Registers predefined list of converters.
        /// </summary>
        /// <param name="environment">Environment to register converters in.</param>
        public void RegisterDefault(LightningEnvironment environment)
        {
            var store = environment.ConverterStore;

            store.AddConvertToBytes<Guid>((db, x) => x.ToByteArray());
            store.AddConvertToBytes<double>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<float>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<bool>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<short>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<int>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<long>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<ushort>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<uint>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<ulong>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<char>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<byte>((db, x) => new [] { x });
            store.AddConvertToBytes<sbyte>((db, x) => new[] { (byte) x });
            store.AddConvertToBytes<string>((db, x) => db.Encoding.GetBytes(x));
            store.AddConvertToBytes<byte[]>((db, x) => x);
            
            ConvertFromBytesWithCorrectSize(store, (db, x) => new Guid(x), 16);
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToDouble(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToSingle(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToBoolean(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt16(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt32(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt64(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt16(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt32(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt64(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToChar(x, 0));
            ConvertFromBytesWithCorrectSize(store, (db, x) => x[0]);
            ConvertFromBytesWithCorrectSize(store, (db, x) => (sbyte)x[0]);            
            store.AddConvertFromBytes((db, x) => db.Encoding.GetString(x));
            store.AddConvertFromBytes((db, x) => x);
        }

        private void ConvertFromBytesWithCorrectSize<TTo>(ConverterStore store, Func<LightningDatabase, byte[], TTo> convert, int? size = null) where TTo : struct
        {
            var func = convert.EnsureCorrectSize(size);
            store.AddConvertFromBytes(func);
        }
    }
}