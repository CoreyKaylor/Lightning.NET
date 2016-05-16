using System;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class FinalizeTests
    {
        public FinalizeTests(SharedFileSystem fileSystem)
        {
            var env = new LightningEnvironment(fileSystem.CreateNewDirectoryForTest());
            env.Open();
            var tx = env.BeginTransaction();
            var db = tx.OpenDatabase();
            tx.CreateCursor(db);
        }

        [Fact]
        public void FinalizerDoesntThrow()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}