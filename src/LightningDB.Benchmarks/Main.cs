using BenchmarkDotNet.Running;

namespace LightningDB.Benchmarks;

public static class Entry
{
    public static void Main(string[] args)
    {
        // Use BenchmarkSwitcher for flexible execution:
        //   dotnet run -c Release -- --filter "*ComparerWrite*"
        //   dotnet run -c Release -- --filter "*IntegerComparer*"
        //   dotnet run -c Release -- --list flat
        BenchmarkSwitcher.FromAssembly(typeof(Entry).Assembly).Run(args);
    }
}