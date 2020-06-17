using System;
using System.Runtime.CompilerServices;

namespace LightningDB.Native
{
    public unsafe struct MDBValue
    {
        internal MDBValue(Span<byte> span)
        {
            size = (IntPtr) span.Length;
            data = (byte*)Unsafe.AsPointer(ref span.GetPinnableReference());
        }
        
        internal MDBValue(Span<byte> span, int size)
        {
            this.size = (IntPtr) size;
            data = (byte*)Unsafe.AsPointer(ref span.GetPinnableReference());
        }
        
        internal IntPtr size;
        internal byte* data;
        public ReadOnlySpan<byte> Span => new Span<byte>(data, (int)size);
    }
}
