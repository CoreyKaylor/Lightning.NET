using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LightningDB.Comparers;

/// <summary>
/// Compares MDBValue instances as GUIDs in reverse byte order (descending).
/// Optimized for 16-byte values using two ulong comparisons with early termination.
/// Falls back to reverse bitwise comparison for non-16-byte inputs.
/// </summary>
/// <remarks>
/// This comparer uses reverse byte-level ordering, NOT reverse Guid.CompareTo() ordering.
/// GUIDs are compared as raw byte sequences from first byte to last, then reversed.
/// </remarks>
public sealed class ReverseGuidComparer : IComparer<MDBValue>
{
    public static readonly ReverseGuidComparer Instance = new();

    private ReverseGuidComparer() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(MDBValue x, MDBValue y)
    {
        var left = x.AsSpan();
        var right = y.AsSpan();

        if (left.Length == 16 && right.Length == 16)
        {
            // Compare first 8 bytes (reversed: right vs left)
            var leftHigh = BinaryPrimitives.ReadUInt64BigEndian(left);
            var rightHigh = BinaryPrimitives.ReadUInt64BigEndian(right);

            var cmp = rightHigh.CompareTo(leftHigh);
            if (cmp != 0)
                return cmp;

            // Compare last 8 bytes (reversed)
            var leftLow = BinaryPrimitives.ReadUInt64BigEndian(left.Slice(8));
            var rightLow = BinaryPrimitives.ReadUInt64BigEndian(right.Slice(8));

            return rightLow.CompareTo(leftLow);
        }

        return right.SequenceCompareTo(left);
    }
}
