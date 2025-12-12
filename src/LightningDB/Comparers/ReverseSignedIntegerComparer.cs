using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as signed integers (int or long) in descending order.
/// Supports 4-byte and 8-byte values. Falls back to reverse bitwise comparison for other sizes.
/// </summary>
public sealed class ReverseSignedIntegerComparer : IComparer<MDBValue>
{
    public static readonly ReverseSignedIntegerComparer Instance = new();

    private ReverseSignedIntegerComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 4 && right.Length == 4)
            return MemoryMarshal.Read<int>(right).CompareTo(MemoryMarshal.Read<int>(left));

        if (left.Length == 8 && right.Length == 8)
            return MemoryMarshal.Read<long>(right).CompareTo(MemoryMarshal.Read<long>(left));

        return right.SequenceCompareTo(left);
    }
}
