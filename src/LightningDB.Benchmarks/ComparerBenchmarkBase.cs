using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

/// <summary>
/// Base class for comparer benchmarks with configurable database setup.
/// Derived classes must add their own [ParamsSource] attribute for the Comparer property.
/// </summary>
public abstract class ComparerBenchmarkBase
{
    private string _path;

    protected LightningEnvironment Env { get; private set; }
    protected LightningDatabase DB { get; private set; }

    // Note: Derived classes must add [ParamsSource] attribute and override this property
    public virtual ComparerDescriptor Comparer { get; set; }

    [Params(100, 1000)]
    public int OpsPerTransaction { get; set; }

    [Params(64, 256)]
    public int ValueSize { get; set; }

    protected byte[] ValueBuffer { get; private set; }
    protected KeyBatch KeyBuffers { get; private set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Console.WriteLine($"Global Setup Begin - Comparer: {Comparer.Name}");

        _path = $"BenchmarkDir_{Guid.NewGuid():N}";
        if (Directory.Exists(_path))
            Directory.Delete(_path, true);

        Env = new LightningEnvironment(_path) { MaxDatabases = 1 };
        Env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        if (Comparer.Comparer != null)
            config.CompareWith(Comparer.Comparer);

        using (var tx = Env.BeginTransaction()) {
            DB = tx.OpenDatabase(configuration: config);
            tx.Commit();
        }

        ValueBuffer = new byte[ValueSize];
        KeyBuffers = GenerateKeys();

        RunSetup();

        Console.WriteLine("Global Setup End");
    }

    protected virtual KeyBatch GenerateKeys()
        => KeyBatch.Generate(OpsPerTransaction, KeyOrdering.Sequential);

    protected virtual void RunSetup() { }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Console.WriteLine("Global Cleanup Begin");

        try {
            DB?.Dispose();
            Env?.Dispose();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("Global Cleanup End");
    }
}
