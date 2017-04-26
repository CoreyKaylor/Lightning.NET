using System;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests
{
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
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
        }

        public void Dispose()
        {
            _env.Dispose();
        }

        [Fact]
        public void DatabasePutShouldNotThrowExceptions()
        {
            var key = "key";
            var value = "value";

            _txn.Put(_db, key, value);
        }

        [Fact]
        public void DatabaseGetShouldNotThrowExceptions()
        {
            _txn.Get(_db, UTF8.GetBytes("key"));
        }

        [Fact]
        public void DatabaseInsertedValueShouldBeRetrivedThen()
        {
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            var persistedValue = _txn.Get(_db, key);
            
            Assert.Equal(persistedValue, value);
        }

        [Fact]
        public void DatabaseDeleteShouldRemoveItem()
        {
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            _txn.Delete(_db, key);

            Assert.False(_txn.ContainsKey(_db, key));
        }

        [Fact]
        public void DatabaseDeleteShouldRemoveAllDuplicateDataItems()
        {
            var fs = new SharedFileSystem();
            using (var env = new LightningEnvironment(fs.CreateNewDirectoryForTest(), configuration: new EnvironmentConfiguration {MapSize = 1024 * 1024, MaxDatabases = 1}))
            {
                env.Open();
                using (var tx = env.BeginTransaction())
                using (var db = tx.OpenDatabase(configuration: new DatabaseConfiguration() { Flags = DatabaseOpenFlags.DuplicatesSort}))
                {
                    var key = "key";
                    var value1 = "value1";
                    var value2 = "value2";

                    tx.Put(db, key, value1);
                    tx.Put(db, key, value2);

                    tx.Delete(db, key);
                    Assert.False(tx.ContainsKey(db, key));
                }
            }
        }

        [Fact]
        public void ContainsKeyShouldReturnTrueIfKeyExists()
        {
            var key = "key";
            var value = "value";

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
            var value = "value";

            _txn.Put(_db, key, value);

            string persistedValue;
            var exists = _txn.TryGet(_db, key, out persistedValue);

            Assert.True(exists);
            Assert.Equal(value, persistedValue);
        }

        [Fact]
        public void CanCommitTransactionToNamedDatabase()
        {
            using (var db = _txn.OpenDatabase("test", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                _txn.Put(db, "key1", "value");

                _txn.Commit();
            }

            using (var txn2 = _env.BeginTransaction())
            {
                using (var db = txn2.OpenDatabase("test"))
                {
                    var value = txn2.Get(db, "key1");
                    Assert.Equal("value", value);
                }
            }
        }
    }
}
