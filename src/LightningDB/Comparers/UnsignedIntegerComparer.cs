using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as unsigned integers (uint or ulong).
/// Supports 4-byte and 8-byte values. Matches LMDB's MDB_INTEGERKEY behavior.
/// Falls back to bitwise comparison for other sizes.
/// </summary>
public sealed class UnsignedIntegerComparer : IComparer<MDBValue>
{
    public static readonly UnsignedIntegerComparer Instance = new();

    private UnsignedIntegerComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 4 && right.Length == 4)
            return MemoryMarshal.Read<uint>(left).CompareTo(MemoryMarshal.Read<uint>(right));

        if (left.Length == 8 && right.Length == 8)
            return MemoryMarshal.Read<ulong>(left).CompareTo(MemoryMarshal.Read<ulong>(right));

        return left.SequenceCompareTo(right);
    }
}
