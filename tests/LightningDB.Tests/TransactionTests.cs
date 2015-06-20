using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class TransactionTests : IDisposable
    {
        private LightningEnvironment _env;
        private LightningTransaction _txn;

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
            //arrange

            //act
            _txn = _env.BeginTransaction();

            //assert
            Assert.Equal(LightningTransactionState.Active, _txn.State);
        }

        [Fact]
        public void TransactionShouldBeAbortedIfEnvironmentCloses()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _env.Dispose();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, _txn.State);
        }

        [Fact]
        public void TransactionShouldChangeStateOnCommit()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.Equal(LightningTransactionState.Commited, _txn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeCreated()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            var subTxn = _txn.BeginTransaction();

            //assert
            Assert.Equal(LightningTransactionState.Active, subTxn.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsAborted()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Abort();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfParentIsCommited()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _env.Dispose();

            //assert
            Assert.Equal(LightningTransactionState.Aborted, child.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnReset()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);

            //act
            _txn.Reset();

            //assert
            Assert.Equal(LightningTransactionState.Reseted, _txn.State);
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnRenew()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            _txn.Reset();

            //act
            _txn.Renew();

            //assert
            Assert.Equal(LightningTransactionState.Active, _txn.State);
        }

        [Fact]
        public void DefaultDatabaseShouldBeDropped()
        {
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None });
            //arrange

            //act
            db.Drop();

            //assert
            Assert.Equal(false, db.IsOpened);
        }

        [Fact]
        public void CanCountTransactionEntries()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, new DatabaseOptions { Flags = DatabaseOpenFlags.None });

            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                _txn.Put(db, i.ToString(), i.ToString());

            //act
            var count = _txn.GetEntriesCount(db);

            //assert;
            Assert.Equal(entriesCount, count);
        }

        [Fact]
        public void TransactionShouldSupportCustomComparer()
        {
            //arrange
            Func<int, int, int> comparison = (l, r) => -Math.Sign(l - r);

            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(
                options: new DatabaseOptions { Compare = b => b.FromFunc(comparison) });

            var keysUnsorted = new int[] { 2, 10, 5 };
            var keysSorted = keysUnsorted.ToArray();
            Array.Sort(keysSorted, new Comparison<int>(comparison));

            //act
            for (var i = 0; i < keysUnsorted.Length; i++)
                _txn.Put(keysUnsorted[i], i);

            //assert
            using (var c = _txn.CreateCursor(db))
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

            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(
                options: new DatabaseOptions 
                { 
                    Flags = DatabaseOpenFlags.DuplicatesFixed,
                    DuplicatesSort = b => b.FromFunc(comparison) 
                });

            var valuesUnsorted = new int[] { 2, 10, 5, 0 };
            var valuesSorted = valuesUnsorted.ToArray();
            Array.Sort(valuesSorted, new Comparison<int>(comparison));

            //act
            using (var c = _txn.CreateCursor(db))
                c.PutMultiple(123, valuesUnsorted);

            //assert
            using (var c = _txn.CreateCursor(db))
            {
                int order = 0;

                KeyValuePair<int, int> pair;
                while (c.MoveNextDuplicate(out pair))
                    Assert.Equal(valuesSorted[order++], pair.Value);
            }
        }
    }
}
