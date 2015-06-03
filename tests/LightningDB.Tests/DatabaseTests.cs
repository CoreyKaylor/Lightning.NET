using System;
using Xunit;

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
        }

        public void Dispose()
        {
            _env.Close();
        }
        
        [Fact]
        public void DatabaseShouldBeCreated()
        {
            var dbName = "test";
            _env.MaxDatabases = 2;
            _env.Open();
            _txn = _env.BeginTransaction();
            //arrange

            //act
            _txn.OpenDatabase(dbName, new DatabaseOptions { Flags = DatabaseOpenFlags.Create });

            //assert
        }

        [Fact]
        public void DefaultDatabaseShouldBeOpened()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            //arrange

            //act
            var db = _txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None });

            //assert
            Assert.Equal(true, db.IsOpened);
        }

        [Fact]
        public void DefaultDatabaseShouldBeClosed()
        {
            _env.Open();
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None });
            //arrange

            //act
            db.Close();

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
                db = committed.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None });
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
