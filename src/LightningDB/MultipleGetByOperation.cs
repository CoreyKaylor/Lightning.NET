using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace LightningDB
{
    /// <summary>
    /// Class to convert values obtained from database.
    /// </summary>
    public class MultipleGetByOperation
    {
        private readonly LightningDatabase _db;
        private readonly byte[] _rawValue;

        /// <summary>
        /// Creates new instance of MultipleGetByOperation
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="rawValue">Value bytes.</param>
        public MultipleGetByOperation(LightningDatabase db, byte[] rawValue)
        {
            _db = db;
            _rawValue = rawValue;
        }

        /// <summary>
        /// Convert values from bytes to a specified type..
        /// </summary>
        /// <typeparam name="TValue">Target value type.</typeparam>
        /// <returns>Converted value.</returns>
        public TValue[] Values<TValue>()
        {
            var itemSize = Marshal.SizeOf(typeof(TValue));
            if (_rawValue.Length % itemSize != 0)
                throw new InvalidOperationException("Returned " + _rawValue.Length + " bytes when expected N*" + itemSize + " bytes");

            var count = _rawValue.Length / itemSize;
            var result = new TValue[count];

            for (var i = 0; i < count; i++)
            {
                var itemBytes = new byte[itemSize];
                Array.Copy(_rawValue, i * itemSize, itemBytes, 0, itemSize);

                result[i] = _db.FromBytes<TValue>(itemBytes);
            }

            return result;
        }
    }    
}