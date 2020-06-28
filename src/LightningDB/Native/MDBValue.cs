using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    /// <summary>
    /// A managed version of the native MDB_val type
    /// </summary>
    /// <remarks>
    /// For Performance and Correctness, the layout of this struct must not be changed. 
    /// This struct is blittable and is marshalled directly to Native code via
    /// P/Invoke.
    /// </remarks>
    public unsafe struct MDBValue
    {
#warning add details fixed performance vs GCHandle pinning
        /// <remarks>
        /// We only expose this shape constructor to basically force you to use
        /// a fixed statment to obtain the pointer. If we accepted a Span or 
        /// ReadOnlySpan here, we would have to do scarier things to pin/unpin
        /// the buffer. Since this library is geared towards safe and easy usage,
        /// this way somewhat forces you onto the correct path.
        /// 
        /// We could also use GCHandle.Alloc to pin
        /// </remarks>
        /// <param name="data">A pointer to a buffer. This buffer must be pinned or allocated from 
        /// the stack to avoid it being moved by the GC during a collection</param>
        internal MDBValue(int bufferSize, byte* pinnedOrStackAllocBuffer)
        {
            this.size = (IntPtr)bufferSize;
            this.data = pinnedOrStackAllocBuffer;
        }

        internal IntPtr size;
        internal byte* data;

        public ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(data, checked((int)size));

        /// <summary>
        /// Convinence method to make intent more clear
        /// </summary>
        /// <returns>A newly allocated array containing data copied from the dereferenced data pointer</returns>
        public byte[] CopyToNewArray() => AsSpan().ToArray();

        public bool TryCopyTo(Span<byte> destinationBuffer) => AsSpan().TryCopyTo(destinationBuffer);
    }
}
