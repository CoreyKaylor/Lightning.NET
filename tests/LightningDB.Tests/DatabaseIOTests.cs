using System;
using Xunit;

namespace LightningDB.Tests
{
    //TODO: Refactor these tests. 
    // Most of them are incorrect because test both input and retrival logic
    // at the same time.
    [Collection("SharedFileSystem")]
    public class DatabaseIOTests : IDisposable
    {
        private LightningEnvironment _env;
        private LightningTransaction _txn;
        private LightningDatabase _db;

        public DatabaseIOTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();

            _env = new LightningEnvironment(path);
            _env.MaxDatabases = 2;
            _env.Open();

            _txn = _env.BeginTransaction();
            _db = _txn.OpenDatabase();
        }

        public void Dispose()
        {
            _env.Dispose();
        }

        [Fact]
        public void DatabasePutShouldNotThrowExceptions()
        {
            var key = "key";
            var value = 25;
            //arrange

            //act
            _txn.Put(_db, key, value);

            //assert
        }

        [Fact]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            var key = "key";
            //arrange

            //act
            _txn.Get(_db, key);

            //assert
        }

        [Fact]
        public void DatabaseInsertedValueShouldBeRetrivedThen()
        {
            //arrange
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            //act
            var persistedValue = _txn.Get(_db, key);
            
            //assert
            Assert.Equal(persistedValue, value);
        }

        [Fact]
        public void DatabaseDeleteShouldRemoveItem()
        {
            //arrange
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            //act
            _txn.Delete(_db, key);

            //assert
            Assert.False(_txn.ContainsKey(_db, key));
        }

        [Fact]
        public void GetByOperationCanMixTypesWithGenerics()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            var persistedValue = _txn.GetBy(_db, key).Value<int>();

            Assert.Equal(value, persistedValue);
        }

        [Fact]
        public void ContainsKeyShouldReturnTrueIfKeyExists()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            var exists = _txn.ContainsKey(_db, key);

            Assert.True(exists);
        }

        [Fact]
        public void ContainsKeyShouldReturnFalseIfKeyNotExists()
        {
            var key = "key";

            var exists = _txn.ContainsKey(_db, key);

            Assert.False(exists);
        }

        [Fact]
        public void TryGetShouldReturnValueIfKeyExists()
        {
            var key = "key";
            var value = 25;

            _txn.Put(_db, key, value);

            int persistedValue;
            var exists = _txn.TryGet(_db, key, out persistedValue);

            Assert.True(exists);
            Assert.Equal(value, persistedValue);
        }

        [Theory]
        [InlineData(null)] 
        [InlineData("test")]
        public void CanCommitTransactionToNamedDatabase(string dbName)
        {
            using (var db = _txn.OpenDatabase(dbName, new DatabaseOptions { Flags = DatabaseOpenFlags.Create }))
            {
                _txn.Put(db, "key1", "value");

                _txn.Commit();
            }

            using (var txn2 = _env.BeginTransaction())
            {
                using (var db = txn2.OpenDatabase(dbName))
                {
                    var value = txn2.Get<string, string>(db, "key1");
                    Assert.Equal("value", value);
                }
            }
        }
    }
}
