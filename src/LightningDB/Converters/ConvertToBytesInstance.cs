using System;

namespace LightningDB.Converters
{
    public class ConvertToBytesInstance<TFrom> : IConvertToBytes<TFrom>
    {
        private readonly Func<LightningDatabase, TFrom, byte[]> _convert;

        public ConvertToBytesInstance(Func<LightningDatabase, TFrom, byte[]> convert)
        {
            _convert = convert;
        }

        public Type ConvertFromType
        {
            get { return typeof(TFrom); }
        }

        public byte[] Convert(LightningDatabase db, TFrom instance)
        {
            return _convert(db, instance);
        }
    }
}