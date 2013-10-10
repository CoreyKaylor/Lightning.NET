using System;
using System.Runtime.InteropServices;

namespace LightningDB
{
    public class LightningException : Exception
    {
        private static string GetMessageByCode(int code)
        {
            var ptr = Native.Library.mdb_strerror(code);
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal LightningException(int code)
            : base (GetMessageByCode(code))
        { }
    }
}
