using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public class LightningException : Exception
    {
        private static string GetMessageByCode(int code)
        {
            var ptr = Native.mdb_strerror(code);
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal LightningException(int code)
            : base (GetMessageByCode(code))
        { }
    }
}
