using System;
using System.Collections.Generic;
namespace LightningDB
{
    /// <summary>
    /// Class to convert values obtained from database.
    /// </summary>
    public class GetByOperation
    {
        private readonly LightningDatabase _db;
        private readonly byte[] _rawValue;

        /// <summary>
        /// Creates new instance of GetByOperation
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="rawValue">Value bytes.</param>
        public GetByOperation(LightningDatabase db, byte[] rawValue)
        {
            _db = db;
            _rawValue = rawValue;
        }

        /// <summary>
        /// Convert value from bytes to a specified type..
        /// </summary>
        /// <typeparam name="TValue">Target value type.</typeparam>
        /// <returns>Converted value.</returns>
        public TValue Value<TValue>()
        {
            return _db.FromBytes<TValue>(_rawValue);
        }
    }    
}