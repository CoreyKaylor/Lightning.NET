using System.Collections.Generic;
using LightningDB.Comparers;

namespace LightningDB.Benchmarks;

/// <summary>
/// Wraps a comparer for BenchmarkDotNet parameterization with friendly display names.
/// </summary>
public readonly struct ComparerDescriptor
{
    public string Name { get; }
    public IComparer<MDBValue> Comparer { get; }

    private ComparerDescriptor(string name, IComparer<MDBValue> comparer)
    {
        Name = name;
        Comparer = comparer;
    }

    public override string ToString() => Name;

    /// <summary>
    /// All available comparers including Default (null = LMDB native).
    /// </summary>
    public static IEnumerable<ComparerDescriptor> All => new[]
    {
        new ComparerDescriptor("Default", null),
        new ComparerDescriptor("Bitwise", BitwiseComparer.Instance),
        new ComparerDescriptor("ReverseBitwise", ReverseBitwiseComparer.Instance),
        new ComparerDescriptor("SignedInt", SignedIntegerComparer.Instance),
        new ComparerDescriptor("ReverseSignedInt", ReverseSignedIntegerComparer.Instance),
        new ComparerDescriptor("UnsignedInt", UnsignedIntegerComparer.Instance),
        new ComparerDescriptor("ReverseUnsignedInt", ReverseUnsignedIntegerComparer.Instance),
        new ComparerDescriptor("Utf8String", Utf8StringComparer.Instance),
        new ComparerDescriptor("ReverseUtf8String", ReverseUtf8StringComparer.Instance),
        new ComparerDescriptor("Length", LengthComparer.Instance),
        new ComparerDescriptor("ReverseLength", ReverseLengthComparer.Instance),
        new ComparerDescriptor("LengthOnly", LengthOnlyComparer.Instance),
        new ComparerDescriptor("HashCode", HashCodeComparer.Instance),
    };

    /// <summary>
    /// Integer comparers only (for focused integer key benchmarks).
    /// </summary>
    public static IEnumerable<ComparerDescriptor> IntegerComparers => new[]
    {
        new ComparerDescriptor("Default", null),
        new ComparerDescriptor("SignedInt", SignedIntegerComparer.Instance),
        new ComparerDescriptor("ReverseSignedInt", ReverseSignedIntegerComparer.Instance),
        new ComparerDescriptor("UnsignedInt", UnsignedIntegerComparer.Instance),
        new ComparerDescriptor("ReverseUnsignedInt", ReverseUnsignedIntegerComparer.Instance),
    };
}
