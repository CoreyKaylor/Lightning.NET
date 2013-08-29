using System;

namespace LightningDB.Converters
{
    public class ConvertFromBytesInstance<TTo> : IConvertFromBytes<TTo>
    {
        private readonly Func<LightningDatabase, byte[], TTo> _convert;

        public ConvertFromBytesInstance(Func<LightningDatabase, byte[], TTo> convert)
        {
            _convert = convert;
        }

        public Type ConvertFromType
        {
            get { return typeof(TTo); }
        }

        public TTo Convert(LightningDatabase db, byte[] bytes)
        {
            return bytes == null ? default(TTo) : _convert(db, bytes);
        }
    }
}