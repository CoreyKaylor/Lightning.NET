using System;
using System.Runtime.InteropServices;

namespace LightningDB
{
    internal class MarshalValueStructure : IDisposable
    {
        public MarshalValueStructure(byte[] value)
        {
            ValueStructure = new ValueStructure
            {
                data = Marshal.AllocHGlobal(value.Length),
                size = value.Length
            };
            Marshal.Copy(value, 0, ValueStructure.data, value.Length);
        }

        public ValueStructure ValueStructure { get; private set; }

        public void Dispose()
        {
            Marshal.FreeHGlobal(ValueStructure.data);
        }
    }
}