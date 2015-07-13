namespace LightningDB
{
    public static class HelperExtensions
    {
        public static bool StartsWith(this byte[] source, byte[] prefix)
        {
            var length = prefix.Length;
            for (var i = 0; i < length; ++i)
            {
                if (source[i] == prefix[i])
                    continue;
                return false;
            }
            return length != 0;
        }
    }
}