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
                _env.Close();

            _env = null;
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfWithoutFlags()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //assert
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfReadOnly()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.ReadOnly);

            //assert
        }

        [Fact]
        public void EnvironmentShouldBeOpened()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //act
            _env.Open();

            //assert
            Assert.Equal(true, _env.IsOpened);
        }

        [Fact]
        public void EnvironmentShouldBeClosed()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            //act
            _env.Close();

            //assert
            Assert.Equal(false, _env.IsOpened);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnvironmentShouldBeCopied(bool compact)
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
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
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None)
            {
                MapSize = 55 * 1024 * 1024
            };

            //act-assert
            _env.Open();
        }

        [Fact]
        public void CanCountEnvironmentEntries()
        {
            const int entriesCount = 10;

            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None }))
            {
                for (var i = 0; i < entriesCount; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }

            //act
            var count = _env.EntriesCount;

            //assert;
            Assert.Equal(entriesCount, count);
        }

        [Fact]
        public void CanGetUsedSize()
        {
            const int entriesCount = 1;

            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            var initialUsedSize = _env.UsedSize;

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None }))
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
