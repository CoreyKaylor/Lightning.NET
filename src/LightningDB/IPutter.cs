namespace LightningDB
{
    public interface IPutter : IDatabaseAttributesProvider
    {
        void Put(byte[] key, byte[] value, PutOptions options);
    }
}
