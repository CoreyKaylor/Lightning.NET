using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal class WindowsNativeBinder : IDisposable
    {
        private readonly IntPtr _handle;

        public WindowsNativeBinder(string fileName)
        {
            _handle = LoadLibrary(fileName);
            if (_handle == IntPtr.Zero)
            {
                throw new DllNotFoundException(fileName);
            }

            var type = typeof(LmdbMethods);
            BindDelegates(type);
            type = typeof(LmdbMethods.Overloads);
            BindDelegates(type);
        }

        private void BindDelegates(Type targetType)
        {
            foreach (var field in targetType.GetTypeInfo().DeclaredFields)
            {
                var procAddress = GetProcAddress(_handle, field.Name);
                if (procAddress == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not load member: " + field.Name);
                }

                var value = Marshal.GetDelegateForFunctionPointer(procAddress, field.FieldType);
                field.SetValue(this, value);
            }
        }

        public void Dispose()
        {
            FreeLibrary(_handle);
            GC.SuppressFinalize(this);
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32")]
        private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
    }
}