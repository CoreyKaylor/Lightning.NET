using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as signed integers (int or long).
/// Supports 4-byte and 8-byte values. Negative values sort before positive values.
/// Falls back to bitwise comparison for other sizes.
/// </summary>
public sealed class SignedIntegerComparer : IComparer<MDBValue>
{
    public static readonly SignedIntegerComparer Instance = new();

    private SignedIntegerComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 4 && right.Length == 4)
            return x.Read<int>().CompareTo(y.Read<int>());

        if (left.Length == 8 && right.Length == 8)
            return x.Read<long>().CompareTo(y.Read<long>());

        return left.SequenceCompareTo(right);
    }
}
