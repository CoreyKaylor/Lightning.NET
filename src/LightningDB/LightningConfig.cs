using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public static class LightningConfig
    {
        public static class Environment
        {
            public const long LibDefaultMapSize = 10485760;
            public const int LibDefaultMaxReaders = 126;
            public const int LibDefaultMaxDatabases = 0;

            static Environment()
            {
                AutoReduceMapSizeIn32BitProcess = false;

                DefaultMapSize = LibDefaultMapSize;
                DefaultMaxReaders = LibDefaultMaxReaders;
                DefaultMaxDatabases = LibDefaultMaxDatabases;
            }

            public static long DefaultMapSize { get; set; }

            public static int DefaultMaxReaders { get; set; }

            public static int DefaultMaxDatabases { get; set; }

            public static bool AutoReduceMapSizeIn32BitProcess { get; set; }
        }
    }
}
