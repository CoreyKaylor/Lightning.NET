using System;
using System.Text;

namespace LightningDB
{
    /// <summary>
    /// Basic setup for the library.
    /// </summary>
    public static class LightningConfig
    {
        /// <summary>
        /// Basic environment configuration
        /// </summary>
        public static class Environment
        {
            /// <summary>
            /// Default MapSize for new environments
            /// </summary>
            public const long LibDefaultMapSize = 1048576;

            /// <summary>
            /// Default MaxReaders for new environments
            /// </summary>
            public const int LibDefaultMaxReaders = 126;

            /// <summary>
            /// Default MaxDatabases for new envitonments
            /// </summary>
            public const int LibDefaultMaxDatabases = 0;

            static Environment()
            {
                AutoReduceMapSizeIn32BitProcess = false;

                DefaultMapSize = LibDefaultMapSize;
                DefaultMaxReaders = LibDefaultMaxReaders;
                DefaultMaxDatabases = LibDefaultMaxDatabases;
            }

            /// <summary>
            /// Default map size for new environments
            /// </summary>
            public static long DefaultMapSize { get; set; }

            /// <summary>
            /// Default MaxReaders for new environments
            /// </summary>
            public static int DefaultMaxReaders { get; set; }

            /// <summary>
            /// Default MaxDatabases for new environments
            /// </summary>
            public static int DefaultMaxDatabases { get; set; }

            /// <summary>
            /// Automatically reduce MapSize to a value allowed by running process's bitness. Default false.
            /// </summary>
            public static bool AutoReduceMapSizeIn32BitProcess { get; set; }
        }

        /// <summary>
        /// Basic environment conviguration
        /// </summary>
        public static class Database
        {
            static Database()
            {
                DefaultOpenFlags = DatabaseOpenFlags.None;
            }

            /// <summary>
            /// Default database open flags
            /// </summary>
            public static DatabaseOpenFlags DefaultOpenFlags { get; set; }
        }
    }
}
