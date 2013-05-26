using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LightningDB.Tests
{
    //TODO: Refactor these tests. 
    // Most of them are incorrect because test both input and retrival logic
    // at the same time.
    [TestClass]
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

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            _txn = _env.BeginTransaction();
            _db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _env.Close();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }

        [TestMethod]
        public void DatabasePutShouldNotThrowExceptions()
        {
            var key = Encoding.UTF8.GetBytes("key");
            var value = BitConverter.GetBytes(25);
            //arrange

            //act
            _db.Put(key, value, PutOptions.None);

            //assert
        }

        [TestMethod]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            var key = Encoding.UTF8.GetBytes("key");
            //arrange

            //act
            _db.Get(key);

            //assert
        }

        [TestMethod]
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

        [TestMethod]
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
