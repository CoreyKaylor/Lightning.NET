using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Native
{
    internal static class PlatformApis
    {
        public static bool IsWindows()
        {
#if DNXCORE50
            // Until Environment.OSVersion.Platform is exposed on .NET Core, we
            // try to call uname and if that fails we assume we are on Windows.
            return true;
#else
            var p = (int)Environment.OSVersion.Platform;
            return (p != 4) && (p != 6) && (p != 128);
#endif
        }

        [DllImport("libc")]
        static extern int uname(StringBuilder buf);

        public static bool IsDarwin()
        {
            var buffer = new StringBuilder(8192);
            try
            {
                if (uname(buffer) == 0)
                {
                    return string.Equals(
                        buffer.ToString(),
                        "Darwin",
                        StringComparison.Ordinal);
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static void Apply(DnxLibraryLoader instance)
        {
            if (instance.IsWindows)
            {
                WindowsApis.Apply(instance);
            }
            else
            {
                LinuxApis.Apply(instance);
            }
        }

        internal static class WindowsApis
        {
            [DllImport("kernel32")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32")]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            public static void Apply(DnxLibraryLoader dnxLibrary)
            {
                dnxLibrary.LoadLibrary = LoadLibrary;
                dnxLibrary.FreeLibrary = FreeLibrary;
                dnxLibrary.GetProcAddress = GetProcAddress;
            }
        }

        internal static class LinuxApis
        {
            [DllImport("libdl")]
            public static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl")]
            public static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl")]
            public static extern int dlclose(IntPtr handle);

            [DllImport("libdl")]
            public static extern IntPtr dlerror();

            public static IntPtr LoadLibrary(string dllToLoad)
            {
                return dlopen(dllToLoad, 2);
            }

            public static bool FreeLibrary(IntPtr hModule)
            {
                return dlclose(hModule) == 0;
            }

            public static IntPtr GetProcAddress(IntPtr hModule, string procedureName)
            {
                dlerror();
                var res = dlsym(hModule, procedureName);
                var errPtr = dlerror();
                return errPtr == IntPtr.Zero ? res : IntPtr.Zero;
            }

            public static void Apply(DnxLibraryLoader instance)
            {
                instance.LoadLibrary = LoadLibrary;
                instance.FreeLibrary = FreeLibrary;
                instance.GetProcAddress = GetProcAddress;
            }
        }
    }
}