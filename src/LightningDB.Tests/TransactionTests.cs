using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace LightningDB.Tests
{
    [TestFixture]
    public class TransactionTests
    {
        private string _path;
        private LightningEnvironment _env;
        private LightningTransaction _txn;

        public TransactionTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            _path = Path.Combine(
                Path.GetDirectoryName(location), 
                "TestDb" + Guid.NewGuid().ToString());
        }

        [SetUp]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();
        }

        [TearDown]
        public void Cleanup()
        {
            _env.Close();

            Directory.Delete(_path, true);
        }

        [Test]
        public void TransactionShouldBeCreated()
        {
            //arrange

            //act
            _txn = _env.BeginTransaction();

            //assert
            Assert.AreEqual(LightningTransactionState.Active, _txn.State);
        }

        [Test]
        public void TransactionShouldBeAbortedIfEnvironmentCloses()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(LightningTransactionState.Aborted, _txn.State);
        }

        [Test]
        public void TransactionShouldChangeStateOnCommit()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.AreEqual(LightningTransactionState.Commited, _txn.State);
        }

        [Test]
        public void ChildTransactionShouldBeCreated()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            var subTxn = _txn.BeginTransaction();

            //assert
            Assert.AreEqual(LightningTransactionState.Active, subTxn.State);
        }

        [Test]
        public void ChildTransactionShouldBeAbortedIfParentIsAborted()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Abort();

            //assert
            Assert.AreEqual(LightningTransactionState.Aborted, child.State);
        }

        [Test]
        public void ChildTransactionShouldBeAbortedIfParentIsCommited()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.AreEqual(LightningTransactionState.Aborted, child.State);
        }

        [Test]
        public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(LightningTransactionState.Aborted, child.State);
        }

        [Test]
        public void ReadOnlyTransactionShouldChangeStateOnReset()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);

            //act
            _txn.Reset();

            //assert
            Assert.AreEqual(LightningTransactionState.Reseted, _txn.State);
        }

        [Test]
        public void ReadOnlyTransactionShouldChangeStateOnRenew()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            _txn.Reset();

            //act
            _txn.Renew();

            //assert
            Assert.AreEqual(LightningTransactionState.Active, _txn.State);
        }

        [Test]
        public void DefaultDatabaseShouldBeDropped()
        {
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);
            //arrange

            //act
            _txn.DropDatabase(db, true);

            //assert
            Assert.AreEqual(false, db.IsOpened);
        }

        [Test]
        public void CanCountTransactionEntries()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase(null, DatabaseOpenFlags.None);

            const int entriesCount = 10;
            for (var i = 0; i < entriesCount; i++)
                _txn.Put(db, i.ToString(), i.ToString());

            //act
            var count = _txn.GetEntriesCount(db);

            //assert;
            Assert.AreEqual(entriesCount, count);
        }

        [Test]
        public void TransactionShouldSupportCustomComparer()
        {
            //arrange
            Func<int, int, int> comparison = (l, r) => -Math.Sign(l - r);

            _txn = _env.BeginTransaction();
            var db = _txn.OpenDatabase<int>(
                comparer: comparison);

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
                    Assert.AreEqual(keysSorted[order++], pair.Key);
            }
        }
    }
}
