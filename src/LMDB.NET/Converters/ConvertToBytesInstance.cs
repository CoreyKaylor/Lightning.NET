using System;

namespace LMDB.Converters
{
    /// <summary>
    /// Class for conversion using a specified lambda from .NET type to byte array.
    /// </summary>
    /// <typeparam name="TFrom">Source type to convert from.</typeparam>
    public class ConvertToBytesInstance<TFrom> : IConvertToBytes<TFrom>
    {
        private readonly Func<LightningDatabase, TFrom, byte[]> _convert;

        /// <summary>
        /// Creates a new instance of ConvertToBytesInstance.
        /// </summary>
        /// <param name="convert">Conversion lambda</param>
        public ConvertToBytesInstance(Func<LightningDatabase, TFrom, byte[]> convert)
        {
            _convert = convert;
        }

        /// <summary>
        /// Source type.
        /// </summary>
        public Type ConvertFromType { get { return typeof(TFrom); } }

        /// <summary>
        /// Converts from source type to a byte array.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="instance">Source value.</param>
        /// <returns>Value converted to a byte array.</returns>
        public byte[] Convert(LightningDatabase db, TFrom instance)
        {
            return _convert(db, instance);
        }
    }
}