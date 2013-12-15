using System;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB
{
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

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string Version { get; private set; }
    }
}
