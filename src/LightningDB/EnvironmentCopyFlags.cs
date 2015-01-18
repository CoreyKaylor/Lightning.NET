using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public enum EnvironmentCopyFlags
    {
        None = 0,

        /// <summary>
        /// MDB_CP_COMPACT. Compacting copy: Omit free space from copy, and renumber all pages sequentially.
        /// </summary>
        Compact = 0x01
    }
}
