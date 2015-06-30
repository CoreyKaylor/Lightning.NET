using System;
using System.Linq;
using System.Collections.Generic;
using LightningDB.Converters;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class TransactionTests : IDisposable
    {
        private LightningEnvironment _env;

        public TransactionTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new LightningEnvironment(path);
            _env.WithConverters();
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
            //arrange
            var txn = _env.BeginTransaction();

            //act
            _env.Dispose();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, txn.State);
        }

        [Fact]
        public void TransactionShouldChangeStateOnCommit()
        {
            //arrange
            var txn = _env.BeginTransaction();

            //act
            txn.Commit();

            //assert
            Assert.Equal(LightningTransactionState.Commited, txn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeCreated()
        {
            //arrange
            var txn = _env.BeginTransaction();

            //act
            var subTxn = txn.BeginTransaction();

            //assert
            Assert.Equal(LightningTransactionState.Active, subTxn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsAborted()
        {
            //arrange
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            //act
            txn.Abort();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsCommited()
        {
            //arrange
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            //act
            txn.Commit();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
        {
            //arrange
            var txn = _env.BeginTransaction();
            var child = txn.BeginTransaction();

            //act
            _env.Dispose();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnReset()
        {
            //arrange
            var txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);

            //act
            txn.Reset();

            //assert
            Assert.Equal(LightningTransactionState.Reseted, txn.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnRenew()
        {
            //arrange
            var txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            txn.Reset();

            //act
            txn.Renew();

            //assert
            Assert.Equal(LightningTransactionState.Active, txn.State);
        }

        [Fact]
        public void CanCountTransactionEntries()
        {
            //arrange
            var txn = _env.BeginTransaction();
            var db = txn.OpenDatabase();

            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                txn.Put(db, i.ToString(), i.ToString());

            //act
            var count = txn.GetEntriesCount(db);

            //assert;
            Assert.Equal(entriesCount, count);
        }

        [Fact]
        public void TransactionShouldSupportCustomComparer()
        {
            Func<int, int, int> comparison = (l, r) => -Math.Sign(l - r);

            var txn = _env.BeginTransaction();
            var options = new DatabaseOptions {Flags = DatabaseOpenFlags.Create};
            Func<byte[], byte[], int> compareWith = (l, r) => comparison(BitConverter.ToInt32(l, 0), BitConverter.ToInt32(r, 0));
            options.CompareWith(Comparer<byte[]>.Create(new Comparison<byte[]>(compareWith)));
            var db = txn.OpenDatabase(options: options);

            var keysUnsorted = new [] { 2, 10, 5 };
            var keysSorted = keysUnsorted.ToArray();
            Array.Sort(keysSorted, new Comparison<int>(comparison));

            for (var i = 0; i < keysUnsorted.Length; i++)
                txn.Put(db, keysUnsorted[i], i);

            using (var c = txn.CreateCursor(db))
            {
                int order = 0;

                KeyValuePair<int, int> pair;
                while (c.MoveNext(out pair))
                    Assert.Equal(keysSorted[order++], pair.Key);
            }
        }

        [Fact]
        public void TransactionShouldSupportCustomDupSorter()
        {
            //arrange
            Func<int, int, int> comparison = (l, r) => -Math.Sign(l - r);

            var txn = _env.BeginTransaction();
            var options = new DatabaseOptions {Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesFixed};
            Func<byte[], byte[], int> compareWith = (l, r) => comparison(BitConverter.ToInt32(l, 0), BitConverter.ToInt32(r, 0));
            options.FindDuplicatesWith(Comparer<byte[]>.Create(new Comparison<byte[]>(compareWith)));
            var db = txn.OpenDatabase(options: options);

            var valuesUnsorted = new [] { 2, 10, 5, 0 };
            var valuesSorted = valuesUnsorted.ToArray();
            Array.Sort(valuesSorted, new Comparison<int>(comparison));

            //act
            using (var c = txn.CreateCursor(db))
                c.PutMultiple(123, valuesUnsorted);

            //assert
            using (var c = txn.CreateCursor(db))
            {
                int order = 0;

                KeyValuePair<int, int> pair;
                while (c.MoveNextDuplicate(out pair))
                    Assert.Equal(valuesSorted[order++], pair.Value);
            }
        }
    }
}
