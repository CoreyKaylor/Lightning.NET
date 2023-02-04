using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks;

[MemoryDiagnoser]
public class WriteBenchmarks : RWBenchmarksBase
{
    [Benchmark]
    public void Write()
    {
        using var transaction = Env.BeginTransaction();

        for (var i = 0; i < OpsPerTransaction; i++) {
            transaction.Put(DB, KeyBuffers[i], ValueBuffer);
        }

        transaction.Commit();
    }
}