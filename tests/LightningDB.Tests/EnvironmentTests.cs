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
            if (_env != null && _env.IsOpened)
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

        [Fact]
        public void CanCountNumberOfDatabasesThroughEnvironmentEntries()
        {
            _env = new LightningEnvironment(_path);
            _env.MaxDatabases = 5;
            _env.Open();

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase("master", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                for (var i = 0; i < 3; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }
            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase("notmaster", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create}))
            {
                for (var i = 0; i < 3; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }

            Assert.Equal(2, _env.EntriesCount);
        }

        [Fact]
        public void CanGetUsedSize()
        {
            const int entriesCount = 1;

            _env = new LightningEnvironment(_path);
            _env.Open();

            var initialUsedSize = _env.UsedSize;

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                for (int i = 0; i < entriesCount; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }

            var sizeDelta = _env.UsedSize - initialUsedSize;

            Assert.Equal(_env.PageSize, sizeDelta);
        }

    }
}
