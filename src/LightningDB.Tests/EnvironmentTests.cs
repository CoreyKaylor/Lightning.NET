using System;
using System.IO;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class EnvironmentTests : IDisposable
    {
        private string _path,  _pathCopy;
        private LightningEnvironment _env;

        public EnvironmentTests(SharedFileSystem fileSystem)
        {
            _path = fileSystem.CreateNewDirectoryForTest();
            _pathCopy = fileSystem.CreateNewDirectoryForTest();
        }

        public void Dispose()
        {
            if (_env != null)
                _env.Dispose();

            _env = null;
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfWithoutFlags()
        {
            _env = new LightningEnvironment(_path);
            _env.Open();
        }

        [Fact]
        public void EnvironmentCreatedFromConfig()
        {
            var mapExpected = 1024*1024*20;
            var maxDatabaseExpected = 2;
            var maxReadersExpected = 3;
            var config = new EnvironmentConfiguration {MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected};
            _env = new LightningEnvironment(_path, config);
            Assert.Equal(_env.MapSize, mapExpected);
            Assert.Equal(_env.MaxDatabases, maxDatabaseExpected);
            Assert.Equal(_env.MaxReaders, maxReadersExpected);
        }

        [Fact]
        public void StartingTransactionBeforeEnvironmentOpen()
        {
            _env = new LightningEnvironment(_path);
            Assert.Throws<InvalidOperationException>(() => _env.BeginTransaction());
        }

        [Fact]
        public void MaxDatabasesWorksThroughConfigIssue62()
        {
            var config = new EnvironmentConfiguration { MaxDatabases = 2 };
            _env = new LightningEnvironment(_path, config);
            _env.Open();
            using (var tx = _env.BeginTransaction())
            {
                tx.OpenDatabase("db1", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
                tx.OpenDatabase("db2", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
                tx.Commit();
            }
            Assert.Equal(_env.MaxDatabases, 2);
        }

        [Fact]
        public void CanLoadAndDisposeMultipleEnvironments()
        {
            _env = new LightningEnvironment(_path);
            _env.Dispose();
            _env = new LightningEnvironment(_path);
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfReadOnly()
        {
            _env = new LightningEnvironment(_path);
            _env.Open(); //readonly requires environment to have been created at least once before
            _env.Dispose();
            _env = new LightningEnvironment(_path);
            _env.Open(EnvironmentOpenFlags.ReadOnly);
        }

        [Fact]
        public void EnvironmentShouldBeOpened()
        {
            _env = new LightningEnvironment(_path);
            _env.Open();

            Assert.True(_env.IsOpened);
        }

        [Fact]
        public void EnvironmentShouldBeClosed()
        {
            _env = new LightningEnvironment(_path);
            _env.Open();

            _env.Dispose();

            Assert.False(_env.IsOpened);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnvironmentShouldBeCopied(bool compact)
        {
            _env = new LightningEnvironment(_path);
            _env.Open(); 

            _env.CopyTo(_pathCopy, compact);

            if (Directory.GetFiles(_pathCopy).Length == 0)
                Assert.True(false, "Copied files doesn't exist");
        }

        [Fact]
        public void CanOpenEnvironmentMoreThan50Mb()
        {
            _env = new LightningEnvironment(_path)
            {
                MapSize = 55 * 1024 * 1024
            };

            _env.Open();
        }
    }
}
