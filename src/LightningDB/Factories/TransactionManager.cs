using System;
using System.Collections.Concurrent;
using System.Linq;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB.Factories
{
    class TransactionManager
    {
        private readonly LightningEnvironment _environment;
        private readonly LightningTransaction _parentTransaction;
        private readonly IntPtr _parentHandle;

        private readonly ConcurrentDictionary<LightningTransaction, bool> _transactions;

        //prevent delegated from being collected by GC by storing them in collection
        private readonly ConcurrentDictionary<CompareFunction, bool> _comparatorsStore;

        public TransactionManager(LightningEnvironment environment, LightningTransaction parentTransaction)
        {
            if (environment == null)
                throw new ArgumentNullException("environment");

            _environment = environment;

            _parentTransaction = parentTransaction;
            _parentHandle = parentTransaction != null
                ? parentTransaction._handle
                : IntPtr.Zero;

            _transactions = new ConcurrentDictionary<LightningTransaction, bool>();
            _comparatorsStore = new ConcurrentDictionary<CompareFunction, bool>();
        }

        private void EnsureEnvironmentOpened()
        {
            if (!_environment.IsOpened)
                throw new InvalidOperationException("Environment should be opened");
        }

        public LightningTransaction Create(TransactionBeginFlags beginFlags)
        {
            EnsureEnvironmentOpened();

            IntPtr handle = default(IntPtr);
            mdb_txn_begin(_environment._handle, _parentHandle, beginFlags, out handle);

            var tran = new LightningTransaction(_environment, handle, _parentTransaction, beginFlags);

            _transactions.TryAdd(tran, true);

            return tran;
        }

        public void WasDiscarded(LightningTransaction tn)
        {
            bool value;
            _transactions.TryRemove(tn, out value);
        }

        private static void AbortAll(TransactionManager manager)
        {
            foreach (var p in manager._transactions.ToList())
            {
                AbortAll(p.Key.SubTransactionsManager);
                p.Key.Abort();
            }
        }

        public void AbortAll()
        {
            _comparatorsStore.Clear();
            AbortAll(this);
        }

        public void StoreCompareFunction(CompareFunction compareFunction)
        {
            _comparatorsStore.TryAdd(compareFunction, true);
        }
    }
}
