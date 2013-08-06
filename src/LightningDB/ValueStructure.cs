using System;
using System.Runtime.InteropServices;

namespace LightningDB
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValueStructure
    {
        public int size;

        public IntPtr data; 
    }
}
