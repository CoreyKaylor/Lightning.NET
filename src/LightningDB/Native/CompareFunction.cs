using System.Runtime.InteropServices;

namespace LightningDB.Native;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int CompareFunction(in MDBValue left, in MDBValue right);