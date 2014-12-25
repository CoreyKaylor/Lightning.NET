using System.IO;
using NUnit.Framework;

namespace LMDB.Tests
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
            _txn.Put(_db, key, value);

            //assert
        }

        [Test]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            var key = "key";
            //arrange

            //act
            _txn.Get(_db, key);

            //assert
        }

        [Test]
        public void DatabaseInsertedValueShouldBeRetrivedThen()
        {
            //arrange
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            //act
            var persistedValue = _txn.Get(_db, key);
            
            //assert
            Assert.AreEqual(persistedValue, value);
        }

        [Test]
        public void DatabaseDeleteShouldRemoveItem()
        {
            //arrange
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            //act
            _txn.Delete(_db, key);

            //assert
            Assert.IsFalse(_txn.ContainsKey(_db, key));
        }

        [Test]
        public void GetByOperationCanMixTypesWithGenerics()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            var persistedValue = _txn.GetBy(_db, key).Value<int>();

            Assert.AreEqual(value, persistedValue);
        }

        [Test]
        public void ContainsKeyShouldReturnTrueIfKeyExists()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            var exists = _txn.ContainsKey(_db, key);

            Assert.IsTrue(exists);
        }

        [Test]
        public void ContainsKeyShouldReturnFalseIfKeyNotExists()
        {
            var key = "key";

            var exists = _txn.ContainsKey(_db, key);

            Assert.IsFalse(exists);
        }

        [Test]
        public void TryGetShouldReturnValueIfKeyExists()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            int persistedValue;
            var exists = _txn.TryGet(_db, key, out persistedValue);

            Assert.IsTrue(exists);
            Assert.AreEqual(value, persistedValue);
        }
    }
}
