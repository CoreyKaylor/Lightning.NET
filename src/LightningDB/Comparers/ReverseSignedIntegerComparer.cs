using System;
using System.Collections.Generic;

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
            return y.Read<int>().CompareTo(x.Read<int>());

        if (left.Length == 8 && right.Length == 8)
            return y.Read<long>().CompareTo(x.Read<long>());

        return right.SequenceCompareTo(left);
    }
}
