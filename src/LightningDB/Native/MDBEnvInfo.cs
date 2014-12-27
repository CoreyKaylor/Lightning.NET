using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    struct MDBEnvInfo
    {
        public IntPtr me_mapaddr;
        public IntPtr me_mapsize;
        public IntPtr me_last_pgno;
        public IntPtr me_last_txnid;
        public uint me_maxreaders;
        public uint me_numreaders;
    }
}
