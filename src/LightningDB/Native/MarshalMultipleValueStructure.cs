using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal class MarshalMultipleValueStructure : IDisposable
    {
        private readonly byte[] _flattened;
        private readonly int _size;
        private readonly int _count;

        private GCHandle _handle;

        public MarshalMultipleValueStructure(byte[][] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            _size = GetSize(values);
            _count = GetCount(values);
            _flattened = values.SelectMany(x => x).ToArray();
            _handle = GCHandle.Alloc(_flattened, GCHandleType.Pinned);
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

        public ValueStructure[] ValueStructures
        {
            get
            {
                return new[]
                {
                    new ValueStructure
                    {
                        size = new IntPtr(_size),
                        data = _handle.AddrOfPinnedObject()
                    },
                    new ValueStructure
                    {
                        size = new IntPtr(_count)
                    }
                };
            }
        }

        public void Dispose()
        {
            _handle.Free();
            GC.SuppressFinalize(this);
        }

        ~MarshalMultipleValueStructure()
        {
            Dispose();
        }
    }
}
