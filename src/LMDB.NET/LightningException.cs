using System;
using System.Runtime.InteropServices;
using LMDB.Native;

namespace LMDB
{
    /// <summary>
    /// An exception caused by lmdb operations.
    /// </summary>
    public class LightningException : Exception
    {
        private static string GetMessageByCode(int code)
        {
            var ptr = NativeMethods.Library.mdb_strerror(code);
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal LightningException(int code)
            : base (GetMessageByCode(code))
        { }
    }
}
