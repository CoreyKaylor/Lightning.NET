using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal class MarshalValueStructure : IDisposable
    {
        private readonly byte[] _value;
        private GCHandle _handle;

        public MarshalValueStructure(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _value = value;
            _handle = GCHandle.Alloc(_value, GCHandleType.Pinned);
        }

        public ValueStructure ValueStructure => new ValueStructure
        {
            data = _handle.AddrOfPinnedObject(),
            size = new IntPtr(_value.Length)
        };

        public void Dispose()
        {
            _handle.Free();
            GC.SuppressFinalize(this);
        }

        ~MarshalValueStructure()
        {
            Dispose();
        }
    }
}
