namespace LightningDB
{
    //TODO Is this interface buying us anything?
    public interface IPutter : IDatabaseAttributesProvider
    {
        void Put(byte[] key, byte[] value, PutOptions options);
    }
}
