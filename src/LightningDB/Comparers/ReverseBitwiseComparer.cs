using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances using lexicographic byte comparison in descending order.
/// </summary>
public sealed class ReverseBitwiseComparer : IComparer<MDBValue>
{
    public static readonly ReverseBitwiseComparer Instance = new();

    private ReverseBitwiseComparer() { }

    public int Compare(MDBValue x, MDBValue y)
        => y.AsSpan().SequenceCompareTo(x.AsSpan());
}
