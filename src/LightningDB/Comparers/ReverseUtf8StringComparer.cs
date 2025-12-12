using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as UTF-8 encoded strings in descending order.
/// </summary>
public sealed class ReverseUtf8StringComparer : IComparer<MDBValue>
{
    public static readonly ReverseUtf8StringComparer Instance = new();

    private ReverseUtf8StringComparer() { }

    public int Compare(MDBValue x, MDBValue y)
        => y.AsSpan().SequenceCompareTo(x.AsSpan());
}
