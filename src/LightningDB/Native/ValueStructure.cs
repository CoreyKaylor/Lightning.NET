using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValueStructure
    {
        public IntPtr size;

        public IntPtr data;
    }
}
