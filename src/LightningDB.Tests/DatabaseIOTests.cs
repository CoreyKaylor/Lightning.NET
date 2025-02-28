using Shouldly;

namespace LightningDB.Tests;

public class DatabaseIOTests : TestBase
{
    public void database_put_should_not_throw_exceptions()
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

    public void database_get_should_not_throw_exceptions()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            tx.Get(db, "key"u8.ToArray());
        });
    }

    public void database_inserted_value_should_be_retrieved_then()
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

    public void database_delete_should_remove_item()
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

    public void database_delete_should_remove_all_duplicate_data_items()
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

    public void contains_key_should_return_true_if_key_exists()
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

    public void contains_key_should_return_false_if_key_not_exists()
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

    public void try_get_should_return_value_if_key_exists()
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

    public void can_commit_transaction_to_named_database()
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