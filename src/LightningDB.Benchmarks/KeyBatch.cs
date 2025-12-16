using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB.Benchmarks;

public enum KeyOrdering
{
    Sequential,
    Random
}

/// <summary>
/// A collection of key arrays with configurable size
/// </summary>
public class KeyBatch
{
    private KeyBatch(byte[][] buffers)
    {
        Buffers = buffers;
    }

    public byte[][] Buffers { get; }


    public int Count => Buffers.Length;
    public ref byte[] this[int index] => ref Buffers[index];


    public static KeyBatch Generate(int keyCount, KeyOrdering keyOrdering)
        => Generate(keyCount, keyOrdering, keySize: 4);

    public static KeyBatch Generate(int keyCount, KeyOrdering keyOrdering, int keySize)
    {
        var buffers = new byte[keyCount][];

        switch (keyOrdering) {
            case KeyOrdering.Sequential:
                PopulateSequential(buffers, keySize);
                break;

            case KeyOrdering.Random:
                PopulateRandom(buffers, keySize);
                break;

            default:
                throw new ArgumentException("That isn't a valid KeyOrdering", nameof(keyOrdering));
        }

        return new KeyBatch(buffers);
    }

    private static void PopulateSequential(byte[][] buffers, int keySize)
    {
        for (var i = 0; i < buffers.Length; i++) {
            buffers[i] = CopyToArray(i, keySize);
        }
    }

    private static void PopulateRandom(byte[][] buffers, int keySize)
    {
        var random = new Random(0);
        var seen = new HashSet<int>(buffers.Length);

        var i = 0;
        while (i < buffers.Length) {
            var keyValue = random.Next(0, buffers.Length);

            if (!seen.Add(keyValue))
                continue;//skip duplicates

            buffers[i++] = CopyToArray(keyValue, keySize);
        }
    }

    private static byte[] CopyToArray(int keyValue, int keySize)
    {
        var key = new byte[keySize];
        if (keySize >= 8)
            MemoryMarshal.Write(key, (long)keyValue);
        else
            MemoryMarshal.Write(key, in keyValue);
        return key;
    }
}