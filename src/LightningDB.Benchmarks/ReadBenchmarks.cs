
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks
{
    [MemoryDiagnoser]
    public class ReadBenchmarks : RWBenchmarksBase
    {
        public override void RunSetup()
        {
            base.RunSetup();

            //setup data to read
            using var tx = Env.BeginTransaction();
            for (int i = 0; i < KeyBuffers.Count; i++)
                tx.Put(DB, KeyBuffers[i], ValueBuffer);

            tx.Commit();
        }

        [Benchmark]
        public void Read()
        {
            using var transaction = Env.BeginTransaction(beginFlags: TransactionBeginFlags.ReadOnly);

            for (int i = 0; i < OpsPerTransaction; i++) {
                var _ = transaction.Get(DB, KeyBuffers[i]);
            }
        }
    }
}
