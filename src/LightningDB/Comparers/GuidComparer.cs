using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as GUIDs using byte ordering (lexicographic).
/// Optimized for 16-byte values using two ulong comparisons with early termination.
/// Falls back to bitwise comparison for non-16-byte inputs.
/// </summary>
/// <remarks>
/// This comparer uses byte-level ordering (memcmp-style), NOT Guid.CompareTo() ordering.
/// GUIDs are compared as raw byte sequences from first byte to last.
/// </remarks>
public sealed class GuidComparer : IComparer<MDBValue>
{
    public static readonly GuidComparer Instance = new();

    private GuidComparer() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 16 && right.Length == 16)
        {
            // Compare first 8 bytes (big-endian for correct byte ordering)
            var leftHigh = BinaryPrimitives.ReadUInt64BigEndian(left);
            var rightHigh = BinaryPrimitives.ReadUInt64BigEndian(right);

            var cmp = leftHigh.CompareTo(rightHigh);
            if (cmp != 0)
                return cmp;

            // Compare last 8 bytes
            var leftLow = BinaryPrimitives.ReadUInt64BigEndian(left.Slice(8));
            var rightLow = BinaryPrimitives.ReadUInt64BigEndian(right.Slice(8));

            return leftLow.CompareTo(rightLow);
        }

        return left.SequenceCompareTo(right);
    }
}
