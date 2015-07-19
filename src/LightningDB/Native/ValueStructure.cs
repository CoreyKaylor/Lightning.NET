using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    public struct ValueStructure
    {
        public IntPtr size;

        public IntPtr data;

        public byte[] GetBytes()
        {
            var buffer = new byte[size.ToInt32()];
            Marshal.Copy(data, buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
