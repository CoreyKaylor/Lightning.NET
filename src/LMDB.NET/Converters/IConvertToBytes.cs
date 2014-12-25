using System;

namespace LMDB.Converters
{
    /// <summary>
    /// Interface for converter from .NET type to a byte array.
    /// </summary>
    /// <typeparam name="TConvertFrom">Type to convert from/</typeparam>
    public interface IConvertToBytes<TConvertFrom>
    {
        /// <summary>
        /// Source type
        /// </summary>
        Type ConvertFromType { get; }

        /// <summary>
        /// Converts value from .NET type to a byte array
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="instance">Value</param>
        /// <returns>Value as a byte array.</returns>
        byte[] Convert(LightningDatabase db, TConvertFrom instance);
    }
}