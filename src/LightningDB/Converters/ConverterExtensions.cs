using System;
using System.Runtime.InteropServices;

namespace LightningDB.Converters
{
    public static class ConverterExtensions
    {
        public static void AddConvertToBytes<TFrom>(this ConverterStore store, Func<LightningDatabase, TFrom, byte[]> convert)
        {
            var converter = new ConvertToBytesInstance<TFrom>(convert);
            store.AddConvertToBytes(converter);
        }

        public static void AddConvertFromBytes<TTo>(this ConverterStore store, Func<LightningDatabase, byte[], TTo> convert)
        {
            var converter = new ConvertFromBytesInstance<TTo>(convert);
            store.AddConvertFromBytes(converter);
        }

        public static Func<LightningDatabase, byte[], TTo> EnsureCorrectSize<TTo>(this Func<LightningDatabase, byte[], TTo> convert, int? size = null) where TTo : struct
        {
            return (db, x) =>
            {
                var actualSize = size ?? Marshal.SizeOf(typeof(TTo));
                if (x.Length != actualSize)
                {
                    var message = String.Format("Invalid byte count. {0} given {1} required.", x.Length, actualSize);
                    throw new InvalidCastException(message);
                }

                return convert(db, x);
            };
        }
    }
}