using System;

namespace LightningDB.Converters
{
    public interface IConvertToBytes<TConvertFrom>
    {
        Type ConvertFromType { get; }
        byte[] Convert(LightningDatabase db, TConvertFrom instance);
    }
}