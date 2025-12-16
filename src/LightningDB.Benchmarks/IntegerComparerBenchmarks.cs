using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

/// <summary>
/// Focused benchmarks for integer comparers testing their optimized 4-byte and 8-byte paths.
/// SignedIntegerComparer and UnsignedIntegerComparer have optimized fast paths for
/// int/uint (4 bytes) and long/ulong (8 bytes) that use direct memory reads.
/// </summary>
[MemoryDiagnoser]
public class IntegerComparerBenchmarks
{
    private string _path;
    private LightningEnvironment _env;
    private LightningDatabase _db;
    private byte[][] _keys;
    private byte[] _valueBuffer;

    [ParamsSource(nameof(IntegerComparers))]
    public ComparerDescriptor Comparer { get; set; }

    public static IEnumerable<ComparerDescriptor> IntegerComparers
        => ComparerDescriptor.IntegerComparers;

    [Params(4, 8)]
    public int KeySize { get; set; }

    [Params(1000, 10000)]
    public int OpsPerTransaction { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Console.WriteLine($"Global Setup Begin - Comparer: {Comparer.Name}, KeySize: {KeySize}");

        _path = $"IntBenchDir_{Guid.NewGuid():N}";
        if (Directory.Exists(_path))
            Directory.Delete(_path, true);

        _env = new LightningEnvironment(_path) { MaxDatabases = 1 };
        _env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        if (Comparer.Comparer != null)
            config.CompareWith(Comparer.Comparer);

        using (var tx = _env.BeginTransaction()) {
            _db = tx.OpenDatabase(configuration: config);
            tx.Commit();
        }

        _valueBuffer = new byte[64];
        _keys = GenerateIntegerKeys(OpsPerTransaction, KeySize);

        Console.WriteLine("Global Setup End");
    }

    private static byte[][] GenerateIntegerKeys(int count, int keySize)
    {
        var keys = new byte[count][];

        for (var i = 0; i < count; i++) {
            keys[i] = new byte[keySize];
            if (keySize == 4)
                MemoryMarshal.Write(keys[i], in i);
            else // keySize == 8
                MemoryMarshal.Write(keys[i], (long)i);
        }

        return keys;
    }

    [Benchmark]
    public void WriteIntegers()
    {
        using var tx = _env.BeginTransaction();

        for (var i = 0; i < OpsPerTransaction; i++) {
            tx.Put(_db, _keys[i], _valueBuffer);
        }

        tx.Commit();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Console.WriteLine("Global Cleanup Begin");

        try {
            _db?.Dispose();
            _env?.Dispose();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("Global Cleanup End");
    }
}
