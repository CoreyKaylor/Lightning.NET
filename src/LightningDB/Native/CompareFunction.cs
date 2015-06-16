using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int CompareFunction(ref ValueStructure left, ref ValueStructure right);
}
