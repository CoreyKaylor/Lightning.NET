using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightningDB.Native;

namespace LightningDB.Factories
{
    class TransactionManager
    {
        private readonly LightningEnvironment _environment;
        private readonly LightningTransaction _parentTransaction;
        private readonly IntPtr _parentHandle;

        private readonly ConcurrentDictionary<LightningTransaction, bool> _transactions;

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
            NativeMethods.Execute(lib => lib.mdb_txn_begin(_environment._handle, _parentHandle, beginFlags, out handle));

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
            AbortAll(this);
        }
    }
}
