using System;

namespace LightningDB.Native
{
    /// <summary>
    /// Information about the environment. 
    /// </summary>
    public struct MDBEnvInfo
    {
        /// <summary>
        /// Address of map, if fixed
        /// </summary>
        public nint me_mapaddr;

        /// <summary>
        /// Size of the data memory map
        /// </summary>
        public nint me_mapsize;

        /// <summary>
        /// ID of the last used page
        /// </summary>
        public nint me_last_pgno;

        /// <summary>
        /// ID of the last committed transaction
        /// </summary>
        public nint me_last_txnid;

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
