using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValueStructure
    {
        public int size;

        public IntPtr data; 
    }
}
