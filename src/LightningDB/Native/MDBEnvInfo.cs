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
        /// <summary>
        /// Address of map, if fixed
        /// </summary>
        public IntPtr me_mapaddr;

        /// <summary>
        /// Size of the data memory map
        /// </summary>
        public IntPtr me_mapsize;

        /// <summary>
        /// ID of the last used page
        /// </summary>
        public IntPtr me_last_pgno;

        /// <summary>
        /// ID of the last committed transaction
        /// </summary>
        public IntPtr me_last_txnid;

        /// <summary>
        /// max reader slots in the environment
        /// </summary>
        public uint me_maxreaders;

        /// <summary>
        /// max reader slots used in the environment
        /// </summary>
        public uint me_numreaders;
    }
}
