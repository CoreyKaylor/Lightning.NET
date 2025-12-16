using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

/// <summary>
/// Benchmarks write operations across all comparers.
/// Write operations trigger B-tree insertions which invoke comparers frequently.
/// </summary>
[MemoryDiagnoser]
public class ComparerWriteBenchmarks : ComparerBenchmarkBase
{
    [ParamsSource(nameof(AllComparers))]
    public override ComparerDescriptor Comparer { get; set; }

    public static IEnumerable<ComparerDescriptor> AllComparers => ComparerDescriptor.All;

    [Benchmark]
    public void Write()
    {
        using var tx = Env.BeginTransaction();

        for (var i = 0; i < OpsPerTransaction; i++) {
            tx.Put(DB, KeyBuffers[i], ValueBuffer);
        }

        tx.Commit();
    }
}
