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
            return GetUname() == string.Empty;
#else
            var p = (int)Environment.OSVersion.Platform;
            return (p != 4) && (p != 6) && (p != 128);
#endif
        }

        [DllImport("libc")]
        static extern int uname(StringBuilder buf);

        private static string GetUname()
        {
            var buffer = new StringBuilder(8192);
            try
            {
                if(uname(buffer) == 0)
                {
                    return buffer.ToString();
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public static bool IsDarwin()
        {
            return string.Equals(GetUname(), "Darwin", StringComparison.Ordinal);
        }
    }
}