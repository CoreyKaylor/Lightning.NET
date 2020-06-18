using System;
using System.Dynamic;
using System.IO;

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
            Console.WriteLine("Global Setup Begin");

            const string Path = "TestDirectory";
            
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);

            Env = new LightningEnvironment(Path) {
                MaxDatabases = 1
            };
            
            Env.Open();

            using (var tx = Env.BeginTransaction()) {
                DB = tx.OpenDatabase();
                tx.Commit();
            }

            RunSetup();

            Console.WriteLine("Global Setup End");
        }

        public abstract void RunSetup();

        [GlobalCleanup]
        public void GlobalCleanup() 
        {
            Console.WriteLine("Global Cleanup Begin");

            try {
                DB.Dispose();
                Env.Dispose();
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Global Cleanup End");
        }
    }

    public abstract class RWBenchmarksBase : BenchmarksBase
    {
        //***** Argument Matrix Start *****//
        [Params(1, 100, 1000)]
        public int OpsPerTransaction { get; set; }

        [Params(8, 64, 256)]
        public int ValueSize { get; set; }

        [Params(KeyOrdering.Sequential)]
        public KeyOrdering KeyOrder { get; set; }

        //***** Argument Matrix End *****//



        //***** Test Values Begin *****//

        protected byte[] ValueBuffer { get; private set; }
        protected KeyBatch KeyBuffers { get; private set; }

        //***** Test Values End *****//

        public override void RunSetup()
        {
            ValueBuffer = new byte[ValueSize];
            KeyBuffers = KeyBatch.Generate(OpsPerTransaction, KeyOrder);
        }
    }
}
