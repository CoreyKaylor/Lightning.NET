using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

/// <summary>
/// Benchmarks read operations across all comparers.
/// Reads use comparers for B-tree traversal during key lookups.
/// </summary>
[MemoryDiagnoser]
public class ComparerReadBenchmarks : ComparerBenchmarkBase
{
    [ParamsSource(nameof(AllComparers))]
    public override ComparerDescriptor Comparer { get; set; }

    public static IEnumerable<ComparerDescriptor> AllComparers => ComparerDescriptor.All;

    protected override void RunSetup()
    {
        // Pre-populate database for reads
        using var tx = Env.BeginTransaction();
        for (var i = 0; i < KeyBuffers.Count; i++)
            tx.Put(DB, KeyBuffers[i], ValueBuffer);
        tx.Commit();
    }

    [Benchmark]
    public void Read()
    {
        using var tx = Env.BeginTransaction(TransactionBeginFlags.ReadOnly);

        for (var i = 0; i < OpsPerTransaction; i++) {
            _ = tx.Get(DB, KeyBuffers[i]);
        }
    }
}
