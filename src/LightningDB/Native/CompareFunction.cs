using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CompareFunction(ref MDBValue left, ref MDBValue right);
}
