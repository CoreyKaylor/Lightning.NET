using System;
using System.Linq;
using System.Collections.Generic;
using Shouldly;
using System.Runtime.InteropServices;

namespace LightningDB.Tests;

public class TransactionTests : TestBase
{
    public void CanDeletePreviouslyCommittedWithMultipleValuesByPassingNullForValue()
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

    public void TransactionShouldBeCreated()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.State.ShouldBe(LightningTransactionState.Ready);
        });
    }

    public void TransactionShouldChangeStateOnCommit()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Commit();
            tx.State.ShouldBe(LightningTransactionState.Done);
        });
    }

    public void ChildTransactionShouldBeCreated()
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

    public void ResetTransactionAbortedOnDispose()
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

    public void ChildTransactionShouldBeAbortedIfParentIsAborted()
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
    
    public void TryGetShouldVerifyFindingAndNotFindingValues()
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
    
    public void TryGetWithKeyAndValueShouldBeFound()
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

    public void ChildTransactionShouldBeAbortedIfParentIsCommitted()
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


    public void ReadOnlyTransactionShouldChangeStateOnReset()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.State.ShouldBe(LightningTransactionState.Reset);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    public void ReadOnlyTransactionShouldChangeStateOnRenew()
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

    public void CanCountTransactionEntries()
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

    public void CanGetDatabaseStatistics()
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

    public void TransactionShouldSupportCustomComparer()
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

    public void TransactionShouldSupportCustomDupSorter()
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
    public void DatabaseShouldBeEmptyAfterTruncate()
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
}