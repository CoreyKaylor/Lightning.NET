using Shouldly;

namespace LightningDB.Tests;

public class DatabaseIOTests : TestBase
{
    public void DatabasePutShouldNotThrowExceptions()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value = "value";

            tx.Put(db, key, value);
        });
    }

    public void DatabaseGetShouldNotThrowExceptions()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            tx.Get(db, "key"u8.ToArray());
        });
    }

    public void DatabaseInsertedValueShouldBeRetrievedThen()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value = "value";
            tx.Put(db, key, value);

            var persistedValue = tx.Get(db, key);

            persistedValue.ShouldBe(value);
        });
    }

    public void DatabaseDeleteShouldRemoveItem()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value = "value";
            tx.Put(db, key, value);

            tx.Delete(db, key);

            tx.ContainsKey(db, key).ShouldBeFalse();
        });
    }

    public void DatabaseDeleteShouldRemoveAllDuplicateDataItems()
    {
        using var env = CreateEnvironment(TempPath(), new EnvironmentConfiguration { MapSize = 1024 * 1024 });
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value1 = "value1";
            const string value2 = "value2";

            tx.Put(db, key, value1);
            tx.Put(db, key, value2);

            tx.Delete(db, key);
            tx.ContainsKey(db, key).ShouldBeFalse();
        }, DatabaseOpenFlags.DuplicatesSort);
    }

    public void ContainsKeyShouldReturnTrueIfKeyExists()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value = "value";

            tx.Put(db, key, value);

            var exists = tx.ContainsKey(db, key);

            exists.ShouldBeTrue();
        });
    }

    public void ContainsKeyShouldReturnFalseIfKeyNotExists()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            var exists = tx.ContainsKey(db, key);

            exists.ShouldBeFalse();
        });
    }

    public void TryGetShouldReturnValueIfKeyExists()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const string key = "key";
            const string value = "value";

            tx.Put(db, key, value);

            var exists = tx.TryGet(db, key, out var persistedValue);

            exists.ShouldBeTrue();
            persistedValue.ShouldBe(value);
        });
    }

    public void CanCommitTransactionToNamedDatabase()
    {
        using var env = CreateEnvironment(TempPath(), new EnvironmentConfiguration{MaxDatabases = 2});
        env.Open();
        using var tx = env.BeginTransaction();
        using var db = tx.OpenDatabase("test", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
        tx.Put(db, "key1", "value");
        tx.Commit();

        using var txn2 = env.BeginTransaction();
        var value = txn2.Get(db, "key1");
        value.ShouldBe("value");
    }
}