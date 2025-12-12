using System;
using System.Collections.Generic;

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
            return x.Read<uint>().CompareTo(y.Read<uint>());

        if (left.Length == 8 && right.Length == 8)
            return x.Read<ulong>().CompareTo(y.Read<ulong>());

        return left.SequenceCompareTo(right);
    }
}
