using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using System.Runtime.InteropServices;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class TransactionDupFixedTests : IDisposable
    {
        private LightningEnvironment _env;

        public TransactionDupFixedTests(SharedFileSystem fileSystem)
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
        public void CanCountTransactionEntries()
        {
            LightningDatabase db;

            using (var openDbTxn = _env.BeginTransaction())
            {
                db = openDbTxn.OpenDatabase();
            }

            var key = MemoryMarshal.Cast<char, byte>("abcd");

            using(var writeTxn = _env.BeginTransaction())
            {
                writeTxn.Put(db, key, MemoryMarshal.Cast<char,byte>("Value1"));
                writeTxn.Put(db, key, MemoryMarshal.Cast<char, byte>("Value2"));
                writeTxn.Commit();
            }

            using (var delTxn = _env.BeginTransaction())
            {
                delTxn.Delete(db, key, null);//should not throw
                delTxn.Commit();
            }
        }
    }


    [Collection("SharedFileSystem")]
    public class TransactionTests : IDisposable
    {
        private LightningEnvironment _env;

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
        public void TransactionShouldBeCreated()
        {
            var txn = _env.BeginTransaction();

            Assert.Equal(LightningTransactionState.Active, txn.State);
        }

        [Fact]
        public void TransactionShouldBeAbortedIfEnvironmentCloses()
        {
            var txn = _env.BeginTransaction();

            _env.Dispose();

            Assert.Equal(LightningTransactionState.Aborted, txn.State);
        }

        [Fact]
        public void TransactionShouldChangeStateOnCommit()
        {
            var txn = _env.BeginTransaction();

            txn.Commit();

            Assert.Equal(LightningTransactionState.Commited, txn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeCreated()
        {
            var txn = _env.BeginTransaction();

            var subTxn = txn.BeginTransaction();

            Assert.Equal(LightningTransactionState.Active, subTxn.State);
        }

        [Fact]
        public void ResetTransactionAbortedOnDispose()
        {
            var txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            txn.Reset();
            txn.Dispose();
            Assert.Equal(LightningTransactionState.Aborted, txn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsAborted()
        {
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            txn.Abort();

            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsCommited()
        {
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            txn.Commit();

            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
        {
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            _env.Dispose();

            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnReset()
        {
            var txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);

            txn.Reset();

            Assert.Equal(LightningTransactionState.Reseted, txn.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnRenew()
        {
            var txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            txn.Reset();

            txn.Renew();

            Assert.Equal(LightningTransactionState.Active, txn.State);
        }

        [Fact]
        public void CanCountTransactionEntries()
        {
            var txn = _env.BeginTransaction();
            var db = txn.OpenDatabase();

            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                txn.Put(db, i.ToString(), i.ToString());

            var count = txn.GetEntriesCount(db);

            Assert.Equal(entriesCount, count);
        }

        [Fact]
        public void TransactionShouldSupportCustomComparer()
        {
            Func<int, int, int> comparison = (l, r) => l.CompareTo(r);
            var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create};
            Func<MDBValue, MDBValue, int> compareWith =
                (l, r) => comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
            options.CompareWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>(compareWith)));

            using (var txnT = _env.BeginTransaction())
            using (var db1 = txnT.OpenDatabase(configuration: options))
            {
                txnT.DropDatabase(db1);
                txnT.Commit();
            }

            var txn = _env.BeginTransaction();
            var db = txn.OpenDatabase(configuration: options);

            var keysUnsorted = Enumerable.Range(1, 10000).OrderBy(x => Guid.NewGuid()).ToList();
            var keysSorted = keysUnsorted.ToArray();
            Array.Sort(keysSorted, new Comparison<int>(comparison));

            GC.Collect();
            for (var i = 0; i < keysUnsorted.Count; i++)
                txn.Put(db, BitConverter.GetBytes(keysUnsorted[i]), BitConverter.GetBytes(i));

            using (var c = txn.CreateCursor(db))
            {
                int order = 0;
                while (c.Next() == MDBResultCode.Success)
                    Assert.Equal(keysSorted[order++], BitConverter.ToInt32(c.GetCurrent().key.CopyToNewArray(), 0));
            }
        }

        [Fact]
        public void TransactionShouldSupportCustomDupSorter()
        {
            Func<int, int, int> comparison = (l, r) => -Math.Sign(l - r);

            var txn = _env.BeginTransaction();
            var options = new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed};
            Func<MDBValue, MDBValue, int> compareWith = (l, r) => comparison(BitConverter.ToInt32(l.CopyToNewArray(), 0), BitConverter.ToInt32(r.CopyToNewArray(), 0));
            options.FindDuplicatesWith(Comparer<MDBValue>.Create(new Comparison<MDBValue>(compareWith)));
            var db = txn.OpenDatabase(configuration: options);

            var valuesUnsorted = new [] { 2, 10, 5, 0 };
            var valuesSorted = valuesUnsorted.ToArray();
            Array.Sort(valuesSorted, new Comparison<int>(comparison));

            using (var c = txn.CreateCursor(db))
                c.Put(BitConverter.GetBytes(123), valuesUnsorted.Select(BitConverter.GetBytes).ToArray());

            using (var c = txn.CreateCursor(db))
            {
                int order = 0;

                while (c.Next() == MDBResultCode.Success)
                    Assert.Equal(valuesSorted[order++], BitConverter.ToInt32(c.GetCurrent().value.CopyToNewArray(), 0));
            }
        }
    }
}
