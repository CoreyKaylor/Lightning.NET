using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LightningDB.Tests
{
    [TestClass]
    public class TransactionTests
    {
        private string _path;
        private LightningEnvironment _env;
        private LightningTransaction _txn;

        public TransactionTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            _path = Path.Combine(Path.GetDirectoryName(location), "TestDb");
        }

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _env.Close();

            Directory.Delete(_path, true);
        }

        [TestMethod]
        public void TransactionShouldBeCreated()
        {
            //arrange

            //act
            _txn = _env.BeginTransaction();

            //assert
            Assert.AreEqual(LightningTransacrionState.Active, _txn.State);
        }

        [TestMethod]
        public void TransactionShouldBeAbortedIfEnvironmentCloses()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(LightningTransacrionState.Aborted, _txn.State);
        }

        [TestMethod]
        public void TransactionShouldChangeStateOnCommit()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.AreEqual(LightningTransacrionState.Commited, _txn.State);
        }

        [TestMethod]
        public void ChildTransactionShouldBeCreated()
        {
            //arrange
            _txn = _env.BeginTransaction();

            //act
            var subTxn = _txn.BeginTransaction();

            //assert
            Assert.AreEqual(LightningTransacrionState.Active, subTxn.State);
        }

        [TestMethod]
        public void ChildTransactionShouldBeAbortedIfParentIsAborted()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Abort();

            //assert
            Assert.AreEqual(LightningTransacrionState.Aborted, child.State);
        }

        [TestMethod]
        public void ChildTransactionShouldBeAbortedIfParentIsCommited()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _txn.Commit();

            //assert
            Assert.AreEqual(LightningTransacrionState.Aborted, child.State);
        }

        [TestMethod]
        public void ChildTransactionShouldBeAbortedIfEnvironmentIsClosed()
        {
            //arrange
            _txn = _env.BeginTransaction();
            var child = _txn.BeginTransaction();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(LightningTransacrionState.Aborted, child.State);
        }

        [TestMethod]
        public void ReadOnlyTransactionShouldChangeStateOnReset()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);

            //act
            _txn.Reset();

            //assert
            Assert.AreEqual(LightningTransacrionState.Reseted, _txn.State);
        }

        [TestMethod]
        public void ReadOnlyTransactionShouldChangeStateOnRenew()
        {
            //arrange
            _txn = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            _txn.Reset();

            //act
            _txn.Renew();

            //assert
            Assert.AreEqual(LightningTransacrionState.Active, _txn.State);
        }
    }
}
