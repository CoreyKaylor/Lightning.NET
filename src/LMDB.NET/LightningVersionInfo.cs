using System;
using System.Runtime.InteropServices;
using LMDB.Native;

namespace LMDB
{
    /// <summary>
    /// Represents lmdb version information.
    /// </summary>
    public class LightningVersionInfo
    {
        internal static LightningVersionInfo Create(INativeLibraryFacade library)
        {
            if (library == null)
                throw new ArgumentNullException("library");

            IntPtr minor, major, patch;
            var version = library.mdb_version(out major, out minor, out patch);

            return new LightningVersionInfo
            {
                Version = Marshal.PtrToStringAnsi(version),
                Major = major.ToInt32(),
                Minor = minor.ToInt32(),
                Patch = patch.ToInt32()
            };
        }

        private LightningVersionInfo()
        {}

        /// <summary>
        /// Major version number.
        /// </summary>
        public int Major { get; private set; }

        /// <summary>
        /// Minor version number.
        /// </summary>
        public int Minor { get; private set; }

        /// <summary>
        /// Patch version number.
        /// </summary>
        public int Patch { get; private set; }

        /// <summary>
        /// Version string.
        /// </summary>
        public string Version { get; private set; }
    }
}
