using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    static class Platform
    {
        public static bool Is64Bit { get { return IntPtr.Size == 8; } }
    }
}
