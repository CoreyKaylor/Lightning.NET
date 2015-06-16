using System;
using System.Runtime.InteropServices;

namespace LightningDB.Converters
{
    /// <summary>
    /// Extension methods for converters and ConverterStore
    /// </summary>
    public static class ConverterExtensions
    {
        /// <summary>
        /// Register a converter into bytes.
        /// </summary>
        /// <typeparam name="TFrom">Converter's source convertion type</typeparam>
        /// <param name="store">Target sotre.</param>
        /// <param name="convert">Convertion lambda</param>
        public static void AddConvertToBytes<TFrom>(this ConverterStore store, Func<LightningDatabase, TFrom, byte[]> convert)
        {
            var converter = new ConvertToBytesInstance<TFrom>(convert);
            store.AddConvertToBytes(converter);
        }
        
        /// <summary>
        /// Register a converter from bytes
        /// </summary>
        /// <typeparam name="TTo">Converter's target convertion type</typeparam>
        /// <param name="store">Target store.</param>
        /// <param name="convert">Convertion lambda.</param>
        public static void AddConvertFromBytes<TTo>(this ConverterStore store, Func<LightningDatabase, byte[], TTo> convert)
        {
            var converter = new ConvertFromBytesInstance<TTo>(convert);
            store.AddConvertFromBytes(converter);
        }

        /// <summary>
        /// Return a convertion delegate that first ensures if a size in bytes is correct.
        /// </summary>
        /// <typeparam name="TTo">Convertion target type.</typeparam>
        /// <param name="convert">Basic convertion lambda without size ensurance.</param>
        /// <param name="size">Explicit size or autodetect if null.</param>
        /// <returns>A convertion delegate that first ensures if a size in bytes is correct</returns>
        public static Func<LightningDatabase, byte[], TTo> EnsureCorrectSize<TTo>(this Func<LightningDatabase, byte[], TTo> convert, int? size = null) 
            where TTo : struct
        {
            return (db, x) =>
            {
                var actualSize = size ?? Marshal.SizeOf(typeof(TTo));
                if (x.Length != actualSize)
                {
                    var message = string.Format("Invalid byte count. {0} given {1} required.", x.Length, actualSize);
                    throw new InvalidCastException(message);
                }

                return convert(db, x);
            };
        }
    }
}