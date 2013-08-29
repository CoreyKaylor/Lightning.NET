using System;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace LightningDB.Tests
{
    //TODO: Refactor these tests. 
    // Most of them are incorrect because test both input and retrival logic
    // at the same time.
    [TestFixture]
    public class DatabaseIOTests
    {
        private string _path;
        private LightningEnvironment _env;
        private LightningTransaction _txn;
        private LightningDatabase _db;

        public DatabaseIOTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            _path = Path.Combine(Path.GetDirectoryName(location), "TestDb");
        }

        [SetUp]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            _txn = _env.BeginTransaction();
            _db = _txn.OpenDatabase();
        }

        [TearDown]
        public void Cleanup()
        {
            _env.Close();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }

        [Test]
        public void DatabasePutShouldNotThrowExceptions()
        {
            var key = "key";
            var value = 25;
            //arrange

            //act
            _db.Put(key, value);

            //assert
        }

        [Test]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            var key = "key";
            //arrange

            //act
            _db.Get(key);

            //assert
        }

        [Test]
        public void DatabaseInsertedValueShouldBeRetrivedThen()
        {
            //arrange
            var key = "key";
            var value = "value";
            _db.Put(key, value);

            //act
            var persistedValue = _db.Get(key);
            
            //assert
            Assert.AreEqual(persistedValue, value);
        }

        [Test]
        public void DatabaseDeleteShouldRemoveItem()
        {
            //arrange
            var key = "key";
            var value = "value";
            _db.Put(key, value);

            //act
            _db.Delete(key);

            //assert
            Assert.IsNull(_db.Get(key));
        }

        [Test]
        public void GetByOperationCanMixTypesWithGenerics()
        {
            var key = "key";
            var value = 25;

            _db.Put(key, value);

            var persistedValue = _db.GetBy(key).Value<int>();

            Assert.AreEqual(value, persistedValue);
        }
    }
}
