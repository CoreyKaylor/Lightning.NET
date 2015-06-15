using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
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
            if (IsDarwin)
            {
                path = Path.Combine(path, "Binaries", "liblmdb.dylib");
            }
            else if (IsWindows)
            {
                path = Path.Combine(path, "Binaries", IntPtr.Size == 4 ? "lmdb32.dll" : "lmdb64.dll");
            }
            else
            {
                path = "lmdb.so";
            }
            var module = LoadLibrary(path);
            if (module == IntPtr.Zero)
                throw new DllNotFoundException(path);

            var type = typeof(LmdbMethods);
            BindDelegates(type, module);
            type = typeof(LmdbMethods.Overloads);
            BindDelegates(type, module);
        }

        private void BindDelegates(Type type, IntPtr module)
        {
            foreach (var field in type.GetTypeInfo().DeclaredFields)
            {
                var procAddress = GetProcAddress(module, field.Name);
                if (procAddress == IntPtr.Zero)
                    continue;

                var value = Marshal.GetDelegateForFunctionPointer(procAddress, field.FieldType);
                field.SetValue(this, value);
            }
        }
    }
}