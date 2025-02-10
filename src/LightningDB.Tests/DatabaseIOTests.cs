using Shouldly;

namespace LightningDB.Tests;

public class DatabaseIOTests : TestBase
{
    private readonly LightningEnvironment _env;
    private readonly LightningTransaction _txn;
    private readonly LightningDatabase _db;

    public DatabaseIOTests()
    {
        _env = CreateEnvironment(TempPath(), new EnvironmentConfiguration { MaxDatabases = 2});
        _env.Open();

        _txn = _env.BeginTransaction();
        _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
    }

    [After(Test)]
    public void CleanupDb()
    {
        _txn.Dispose();
        _db.Dispose();
        _env.Dispose();
    }

    [Test]
    public void DatabasePutShouldNotThrowExceptions()
    {
        const string key = "key";
        const string value = "value";

        _txn.Put(_db, key, value);
    }

    [Test]
    public void DatabaseGetShouldNotThrowExceptions()
    {
        _txn.Get(_db, "key"u8.ToArray());
    }

    [Test]
    public void DatabaseInsertedValueShouldBeRetrievedThen()
    {
        const string key = "key";
        const string value = "value";
        _txn.Put(_db, key, value);

        var persistedValue = _txn.Get(_db, key);
            
        persistedValue.ShouldBe(value);
    }

    [Test]
    public void DatabaseDeleteShouldRemoveItem()
    {
        const string key = "key";
        const string value = "value";
        _txn.Put(_db, key, value);

        _txn.Delete(_db, key);

        _txn.ContainsKey(_db, key).ShouldBeFalse();
    }

    [Test]
    public void DatabaseDeleteShouldRemoveAllDuplicateDataItems()
    {
        using var env = CreateEnvironment(TempPath(), new EnvironmentConfiguration { MapSize = 1024 * 1024 });
        env.Open();
        using var tx = env.BeginTransaction();
        using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.DuplicatesSort});
        const string key = "key";
        const string value1 = "value1";
        const string value2 = "value2";

        tx.Put(db, key, value1);
        tx.Put(db, key, value2);

        tx.Delete(db, key);
        tx.ContainsKey(db, key).ShouldBeFalse();
    }

    [Test]
    public void ContainsKeyShouldReturnTrueIfKeyExists()
    {
        const string key = "key";
        const string value = "value";

        _txn.Put(_db, key, value);

        var exists = _txn.ContainsKey(_db, key);

        exists.ShouldBeTrue();
    }

    [Test]
    public void ContainsKeyShouldReturnFalseIfKeyNotExists()
    {
        const string key = "key";

        var exists = _txn.ContainsKey(_db, key);

        exists.ShouldBeFalse();
    }

    [Test]
    public void TryGetShouldReturnValueIfKeyExists()
    {
        const string key = "key";
        const string value = "value";

        _txn.Put(_db, key, value);

        var exists = _txn.TryGet(_db, key, out var persistedValue);

        exists.ShouldBeTrue();
        persistedValue.ShouldBe(value);
    }

    [Test]
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
                value.ShouldBe("value");
            }
        }
    }
}