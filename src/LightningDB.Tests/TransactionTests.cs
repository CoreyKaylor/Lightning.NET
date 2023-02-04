﻿using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using System.Runtime.InteropServices;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class TransactionTests : IDisposable
{
    private readonly LightningEnvironment _env;

    public TransactionTests(SharedFileSystem fileSystem)
    {
        var path = fileSystem.CreateNewDirectoryForTest();
        _env = new LightningEnvironment(path);
        _env.Open();
    }

    public void Dispose()
    {
        _env.Dispose();
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
            Assert.Equal(LightningTransactionState.Active, tx.State);
        });
    }

    [Fact]
    public void TransactionShouldBeAbortedIfEnvironmentCloses()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            _env.Dispose();
            Assert.Equal(LightningTransactionState.Aborted, tx.State);
        });
    }

    [Fact]
    public void TransactionShouldChangeStateOnCommit()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Commit();
            Assert.Equal(LightningTransactionState.Committed, tx.State);
        });
    }

    [Fact]
    public void ChildTransactionShouldBeCreated()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            var subTxn = tx.BeginTransaction();
            Assert.Equal(LightningTransactionState.Active, subTxn.State);
        });
    }

    [Fact]
    public void ResetTransactionAbortedOnDispose()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            tx.Reset();
            tx.Dispose();
            Assert.Equal(LightningTransactionState.Aborted, tx.State);
        }, transactionFlags: TransactionBeginFlags.ReadOnly);
    }

    [Fact]
    public void ChildTransactionShouldBeAbortedIfParentIsAborted()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            var child = tx.BeginTransaction();
            tx.Abort();
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        });
    }

    [Fact]
    public void ChildTransactionShouldBeAbortedIfParentIsCommitted()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            var child = tx.BeginTransaction();
            tx.Commit();
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        });
    }

    [Fact]
    public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
    {
        _env.RunTransactionScenario((tx, _) =>
        {
            var child = tx.BeginTransaction();
            _env.Dispose();
            Assert.Equal(LightningTransactionState.Aborted, child.State);
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
            Assert.Equal(LightningTransactionState.Active, tx.State);
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
        using (var db1 = txnT.OpenDatabase(configuration: options))
        {
            txnT.DropDatabase(db1);
            txnT.Commit();
        }

        var txn = _env.BeginTransaction();
        var db = txn.OpenDatabase(configuration: options);

        var keysUnsorted = Enumerable.Range(1, 10000).OrderBy(_ => Guid.NewGuid()).ToList();
        var keysSorted = keysUnsorted.ToArray();
        Array.Sort(keysSorted, new Comparison<int>((Func<int, int, int>)Comparison));

        GC.Collect();
        for (var i = 0; i < keysUnsorted.Count; i++)
            txn.Put(db, BitConverter.GetBytes(keysUnsorted[i]), BitConverter.GetBytes(i));

        using (var c = txn.CreateCursor(db))
        {
            var order = 0;
            while (c.Next() == MDBResultCode.Success)
                Assert.Equal(keysSorted[order++], BitConverter.ToInt32(c.GetCurrent().key.CopyToNewArray(), 0));
        }
    }

    [Fact]
    public void TransactionShouldSupportCustomDupSorter()
    {
        int Comparison(int l, int r) => -Math.Sign(l - r);

        var txn = _env.BeginTransaction();
        var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed};
        int CompareWith(MDBValue l, MDBValue r) => Comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
        options.FindDuplicatesWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>((Func<MDBValue, MDBValue, int>)CompareWith)));
        var db = txn.OpenDatabase(configuration: options);

        var valuesUnsorted = new [] { 2, 10, 5, 0 };
        var valuesSorted = valuesUnsorted.ToArray();
        Array.Sort(valuesSorted, new Comparison<int>((Func<int, int, int>)Comparison));

        using (var c = txn.CreateCursor(db))
            c.Put(BitConverter.GetBytes(123), valuesUnsorted.Select(BitConverter.GetBytes).ToArray());

        using (var c = txn.CreateCursor(db))
        {
            var order = 0;

            while (c.Next() == MDBResultCode.Success)
                Assert.Equal(valuesSorted[order++], BitConverter.ToInt32(c.GetCurrent().value.CopyToNewArray(), 0));
        }
    }
}