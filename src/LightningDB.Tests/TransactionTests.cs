using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using System.Runtime.InteropServices;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class TransactionTests : TestBase
{
    public TransactionTests(SharedFileSystem fileSystem) : base(fileSystem)
    {
        _env.Open();
    }

    [Fact]
    public void CanDeletePreviouslyCommittedWithMultipleValuesByPassingNullForValue()
    {
        _env.RunTransactionScenario((tx, db) =>
        {
            var key = MemoryMarshal.Cast<char, byte>("abcd");

            tx.Put(db, key, MemoryMarshal.Cast<char, byte>("Value1"));
            tx.Put(db, key, MemoryMarshal.Cast<char, byte>("Value2"), PutOptions.AppendData);
            tx.Commit();
            tx.Dispose();
                
            using var delTxn = _env.BeginTransaction();
            var result = delTxn.Delete(db, key, null);
            Assert.Equal(MDBResultCode.Success, result);
            result = delTxn.Commit();
            Assert.Equal(MDBResultCode.Success, result);
        }, DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed);
    }

    [Fact]
    public void TransactionShouldBeCreated()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            Assert.Equal(LightningTransactionState.Ready, tx.State);
        });
    }

    [Fact]
    public void TransactionShouldChangeStateOnCommit()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Commit();
            Assert.Equal(LightningTransactionState.Done, tx.State);
        });
    }

    [Fact]
    public void ChildTransactionShouldBeCreated()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            using var subTxn = tx.BeginTransaction();
            Assert.Equal(LightningTransactionState.Ready, subTxn.State);
            Assert.Same(subTxn.ParentTransaction, tx);
        });
    }

    [Fact]
    public void ResetTransactionAbortedOnDispose()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.Dispose();
            Assert.Equal(LightningTransactionState.Released, tx.State);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    [Fact]
    public void ChildTransactionShouldBeAbortedIfParentIsAborted()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            using var child = tx.BeginTransaction();
            tx.Abort();
            var result = child.Commit();
            Assert.Equal(MDBResultCode.BadTxn, result);
        });
    }

    [Fact]
    public void ChildTransactionShouldBeAbortedIfParentIsCommitted()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            using var child = tx.BeginTransaction();
            tx.Commit();
            var result = child.Commit();
            Assert.Equal(MDBResultCode.BadTxn, result);
        });
    }


    [Fact]
    public void ReadOnlyTransactionShouldChangeStateOnReset()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            Assert.Equal(LightningTransactionState.Reset, tx.State);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    [Fact]
    public void ReadOnlyTransactionShouldChangeStateOnRenew()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.Renew();
            Assert.Equal(LightningTransactionState.Ready, tx.State);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    [Fact]
    public void CanCountTransactionEntries()
    {
        _env.RunTransactionScenario((tx, db) =>
        {
            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                tx.Put(db, i.ToString(), i.ToString());

            var count = tx.GetEntriesCount(db);
            Assert.Equal(entriesCount, count);
        });
    }

    [Fact]
    public void TransactionShouldSupportCustomComparer()
    {
        int Comparison(int l, int r) => l.CompareTo(r);
        var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create};
        int CompareWith(MDBValue l, MDBValue r) => Comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
        options.CompareWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>((Func<MDBValue, MDBValue, int>)CompareWith)));

        using (var txnT = _env.BeginTransaction())
        using (var db1 = txnT.OpenDatabase(configuration: options, closeOnDispose: true))
        {
            txnT.DropDatabase(db1);
            txnT.Commit();
        }

        using var txn = _env.BeginTransaction();
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
                Assert.Equal(keysSorted[order++], BitConverter.ToInt32(result.Item2.CopyToNewArray(), 0));
        }
    }

    [Fact]
    public void TransactionShouldSupportCustomDupSorter()
    {
        int Comparison(int l, int r) => -Math.Sign(l - r);

        using var txn = _env.BeginTransaction();
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
                Assert.Equal(valuesSorted[order++], BitConverter.ToInt32(result.Item3.CopyToNewArray(), 0));
        }
    }
}