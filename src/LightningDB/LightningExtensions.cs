using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB
{
    public static class LightningExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MDBResultCode ThrowOnReadError(this MDBResultCode resultCode)
        {
            if (resultCode == MDBResultCode.NotFound)
                return resultCode;
            return resultCode.ThrowOnError();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MDBResultCode ThrowOnError(this MDBResultCode resultCode)
        {
            if (resultCode == MDBResultCode.Success)
                return resultCode;
            var statusCode = (int) resultCode;
            var message = mdb_strerror(statusCode);
            throw new LightningException(message, statusCode); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (MDBResultCode resultCode, MDBValue key, MDBValue value) ThrowOnReadError(
            this ValueTuple<MDBResultCode, MDBValue, MDBValue> result)
        {
            result.Item1.ThrowOnReadError();
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string mdb_strerror(int err)
        {
            var ptr = LmdbMethods.mdb_strerror(err);
            return Marshal.PtrToStringAnsi(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<ValueTuple<MDBValue, MDBValue>> AsEnumerable(this LightningCursor cursor)
        {
            while (cursor.MoveNext() == MDBResultCode.Success)
            {
                var current = cursor.GetCurrent();
                yield return (current.key, current.value);
            }
        }
    }
}