using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LightningDB.Converters
{
    /// <summary>
    /// Encapsulates collections to store converters used by LightningEnvironment.
    /// </summary>
    public class ConverterStore
    {
        private readonly IDictionary<Type, object> _convertToBytes = new ConcurrentDictionary<Type, object>(); 
        private readonly IDictionary<Type, object> _convertFromBytes = new ConcurrentDictionary<Type, object>(); 

        public void AddConvertToBytes<TConvertFrom>(IConvertToBytes<TConvertFrom> converter)
        {
            _convertToBytes.Add(converter.ConvertFromType, converter);
        }

        public void AddConvertFromBytes<TConvertTo>(IConvertFromBytes<TConvertTo> converter)
        {
            _convertFromBytes.Add(converter.ConvertFromType, converter);
        }

        public IConvertToBytes<TConvertFrom> GetToBytes<TConvertFrom>()
        {
            return (IConvertToBytes<TConvertFrom>) GetToBytes(typeof(TConvertFrom));
        }

        public IConvertFromBytes<TConvertTo> GetFromBytes<TConvertTo>()
        {
            return (IConvertFromBytes<TConvertTo>) GetFromBytes(typeof(TConvertTo));
        }

        public object GetToBytes(Type fromType)
        {
            if (!_convertToBytes.ContainsKey(fromType))
                throw new ConverterNotFoundException(fromType);

            return _convertToBytes[fromType];
        }

        public object GetFromBytes(Type toType)
        {
            if (!_convertToBytes.ContainsKey(toType))
                throw new ConverterNotFoundException(toType);

            return _convertFromBytes[toType];
        }
    }
}