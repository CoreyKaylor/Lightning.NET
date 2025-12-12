using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances by length first, then by content.
/// Shorter values sort before longer values.
/// </summary>
public sealed class LengthComparer : IComparer<MDBValue>
{
    public static readonly LengthComparer Instance = new();

    private LengthComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();
        var lengthCmp = left.Length.CompareTo(right.Length);
        return lengthCmp != 0 ? lengthCmp : left.SequenceCompareTo(right);
    }
}
