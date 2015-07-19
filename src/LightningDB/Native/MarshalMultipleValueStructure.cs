using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    public class MarshalMultipleValueStructure : IDisposable
    {
        private readonly byte[] _key;
        private readonly byte[] _flattened;
        private readonly int _size;
        private readonly int _count;

        private GCHandle _keyHandle;
        private GCHandle _valuesHandle;

        public MarshalMultipleValueStructure(byte[] key, byte[][] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            _size = GetSize(values);
            _count = GetCount(values);
            _flattened = values.SelectMany(x => x).ToArray();
            _valuesHandle = GCHandle.Alloc(_flattened, GCHandleType.Pinned);

            _key = key;
            _keyHandle = GCHandle.Alloc(_key, GCHandleType.Pinned);

            Values = new[]
            {
                new ValueStructure
                {
                    size = new IntPtr(_size),
                    data = _valuesHandle.AddrOfPinnedObject()
                },
                new ValueStructure
                {
                    size = new IntPtr(_count)
                }
            };

            Key = new ValueStructure
            {
                size = new IntPtr(_key.Length),
                data = _keyHandle.AddrOfPinnedObject()
            };
        }

        private int GetSize(byte[][] values)
        {
            if (values.Length == 0 || values[0] == null || values[0].Length == 0)
                return 0;

            return values[0].Length;
        }

        private int GetCount(byte[][] values)
        {
            return values.Length;
        }

        public ValueStructure Key;

        public ValueStructure[] Values;

        public void Dispose()
        {
            _keyHandle.Free();
            _valuesHandle.Free();
            GC.SuppressFinalize(this);
        }

        ~MarshalMultipleValueStructure()
        {
            Dispose();
        }
    }
}
