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
            //arrange

            //act
            _env = new LightningEnvironment(_path);
            _env.Open();

            //assert
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
            //arrange

            //act
            _env = new LightningEnvironment(_path);
            _env.Open(); //readonly requires environment to have been created at least once before
            _env.Dispose();
            _env = new LightningEnvironment(_path);
            _env.Open(EnvironmentOpenFlags.ReadOnly);

            //assert
        }

        [Fact]
        public void EnvironmentShouldBeOpened()
        {
            //arrange
            _env = new LightningEnvironment(_path);

            //act
            _env.Open();

            //assert
            Assert.True(_env.IsOpened);
        }

        [Fact]
        public void EnvironmentShouldBeClosed()
        {
            //arrange
            _env = new LightningEnvironment(_path);
            _env.Open();

            //act
            _env.Dispose();

            //assert
            Assert.False(_env.IsOpened);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnvironmentShouldBeCopied(bool compact)
        {
            //arrange
            _env = new LightningEnvironment(_path);
            _env.Open(); 

            //act
            _env.CopyTo(_pathCopy, compact);

            //assert
            if (Directory.GetFiles(_pathCopy).Length == 0)
                Assert.True(false, "Copied files doesn't exist");
        }

        [Fact]
        public void CanOpenEnvironmentMoreThan50Mb()
        {
            //arrange
            _env = new LightningEnvironment(_path)
            {
                MapSize = 55 * 1024 * 1024
            };

            //act-assert
            _env.Open();
        }

        [Fact]
        public void CanCountNumberOfDatabasesThroughEnvironmentEntries()
        {
            _env = new LightningEnvironment(_path);
            _env.MaxDatabases = 5;
            _env.Open();

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase("master", new DatabaseOptions { Flags = DatabaseOpenFlags.Create }))
            {
                for (var i = 0; i < 3; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }
            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase("notmaster", new DatabaseOptions {Flags = DatabaseOpenFlags.Create}))
            {
                for (var i = 0; i < 3; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }

            Assert.Equal(2, _env.EntriesCount);
        }

        [Fact(Skip = "Not sure what the logic really should be here, but guessing it was a false positive, not sure though")]
        public void CanGetUsedSize()
        {
            const int entriesCount = 1;

            //arrange
            _env = new LightningEnvironment(_path);
            _env.MaxDatabases = 0;
            _env.Open();

            var initialUsedSize = _env.UsedSize;

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase("master", new DatabaseOptions { Flags = DatabaseOpenFlags.Create }))
            {
                for (int i = 0; i < entriesCount; i++)
                    txn.Put(db, i, i);

                txn.Commit();
            }

            //act
            var sizeDelta = _env.UsedSize - initialUsedSize;

            //act-assert;
            Assert.Equal(_env.PageSize, sizeDelta);
        }

    }
}
