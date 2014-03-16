using System;

namespace LightningDB.Converters
{
    /// <summary>
    /// Class for converting from a byte array to a concrete type using specified lambda
    /// </summary>
    /// <typeparam name="TTo"></typeparam>
    public class ConvertFromBytesInstance<TTo> : IConvertFromBytes<TTo>
    {
        private readonly Func<LightningDatabase, byte[], TTo> _convert;

        /// <summary>
        /// Creates new instance of ConvertFromBytesInstance
        /// </summary>
        /// <param name="convert">Conversion lambda</param>
        public ConvertFromBytesInstance(Func<LightningDatabase, byte[], TTo> convert)
        {
            _convert = convert;
        }

        /// <summary>
        /// Destination type
        /// </summary>
        public Type ConvertFromType { get { return typeof(TTo); } }

        /// <summary>
        /// Converts a byte array to a destination type.
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Converted value.</returns>
        public TTo Convert(LightningDatabase db, byte[] bytes)
        {
            return bytes == null ? default(TTo) : _convert(db, bytes);
        }
    }
}