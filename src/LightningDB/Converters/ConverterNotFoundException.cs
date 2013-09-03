using System;

namespace LightningDB.Converters
{
    public class ConverterNotFoundException : Exception
    {
        public ConverterNotFoundException(Type type) 
            : base(string.Format("Unable to find converter for {0}", type.FullName))
        {
        }
    }
}