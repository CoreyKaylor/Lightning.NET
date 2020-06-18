using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;

namespace LightningDB.Benchmarks {
    public static class Entry 
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<WriteBenchmarks>();
        }
    }
}