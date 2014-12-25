using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LMDB.Converters
{
    /// <summary>
    /// Encapsulates collections to store converters used by LightningEnvironment.
    /// </summary>
    public class ConverterStore
    {
        private readonly IDictionary<Type, object> _convertToBytes = new ConcurrentDictionary<Type, object>(); 
        private readonly IDictionary<Type, object> _convertFromBytes = new ConcurrentDictionary<Type, object>(); 

        /// <summary>
        /// Register a converter into bytes.
        /// </summary>
        /// <typeparam name="TConvertFrom">Converter's source convertion type</typeparam>
        /// <param name="converter">A converter</param>
        public void AddConvertToBytes<TConvertFrom>(IConvertToBytes<TConvertFrom> converter)
        {
            _convertToBytes.Add(converter.ConvertFromType, converter);
        }

        /// <summary>
        /// Register a converter from bytes
        /// </summary>
        /// <typeparam name="TConvertTo">Converter's target convertion type</typeparam>
        /// <param name="converter">A converter</param>
        public void AddConvertFromBytes<TConvertTo>(IConvertFromBytes<TConvertTo> converter)
        {
            _convertFromBytes.Add(converter.ConvertFromType, converter);
        }

        /// <summary>
        /// Gets a converter into bytes for the specified type
        /// </summary>
        /// <typeparam name="TConvertFrom">Type to convert from.</typeparam>
        /// <returns>Corresponsding converter.</returns>
        public IConvertToBytes<TConvertFrom> GetToBytes<TConvertFrom>()
        {
            return (IConvertToBytes<TConvertFrom>) GetToBytes(typeof(TConvertFrom));
        }

        /// <summary>
        /// Gets a converter from bytes into the specified type.
        /// </summary>
        /// <typeparam name="TConvertTo">Type to convert to.</typeparam>
        /// <returns>Corresponding converter.</returns>
        public IConvertFromBytes<TConvertTo> GetFromBytes<TConvertTo>()
        {
            return (IConvertFromBytes<TConvertTo>) GetFromBytes(typeof(TConvertTo));
        }

        /// <summary>
        /// Gets a converter into bytes for the specified type
        /// </summary>
        /// <param name="fromType">Type to convert from</param>
        /// <returns>Corresponsding converter</returns>
        public object GetToBytes(Type fromType)
        {
            if (!_convertToBytes.ContainsKey(fromType))
                throw new ConverterNotFoundException(fromType);

            return _convertToBytes[fromType];
        }

        /// <summary>
        /// Gets a converter from bytes into the specified type.
        /// </summary>
        /// <param name="toType">Type to convert to</param>
        /// <returns>Corresponding converter.</returns>
        public object GetFromBytes(Type toType)
        {
            if (!_convertToBytes.ContainsKey(toType))
                throw new ConverterNotFoundException(toType);

            return _convertFromBytes[toType];
        }
    }
}