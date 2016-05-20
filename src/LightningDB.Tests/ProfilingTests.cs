using System;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class ProfilingTests : IDisposable
    {
        private readonly LightningEnvironment _env;

        public ProfilingTests(SharedFileSystem fileSystem)
        {
            _env = new LightningEnvironment(fileSystem.CreateNewDirectoryForTest());
            _env.MaxDatabases = 2;
            _env.Open();
        }

        [Fact(Skip = "Can come back to profiling tests later"), Trait("prof", "explicit")]
        public void DoStuff()
        {
            Console.WriteLine("Take a baseline snapshot then press enter.");
            Console.ReadLine();
            DoStuffHelper();
            Console.WriteLine("Take another snapshot for comparison and press enter.");
            Console.ReadLine();
        }

        private void DoStuffHelper()
        {
            using (var tx = _env.BeginTransaction())
            using (var db = tx.OpenDatabase("test", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                for (var i = 0; i < 100; ++i)
                {
                    tx.Put(db, BitConverter.GetBytes(i), BitConverter.GetBytes(i));
                }
                tx.Commit();
            }
            using (var tx = _env.BeginTransaction())
            using (var db = tx.OpenDatabase("test"))
            {
                tx.Get(db, BitConverter.GetBytes(1));
                using (var cursor = tx.CreateCursor(db))
                {
                    while (cursor.MoveNext())
                    {
                        var current = cursor.Current;
                    }
                }
            }
        }

        public void Dispose()
        {
            _env.Dispose();
        }
    }
}