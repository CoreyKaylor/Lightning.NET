using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as unsigned integers (uint or ulong) in descending order.
/// Supports 4-byte and 8-byte values. Falls back to reverse bitwise comparison for other sizes.
/// </summary>
public sealed class ReverseUnsignedIntegerComparer : IComparer<MDBValue>
{
    public static readonly ReverseUnsignedIntegerComparer Instance = new();

    private ReverseUnsignedIntegerComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 4 && right.Length == 4)
            return y.Read<uint>().CompareTo(x.Read<uint>());

        if (left.Length == 8 && right.Length == 8)
            return y.Read<ulong>().CompareTo(x.Read<ulong>());

        return right.SequenceCompareTo(left);
    }
}
