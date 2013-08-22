using System;

namespace LightningDB.Converters
{
    public interface IConvertFromBytes<TTo>
    {
        Type ConvertFromType { get; }
        TTo Convert(LightningDatabase db, byte[] bytes);
    }
}