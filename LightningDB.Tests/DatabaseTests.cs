using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LightningDB.Tests
{
    [TestClass]
    public class DatabaseTests
    {
        private string _path;
        private LightningEnvironment _env;
        private LightningTransaction _txn;

        public DatabaseTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            _path = Path.Combine(Path.GetDirectoryName(location), "TestDb");
        }

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _env.Close();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
        
        [TestMethod]
        public void DatabaseShouldBeCreated()
        {
            var dbName = "test";
            _env.MapDatabases = 2;
            _env.Open();
            _txn = _env.BeginTransaction();
            //arrange

            //act
            _txn.OpenDatabase(dbName, DatabaseOpenFlags.Create);

            //assert
        }

        [TestMethod]
        public void DefaultDatabaseShouldBeOpened()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            //arrange

            //act
            var db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);

            //assert
            Assert.AreEqual(true, db.IsOpened);
        }

        [TestMethod]
        public void DefaultDatabaseShouldBeClosed()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);
            //arrange

            //act
            db.Close();

            //assert
            Assert.AreEqual(false, db.IsOpened);
        }

        [TestMethod]
        public void DefaultDatabaseShouldBeDropped()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);
            //arrange

            //act
            db.DropDatabase(true);

            //assert
            Assert.AreEqual(false, db.IsOpened);
        }
    }
}
