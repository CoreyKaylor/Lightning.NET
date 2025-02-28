using System;
using System.Linq;
using System.Collections.Generic;
using Shouldly;
using System.Runtime.InteropServices;

namespace LightningDB.Tests;

public class TransactionTests : TestBase
{
    public void can_delete_previously_committed_with_multiple_values_by_passing_null_for_value()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("abcd");

            tx.Put(db, key, MemoryMarshal.Cast<char, byte>("Value1"));
            tx.Put(db, key, MemoryMarshal.Cast<char, byte>("Value2"), PutOptions.AppendData);
            tx.Commit();
            tx.Dispose();

            using var delTxn = env.BeginTransaction();
            var result = delTxn.Delete(db, key, null);
            result.ShouldBe(MDBResultCode.Success);
            result = delTxn.Commit();
            result.ShouldBe(MDBResultCode.Success);
        }, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed);
    }

    public void transaction_should_be_created()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.State.ShouldBe(LightningTransactionState.Ready);
        });
    }

    public void transaction_should_change_state_on_commit()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Commit();
            tx.State.ShouldBe(LightningTransactionState.Done);
        });
    }

    public void child_transaction_should_be_created()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            using var subTxn = tx.BeginTransaction();
            subTxn.State.ShouldBe(LightningTransactionState.Ready);
            tx.ShouldBeSameAs(subTxn.ParentTransaction);
        });
    }

    public void reset_transaction_aborted_on_dispose()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.Dispose();
            tx.State.ShouldBe(LightningTransactionState.Released);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    public void child_transaction_should_be_aborted_if_parent_is_aborted()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            using var child = tx.BeginTransaction();
            tx.Abort();
            var result = child.Commit();
            result.ShouldBe(MDBResultCode.BadTxn);
        });
    }

    public void try_get_should_verify_finding_and_not_finding_values()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("key1");
            var value = MemoryMarshal.Cast<char, byte>("value1");

            tx.Put(db, key, value);

            tx.TryGet(db, key.ToArray(), out var retrievedValue).ShouldBeTrue();
            retrievedValue.ShouldBe(value.ToArray());

            var missingKey = MemoryMarshal.Cast<char, byte>("key2");
            tx.TryGet(db, missingKey.ToArray(), out _).ShouldBeFalse();
        });
    }

    public void try_get_with_key_and_value_should_be_found()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("key3");
            var value = MemoryMarshal.Cast<char, byte>("value3");
            tx.Put(db, key, value);

            var resultBuffer = new byte[value.Length];
            tx.TryGet(db, key.ToArray(), resultBuffer).ShouldBeTrue();
            resultBuffer.ShouldBe(value.ToArray());
        });
    }

    public void child_transaction_should_be_aborted_if_parent_is_committed()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            using var child = tx.BeginTransaction();
            tx.Commit();
            var result = child.Commit();
            result.ShouldBe(MDBResultCode.BadTxn);
        });
    }


    public void read_only_transaction_should_change_state_on_reset()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.State.ShouldBe(LightningTransactionState.Reset);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    public void read_only_transaction_should_change_state_on_renew()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.Renew();
            tx.State.ShouldBe(LightningTransactionState.Ready);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    public void can_count_transaction_entries()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                tx.Put(db, i.ToString(), i.ToString());

            var count = tx.GetEntriesCount(db);
            count.ShouldBe(entriesCount);
        });
    }

    public void can_get_database_statistics()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((commitTx, db) =>
        {
            commitTx.Commit().ThrowOnError();

            Should.Throw<LightningException>(() => db.DatabaseStats);

            const int entriesCount = 5;
            using var tx = env.BeginTransaction();
            for (var i = 0; i < entriesCount; i++)
                tx.Put(db, i.ToString(), i.ToString()).ThrowOnError();

            var stats = tx.GetStats(db);
            stats.Entries.ShouldBe(entriesCount);
            stats.BranchPages.ShouldBe(0);
            stats.LeafPages.ShouldBe(1);
            stats.OverflowPages.ShouldBe(0);
            stats.PageSize.ShouldBe(env.EnvironmentStats.PageSize);
            stats.BTreeDepth.ShouldBe(1);
        });
    }

    public void transaction_should_support_custom_comparer()
    {
        int Comparison(int l, int r) => l.CompareTo(r);
        var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create};
        int CompareWith(MDBValue l, MDBValue r) => Comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
        options.CompareWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>((Func<MDBValue, MDBValue, int>)CompareWith)));

        using var env = CreateEnvironment();
        env.Open();
        using (var txnT = env.BeginTransaction())
        using (var db1 = txnT.OpenDatabase(configuration: options, closeOnDispose: true))
        {
            txnT.DropDatabase(db1);
            txnT.Commit();
        }

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: options, closeOnDispose: true);

        var keysUnsorted = Enumerable.Range(1, 10000).OrderBy(_ => Guid.NewGuid()).ToList();
        var keysSorted = keysUnsorted.ToArray();
        Array.Sort(keysSorted, new Comparison<int>((Func<int, int, int>)Comparison));

        GC.Collect();
        for (var i = 0; i < keysUnsorted.Count; i++)
            txn.Put(db, BitConverter.GetBytes(keysUnsorted[i]), BitConverter.GetBytes(i));

        using (var c = txn.CreateCursor(db))
        {
            var order = 0;
            (MDBResultCode, MDBValue, MDBValue) result;
            while ((result = c.Next()).Item1 == MDBResultCode.Success)
                BitConverter.ToInt32(result.Item2.CopyToNewArray()).ShouldBe(keysSorted[order++]);
        }
    }

    public void transaction_should_support_custom_dup_sorter()
    {
        int Comparison(int l, int r) => -Math.Sign(l - r);

        using var env = CreateEnvironment();
        env.Open();
        using var txn = env.BeginTransaction();
        var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed};
        int CompareWith(MDBValue l, MDBValue r) => Comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
        options.FindDuplicatesWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>((Func<MDBValue, MDBValue, int>)CompareWith)));
        using var db = txn.OpenDatabase(configuration: options, closeOnDispose: true);

        var valuesUnsorted = new [] { 2, 10, 5, 0 };
        var valuesSorted = valuesUnsorted.ToArray();
        Array.Sort(valuesSorted, new Comparison<int>((Func<int, int, int>)Comparison));

        using (var c = txn.CreateCursor(db))
            c.Put(BitConverter.GetBytes(123), valuesUnsorted.Select(BitConverter.GetBytes).ToArray());

        using (var c = txn.CreateCursor(db))
        {
            var order = 0;

            (MDBResultCode, MDBValue, MDBValue) result;
            while ((result = c.Next()).Item1 == MDBResultCode.Success)
                BitConverter.ToInt32(result.Item3.CopyToNewArray()).ShouldBe(valuesSorted[order++]);
        }
    }
    public void database_should_be_empty_after_truncate()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            // Insert several key-value pairs
            var key1 = MemoryMarshal.Cast<char, byte>("key1");
            var key2 = MemoryMarshal.Cast<char, byte>("key2");
            var value1 = MemoryMarshal.Cast<char, byte>("value1");
            var value2 = MemoryMarshal.Cast<char, byte>("value2");

            tx.Put(db, key1, value1);
            tx.Put(db, key2, value2);

            // Truncate the database
            tx.TruncateDatabase(db);

            // Verify the database is empty
            tx.ContainsKey(db, key1).ShouldBeFalse();
            tx.ContainsKey(db, key2).ShouldBeFalse();
        });
    }

    public void can_get_transaction_id()
    {
        using var env = CreateEnvironment();
        env.Open();

        env.RunTransactionScenario((tx, _) =>
        {
            tx.Id.ShouldBeGreaterThan(0);
        });
    }

    public void can_compare_key_values()
    {
        using var env = CreateEnvironment();
        env.Open();

        env.RunTransactionScenario((tx, db) =>
        {
            var key1 = MemoryMarshal.Cast<char, byte>("aaa");
            var key2 = MemoryMarshal.Cast<char, byte>("bbb");

            // Key1 should be less than Key2
            tx.CompareKeys(db, key1, key2).ShouldBeLessThan(0);

            // Key2 should be greater than Key1
            tx.CompareKeys(db, key2, key1).ShouldBeGreaterThan(0);

            // Same keys should be equal
            tx.CompareKeys(db, key1, key1).ShouldBe(0);
        });
    }

    public void can_compare_data_values()
    {
        using var env = CreateEnvironment();
        // Set MaxDatabases to allow creating the extra test database
        env.MaxDatabases = 10;
        env.Open();

        env.RunTransactionScenario((tx, db) =>
        {
            // Test with a database that supports duplicates for data comparison
            var dbConfig = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort };
            using var dupsDb = tx.OpenDatabase("dups_db", dbConfig);

            var data1 = MemoryMarshal.Cast<char, byte>("value1");
            var data2 = MemoryMarshal.Cast<char, byte>("value2");

            // Data1 should be less than Data2
            tx.CompareData(dupsDb, data1, data2).ShouldBeLessThan(0);

            // Data2 should be greater than Data1
            tx.CompareData(dupsDb, data2, data1).ShouldBeGreaterThan(0);

            // Same data values should be equal
            tx.CompareData(dupsDb, data1, data1).ShouldBe(0);
        });
    }

    public void should_use_no_overwrite_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("testKey");
            var value1 = MemoryMarshal.Cast<char, byte>("value1");
            var value2 = MemoryMarshal.Cast<char, byte>("value2");

            // First put succeeds
            var result = tx.Put(db, key, value1);
            result.ShouldBe(MDBResultCode.Success);

            // Second put with NoOverwrite should fail with KeyExist
            result = tx.Put(db, key, value2, PutOptions.NoOverwrite);
            result.ShouldBe(MDBResultCode.KeyExist);

            // Value should remain unchanged
            tx.TryGet(db, key.ToArray(), out var retrievedValue).ShouldBeTrue();
            retrievedValue.ShouldBe(value1.ToArray());
        });
    }

    public void should_use_no_duplicate_data_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("testKey");
            var value1 = MemoryMarshal.Cast<char, byte>("value1");
            var value2 = MemoryMarshal.Cast<char, byte>("value2");

            // Put first value
            var result = tx.Put(db, key, value1);
            result.ShouldBe(MDBResultCode.Success);

            // Put second value
            result = tx.Put(db, key, value2);
            result.ShouldBe(MDBResultCode.Success);

            // Try to put first value again with NoDuplicateData, should fail
            result = tx.Put(db, key, value1, PutOptions.NoDuplicateData);
            result.ShouldBe(MDBResultCode.KeyExist);

            // Different value should succeed
            var value3 = MemoryMarshal.Cast<char, byte>("value3");
            result = tx.Put(db, key, value3, PutOptions.NoDuplicateData);
            result.ShouldBe(MDBResultCode.Success);
        }, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);
    }

    public void should_use_append_data_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            // Create sorted keys
            var keys = Enumerable.Range(1, 5)
                .Select(i => MemoryMarshal.Cast<char, byte>($"key{i:D5}").ToArray())
                .ToArray();

            // Insert keys in order with AppendData option
            foreach (var key in keys)
            {
                var result = tx.Put(db, key, key, PutOptions.AppendData);
                result.ShouldBe(MDBResultCode.Success);
            }

            // Verify all keys were inserted correctly
            for (int i = 0; i < keys.Length; i++)
            {
                tx.TryGet(db, keys[i].ToArray(), out var value).ShouldBeTrue();
                value.ShouldBe(keys[i].ToArray());
            }

            // Inserting in wrong order should fail with KeyExist,
            var outOfOrderKey = MemoryMarshal.Cast<char, byte>("key00000");
            var putResult = tx.Put(db, outOfOrderKey, outOfOrderKey, PutOptions.AppendData);
            putResult.ShouldBe(MDBResultCode.KeyExist);
            var entriesCount = tx.GetEntriesCount(db);
            entriesCount.ShouldBeGreaterThanOrEqualTo(keys.Length);
        });
    }

    public void should_use_append_duplicate_data_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("testKey");

            // Create sorted values
            var values = Enumerable.Range(1, 5)
                .Select(i => MemoryMarshal.Cast<char, byte>($"value{i:D5}").ToArray())
                .ToArray();

            // Insert first value normally
            var result = tx.Put(db, key, values[0]);
            result.ShouldBe(MDBResultCode.Success);

            // Insert remaining values in order with AppendDuplicateData option
            for (int i = 1; i < values.Length; i++)
            {
                result = tx.Put(db, key, values[i], PutOptions.AppendDuplicateData);
                result.ShouldBe(MDBResultCode.Success);
            }

            // Check that all values are associated with the key
            using var cursor = tx.CreateCursor(db);
            cursor.Set(key.ToArray());
            var count = 0;
            do
            {
                var (resultCode, retrievedKey, retrievedValue) = cursor.GetCurrent();
                resultCode.ShouldBe(MDBResultCode.Success);
                count++;
            } while (cursor.NextDuplicate().resultCode == MDBResultCode.Success);

            count.ShouldBe(values.Length);
        }, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort);
    }

    public void should_handle_combined_put_options()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("testKey");
            var value1 = MemoryMarshal.Cast<char, byte>("value1");
            var value2 = MemoryMarshal.Cast<char, byte>("value2");

            // First put succeeds
            var result = tx.Put(db, key, value1);
            result.ShouldBe(MDBResultCode.Success);

            // Combined options: NoOverwrite | AppendData
            // Since NoOverwrite will prevent overwriting the key,
            // AppendData won't matter in this case
            result = tx.Put(db, key, value2, PutOptions.NoOverwrite | PutOptions.AppendData);
            result.ShouldBe(MDBResultCode.KeyExist);

            // Value should remain unchanged
            tx.TryGet(db, key.ToArray(), out var retrievedValue).ShouldBeTrue();
            retrievedValue.ShouldBe(value1.ToArray());
        });
    }

    public void should_handle_reserve_space_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("reserveKey");

            // Create a value with a specific size
            var valueSize = 128;
            var value = new byte[valueSize];
            for (int i = 0; i < valueSize; i++)
            {
                value[i] = (byte)(i % 256);
            }

            // In a real implementation with ReserveSpace, you'd get a pointer
            // to fill directly, but for testing we can just verify it works
            var result = tx.Put(db, key, value, PutOptions.ReserveSpace);

            // This option is mostly used for direct memory manipulation
            // which is difficult to test in a managed environment
            // We can at least verify the key exists
            tx.ContainsKey(db, key).ShouldBeTrue();
        });
    }
}
