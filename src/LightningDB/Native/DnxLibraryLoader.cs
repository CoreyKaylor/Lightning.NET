using System;

namespace LightningDB.Native
{
#if DNXCORE50 || DNX451
    public class DnxLibraryLoader
    {
        public DnxLibraryLoader()
        {
            IsWindows = PlatformApis.IsWindows();
            IsDarwin = PlatformApis.IsDarwin();
        }

        public bool IsWindows;
        public bool IsDarwin;
        public Func<IntPtr, bool> FreeLibrary;
        public Func<string, IntPtr> LoadLibrary;
        public Func<IntPtr, string, IntPtr> GetProcAddress;

        public void Load(string path)
        {
            PlatformApis.Apply(this);
            NativeMethods.LoadLibrary(this, path);
        }
    }
#endif
}