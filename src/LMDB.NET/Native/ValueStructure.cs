using System;
using System.Runtime.InteropServices;

namespace LMDB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValueStructure
    {
        public IntPtr size;

        public IntPtr data; 
    }
}
