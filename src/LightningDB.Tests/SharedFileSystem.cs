using System;
using System.IO;
using Xunit;

namespace LightningDB.Tests
{
    public class SharedFileSystem : IDisposable
    {
        private readonly string _testTempDir;

        public SharedFileSystem()
        {
            _testTempDir = Path.Combine(Directory.GetCurrentDirectory(), "testrun");
        }

        public void Dispose()
        {
            if (Directory.Exists(_testTempDir))
            {
                Directory.Delete(_testTempDir, true);
            }
        }

        public string CreateNewDirectoryForTest()
        {
            var path = Path.Combine(_testTempDir, "TestDb", Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }
    }

    [CollectionDefinition("SharedFileSystem")]
    public class SharedFileSystemCollection : ICollectionFixture<SharedFileSystem>
    {
    }
}