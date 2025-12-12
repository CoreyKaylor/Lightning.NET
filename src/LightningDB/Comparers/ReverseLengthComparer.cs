using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances by length first (descending), then by content (descending).
/// Longer values sort before shorter values.
/// </summary>
public sealed class ReverseLengthComparer : IComparer<MDBValue>
{
    public static readonly ReverseLengthComparer Instance = new();

    private ReverseLengthComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();
        var lengthCmp = right.Length.CompareTo(left.Length);
        return lengthCmp != 0 ? lengthCmp : right.SequenceCompareTo(left);
    }
}
