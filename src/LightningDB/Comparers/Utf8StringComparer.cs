using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as UTF-8 encoded strings using ordinal comparison.
/// </summary>
public sealed class Utf8StringComparer : IComparer<MDBValue>
{
    public static readonly Utf8StringComparer Instance = new();

    private Utf8StringComparer() { }

    public int Compare(MDBValue x, MDBValue y)
        => x.AsSpan().SequenceCompareTo(y.AsSpan());
}
