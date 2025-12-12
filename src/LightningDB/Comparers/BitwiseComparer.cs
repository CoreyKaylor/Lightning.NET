using System;
using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances using lexicographic byte comparison (memcmp-style).
/// </summary>
public sealed class BitwiseComparer : IComparer<MDBValue>
{
    public static readonly BitwiseComparer Instance = new();

    private BitwiseComparer() { }

    public int Compare(MDBValue x, MDBValue y)
        => x.AsSpan().SequenceCompareTo(y.AsSpan());
}
