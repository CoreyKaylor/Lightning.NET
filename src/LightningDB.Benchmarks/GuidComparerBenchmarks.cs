using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

/// <summary>
/// Focused benchmarks for GUID comparers testing their optimized 16-byte path.
/// GuidComparer uses two ulong comparisons with big-endian reads for byte ordering.
/// </summary>
[MemoryDiagnoser]
public class GuidComparerBenchmarks
{
    private string _path;
    private LightningEnvironment _env;
    private LightningDatabase _db;
    private byte[][] _keys;
    private byte[] _valueBuffer;

    [ParamsSource(nameof(GuidComparers))]
    public ComparerDescriptor Comparer { get; set; }

    public static IEnumerable<ComparerDescriptor> GuidComparers
        => ComparerDescriptor.GuidComparers;

    [Params(1000, 10000)]
    public int OpsPerTransaction { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Console.WriteLine($"Global Setup Begin - Comparer: {Comparer.Name}");

        _path = $"GuidBenchDir_{Guid.NewGuid():N}";
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
        _keys = GenerateGuidKeys(OpsPerTransaction);

        Console.WriteLine("Global Setup End");
    }

    private static byte[][] GenerateGuidKeys(int count)
    {
        var keys = new byte[count][];

        for (var i = 0; i < count; i++) {
            keys[i] = Guid.NewGuid().ToByteArray();
        }

        return keys;
    }

    [Benchmark]
    public void WriteGuids()
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
