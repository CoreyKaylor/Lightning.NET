using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    struct MDBStat
    {
        public uint ms_psize;
        public uint ms_depth;
        public IntPtr ms_branch_pages;
        public IntPtr ms_leaf_pages;
        public IntPtr ms_overflow_pages;
        public IntPtr ms_entries;
    }
}
