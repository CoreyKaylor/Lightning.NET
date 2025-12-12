using System;
using System.Collections.Generic;
#if !NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances by hash code for faster comparison of large values.
/// Falls back to byte comparison when hashes collide.
/// Sort order is deterministic within a process but may vary across restarts.
/// </summary>
public sealed class HashCodeComparer : IComparer<MDBValue>
{
    public static readonly HashCodeComparer Instance = new();

    private HashCodeComparer() { }

    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        var leftHash = ComputeHash(left);
        var rightHash = ComputeHash(right);

        var hashCmp = leftHash.CompareTo(rightHash);
        return hashCmp != 0 ? hashCmp : left.SequenceCompareTo(right);
    }

#if NET6_0_OR_GREATER
    private static int ComputeHash(ReadOnlySpan<byte> data)
    {
        var hc = new HashCode();
        hc.AddBytes(data);
        return hc.ToHashCode();
    }
#else
    private static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        const ulong prime = 0x9E3779B97F4A7C15UL;
        var hash = (ulong)data.Length;

        while (data.Length >= 8)
        {
            hash ^= MemoryMarshal.Read<ulong>(data);
            hash *= prime;
            hash ^= hash >> 32;
            data = data.Slice(8);
        }

        if (data.Length >= 4)
        {
            hash ^= MemoryMarshal.Read<uint>(data);
            hash *= prime;
            data = data.Slice(4);
        }

        foreach (var b in data)
        {
            hash ^= b;
            hash *= prime;
        }

        return hash;
    }
#endif
}
