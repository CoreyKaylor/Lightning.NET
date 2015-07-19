using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    public class MarshalValueStructure : IDisposable
    {
        private readonly byte[] _key;
        private readonly byte[] _value;
        private GCHandle _keyHandle;
        private GCHandle _valueHandle;

        public MarshalValueStructure(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _key = key;
            _keyHandle = GCHandle.Alloc(_key, GCHandleType.Pinned);
            Key = new ValueStructure
            {
                size = new IntPtr(_key.Length),
                data = _keyHandle.AddrOfPinnedObject()
            };
        }

        public MarshalValueStructure(byte[] key, byte[] value) : this(key)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _value = value;
            _valueHandle = GCHandle.Alloc(_value, GCHandleType.Pinned);
            Value = new ValueStructure
            {
                size = new IntPtr(_value.Length),
                data = _valueHandle.AddrOfPinnedObject()
            };
        }

        public ValueStructure Key;

        public ValueStructure Value;

        public void Dispose()
        {
            _keyHandle.Free();
            if(_value != null)
                _valueHandle.Free();

            GC.SuppressFinalize(this);
        }

        ~MarshalValueStructure()
        {
            Dispose();
        }
    }
}
