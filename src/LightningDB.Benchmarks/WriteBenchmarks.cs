using System.Collections;

using BenchmarkDotNet.Attributes;

using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;

namespace LightningDB.Benchmarks
{
    [MemoryDiagnoser]
    public class WriteBenchmarks : RWBenchmarksBase
    {
        [Benchmark]
        public void Write()
        {
            using var transaction = Env.BeginTransaction();

            for (int i = 0; i < OpsPerTransaction; i++) {
                transaction.Put(DB, KeyBuffers[i], ValueBuffer);
            }

            transaction.Commit();
        }
    }
}
