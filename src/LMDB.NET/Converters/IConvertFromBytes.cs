using System;

namespace LMDB.Converters
{
    /// <summary>
    /// Interface for converters from a byte array to a .NET type
    /// </summary>
    /// <typeparam name="TTo">Type to convert to</typeparam>
    public interface IConvertFromBytes<TTo>
    {
        /// <summary>
        /// Destination type.
        /// </summary>
        Type ConvertFromType { get; }

        /// <summary>
        /// Convertes from a byte array to a .NET type
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="bytes">Value bytes</param>
        /// <returns>Converted value</returns>
        TTo Convert(LightningDatabase db, byte[] bytes);
    }
}