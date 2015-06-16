using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal class UnixNativeBinder : IDisposable
    {
        private IntPtr _handle;

        public UnixNativeBinder(string fileName)
        {
            _handle = dlopen(fileName, 2);
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
                dlerror();
                var pointer = dlsym(_handle, field.Name);
                var error = dlerror();
                if (error != IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not load member: " + field.Name);
                }

                var value = Marshal.GetDelegateForFunctionPointer(pointer, field.FieldType);
                field.SetValue(this, value);
            }
        }

        public void Dispose()
        {
            dlclose(_handle);
            GC.SuppressFinalize(this);
        }

        [DllImport("dl")]
        private static extern IntPtr dlopen(string fileName, int flags);
        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);
        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
        [DllImport("dl")]
        private static extern IntPtr dlerror();
    }
}