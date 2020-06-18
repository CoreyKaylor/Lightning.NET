using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

using LightningDB;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightningDB.Benchmarks
{
    public abstract class BenchmarksBase
    {
        public LightningEnvironment Env { get; set; }
        public LightningDatabase DB { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            const string Path = "TestDirectory";
            
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);

            Env = new LightningEnvironment(Path) {
                MaxDatabases = 1
            };
            
            Env.Open();

            using var tx = Env.BeginTransaction();
            DB = tx.OpenDatabase();

            RunSetup();
        }

        public abstract void RunSetup();

        [GlobalCleanup]
        public void GlobalCleanup() 
        {
            DB.Dispose();
            Env.Dispose();
        }
    }

    [MemoryDiagnoser]
    public class WriteBenchmarks : BenchmarksBase
    {
        [Params(1, 10, 100)]
        public int BatchSize { get; set; }


        [Params(4, 8, 64, 256)]
        public int WriteSize { get; set; }


        //[Params(true, false)]
        [Params(true)]
        public bool SequentialKeys { get; set; }

        public byte[] ValueBuffer { get; set; }
        public byte[][] KeyBuffers { get; set; }



        public override void RunSetup()
        {
            ValueBuffer = new byte[WriteSize];
            KeyBuffers = new byte[BatchSize][];

            if(SequentialKeys) {
                for(int i = 0; i < BatchSize; i++) {
                    var key = new byte[4];
                    MemoryMarshal.Write(key, ref i);
                    KeyBuffers[i] = key;
                }
            }
            else {
                var random = new Random(0);
                var seen = new HashSet<int>(BatchSize);

                for (int i = 0; i < BatchSize;) {                    
                    var keyValue = random.Next(0, BatchSize);

                    if (seen.Add(keyValue)) {
                        var key = new byte[4];
                        MemoryMarshal.Write(key, ref keyValue);
                        KeyBuffers[i++] = key;
                    }
                    else
                        continue;
                }
            }
        }


        [Benchmark]
        public void Write()
        {
            using(var transaction = Env.BeginTransaction())
            {
                for (int i = 0; i < BatchSize; i++) {
                    transaction.Put(DB, KeyBuffers[i], ValueBuffer);
                }
            }
        }
    }
}
