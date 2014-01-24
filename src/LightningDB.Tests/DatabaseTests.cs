using System.IO;
using NUnit.Framework;

namespace LightningDB.Tests
{
    [TestFixture]
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

        [SetUp]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
        }

        [TearDown]
        public void Cleanup()
        {
            _env.Close();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
        
        [Test]
        public void DatabaseShouldBeCreated()
        {
            var dbName = "test";
            _env.MaxDatabases = 2;
            _env.Open();
            _txn = _env.BeginTransaction();
            //arrange

            //act
            _txn.OpenDatabase(dbName, DatabaseOpenFlags.Create);

            //assert
        }

        [Test]
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

        [Test]
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

        [Test]
        public void DatabaseFromCommitedTransactionShouldBeAccessable()
        {
            //arrange
            _env.Open();

            LightningDatabase db;
            using (var committed = _env.BeginTransaction())
            {
                db = committed.OpenDatabase(null, DatabaseOpenFlags.None);
                committed.Commit();
            }
            
            //act
            try
            {
                _txn = _env.BeginTransaction();
                _txn.Put(db, "key", 1);
            }
            finally
            {
                db.Close();
            }

            //assert
            
        }
    }
}
