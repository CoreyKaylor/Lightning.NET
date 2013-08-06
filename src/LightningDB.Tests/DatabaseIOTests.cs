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
            _db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);
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
            var key = Encoding.UTF8.GetBytes("key");
            var value = BitConverter.GetBytes(25);
            //arrange

            //act
            _db.Put(key, value, PutOptions.None);

            //assert
        }

        [Test]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            var key = Encoding.UTF8.GetBytes("key");
            //arrange

            //act
            _db.Get(key);

            //assert
        }

        [Test]
        public void DatabaseInsertedValueShouldBeRetrivedThen()
        {
            //arrange
            var key = Encoding.UTF8.GetBytes("key");
            var value = Encoding.UTF8.GetBytes("value");
            _db.Put(key, value, PutOptions.None);

            //act
            var valueBytes = _db.Get(key);
            
            //assert
            Assert.AreEqual("value", Encoding.UTF8.GetString(valueBytes));
        }

        [Test]
        public void DatabaseDeleteShouldRemoveItem()
        {
            //arrange
            var key = Encoding.UTF8.GetBytes("key");
            var value = Encoding.UTF8.GetBytes("value");
            _db.Put(key, value, PutOptions.None);

            //act
            _db.Delete(key, null);

            //assert
            Assert.IsNull(_db.Get(key));
        }
    }
}
