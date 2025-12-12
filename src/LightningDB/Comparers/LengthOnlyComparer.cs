using System.Collections.Generic;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances by length only, ignoring content.
/// Values of equal length are considered equal regardless of content.
/// </summary>
public sealed class LengthOnlyComparer : IComparer<MDBValue>
{
    public static readonly LengthOnlyComparer Instance = new();

    private LengthOnlyComparer() { }

    public int Compare(MDBValue x, MDBValue y)
        => x.AsSpan().Length.CompareTo(y.AsSpan().Length);
}
