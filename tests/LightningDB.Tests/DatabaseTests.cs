using System;
using System.Text;
using Xunit;
using LightningDB.Converters;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class DatabaseTests : IDisposable
    {
        private LightningEnvironment _env;
        private LightningTransaction _txn;

        public DatabaseTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new LightningEnvironment(path);
            _env.WithConverters();
            _env.MaxDatabases = 2;
        }

        public void Dispose()
        {
            _env.Dispose();
        }
        
        [Fact]
        public void DatabaseShouldBeCreated()
        {
            var dbName = "test";
            _env.MaxDatabases = 2;
            _env.Open();
            using (var txn = _env.BeginTransaction())
            using (txn.OpenDatabase(dbName, new DatabaseOptions { Flags = DatabaseOpenFlags.Create }))
            {
                txn.Commit();
            }
            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(dbName, new DatabaseOptions { Flags = DatabaseOpenFlags.None }))
            {
                Assert.False(db.IsReleased);
                txn.Commit();
            }
        }

        [Fact]
        public void DatabaseShouldBeClosed()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase("master", new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            //arrange

            //act
            db.Dispose();

            //assert
            Assert.Equal(false, db.IsOpened);
        }

        [Fact]
        public void DatabaseFromCommitedTransactionShouldBeAccessable()
        {
            //arrange
            _env.Open();

            LightningDatabase db;
            using (var committed = _env.BeginTransaction())
            {
                db = committed.OpenDatabase("master", new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
                committed.Commit();
            }
            
            //act
            using (db)
            using (var txn = _env.BeginTransaction())
            {
                txn.Put(db, "key", 1);
                txn.Commit();
            }
        }

        [Fact]
        public void DatabaseShouldBeDropped()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase("notmaster", new DatabaseOptions {Flags = DatabaseOpenFlags.Create});
            _txn.Commit();
            _txn.Dispose();
            db.Dispose();

            _txn = _env.BeginTransaction();
            db = _txn.OpenDatabase("notmaster");

            db.Drop();
            _txn.Commit();
            _txn.Dispose();

            _txn = _env.BeginTransaction();

            var ex = Assert.Throws<LightningException>(() => _txn.OpenDatabase("notmaster"));

            Assert.Equal(ex.StatusCode, -30798);
        }

        [Fact]
        public void TruncatingTheDatabase()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase("master", new DatabaseOptions {Flags = DatabaseOpenFlags.Create});

            _txn.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
            _txn.Commit();
            _txn.Dispose();
            _txn = _env.BeginTransaction();
            db = _txn.OpenDatabase("master");
            db.Truncate();
            _txn.Commit();
            _txn.Dispose();
            _txn = _env.BeginTransaction();
            var result = _txn.Get(db, Encoding.UTF8.GetBytes("hello"));

            Assert.Null(result);
        }
    }
}
