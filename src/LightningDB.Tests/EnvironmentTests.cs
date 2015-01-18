using System;
using System.IO;
using NUnit.Framework;

namespace LightningDB.Tests
{
    [TestFixture]
    public class EnvironmentTests
    {
        private string _path,  _pathCopy;
        private LightningEnvironment _env;

        public EnvironmentTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            var dir = Path.GetDirectoryName(location);

            _path = Path.Combine(dir, "TestDb" + Guid.NewGuid().ToString());
            _pathCopy = _path + "Copy";
        }

        [SetUp]
        public void Init()
        {
            Directory.CreateDirectory(_path);
            Directory.CreateDirectory(_pathCopy);
        }

        [TearDown]
        public void Cleanup()
        {
            if (_env != null && _env.IsOpened)
                _env.Close();

            _env = null;

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);

            if (Directory.Exists(_pathCopy))
                Directory.Delete(_pathCopy, true);
        }

        [Test]
        public void EnvironmentShouldBeCreatedIfWithoutFlags()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //assert
        }

        [Test]
        public void EnvironmentShouldBeCreatedIfReadOnly()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.ReadOnly);

            //assert
        }

        [Test]
        public void EnvironmentShouldBeOpened()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //act
            _env.Open();

            //assert
            Assert.AreEqual(true, _env.IsOpened);
        }

        [Test]
        public void EnvironmentShouldBeClosed()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(false, _env.IsOpened);
        }

        [Test]
        public void EnvironmentShouldBeCopied()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open(); 

            //act
            _env.CopyTo(_pathCopy);

            //assert
            if (Directory.GetFiles(_pathCopy).Length == 0)
                Assert.Fail("Copied files doesn't exist");
        }

        [Test]
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

        [Test]
        public void CanCountEnvironmentEntries()
        {
            const int entriesCount = 10;

            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(null, DatabaseOpenFlags.None))
            {
                for (var i = 0; i < entriesCount; i++)
                    txn.Put(db, i.ToString(), i.ToString());

                txn.Commit();
            }

            //act
            var count = _env.EntriesCount;

            //assert;
            Assert.AreEqual(entriesCount, count);
        }

        [Test]
        public void CanGetUsedSize()
        {
            const int entriesCount = 1;

            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            var initialUsedSize = _env.UsedSize;

            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(null, DatabaseOpenFlags.None))
            {
                for (int i = 0; i < entriesCount; i++)
                    txn.Put(db, i, i);

                txn.Commit();
            }

            //act
            var sizeDelta = _env.UsedSize - initialUsedSize;

            //act-assert;
            Assert.AreEqual(_env.PageSize, sizeDelta);
        }

    }
}
