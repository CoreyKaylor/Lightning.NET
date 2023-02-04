using BenchmarkDotNet.Running;

namespace LightningDB.Benchmarks;

public static class Entry 
{
    public static void Main(string[] args)
    {
        //BenchmarkRunner.Run<WriteBenchmarks>();
        BenchmarkRunner.Run<ReadBenchmarks>();
    }
}