using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int CompareFunction(
        IntPtr left,
        IntPtr right);
}
