using System;

namespace LightningDB.Converters
{
    public class DefaultConverters
    {
        public void RegisterDefault(LightningEnvironment environment)
        {
            var store = environment.ConverterStore;
            store.AddConvertToBytes<int>((db, x) => BitConverter.GetBytes(x));
            store.AddConvertToBytes<string>((db, x) => db.Encoding.GetBytes(x));
            store.AddConvertToBytes<byte[]>((db, x) => x);
            
            convertFromBytesWithCorrectSize(store, (db, x) => new Guid(x), 16);
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToDouble(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToSingle(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToBoolean(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt16(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt32(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToInt64(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt16(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt32(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToUInt64(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => BitConverter.ToChar(x, 0));
            convertFromBytesWithCorrectSize(store, (db, x) => x[0]);
            convertFromBytesWithCorrectSize(store, (db, x) => (SByte)x[0]);
            store.AddConvertFromBytes((db, x) => db.Encoding.GetString(x));
        }

        private void convertFromBytesWithCorrectSize<TTo>(ConverterStore store, Func<LightningDatabase, byte[], TTo> convert, int? size = null) where TTo : struct
        {
            var func = convert.EnsureCorrectSize(size);
            store.AddConvertFromBytes(func);
        }
    }
}