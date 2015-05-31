using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightningDB.Native;

namespace LightningDB
{
    public class DatabaseOptions
    {
        public DatabaseOptions()
        {
            Flags = LightningConfig.Database.DefaultOpenFlags;
            Encoding = LightningConfig.Database.DefaultEncoding;
        }

        #region IDatabaseOptions Members

        public Func<CompareFunctionBuilder, LightningCompareDelegate> Compare { get; set; }

        public Func<CompareFunctionBuilder, LightningCompareDelegate> DuplicatesSort { get; set; }

        public Encoding Encoding { get; set; }

        public DatabaseOpenFlags Flags { get; set; }

        #endregion

        private static CompareFunction CreateNativeCompareFunction(
            LightningDatabase db, LightningCompareDelegate compare)
        {
            return (IntPtr left, IntPtr right) =>
                compare.Invoke(db, NativeMethods.ValueByteArrayFromPtr(left), NativeMethods.ValueByteArrayFromPtr(right));
        }

        private static void SetNativeCompareFunction(
            LightningTransaction tran, 
            LightningDatabase db,
            Func<CompareFunctionBuilder, LightningCompareDelegate> delegateFactory,
            Func<INativeLibraryFacade, CompareFunction, int> setter)
        {
            if (delegateFactory == null)
                return;

            var comparer = delegateFactory.Invoke(new CompareFunctionBuilder());
            if (comparer == null)
                return;

            var compareFunction = CreateNativeCompareFunction(db, comparer);

            NativeMethods.Execute(lib => setter.Invoke(lib, compareFunction));
            tran.SubTransactionsManager.StoreCompareFunction(compareFunction);
        }

        internal void SetComparer(LightningTransaction tran, LightningDatabase db)
        {
            SetNativeCompareFunction(
                tran,
                db,
                Compare,
                (lib, func) => lib.mdb_set_compare(tran._handle, db._handle, func));
        }

        internal void SetDuplicatesSort(LightningTransaction tran, LightningDatabase db)
        {
            if (!db.OpenFlags.HasFlag(DatabaseOpenFlags.DuplicatesSort))
                return;

            SetNativeCompareFunction(
                tran,
                db,
                DuplicatesSort,
                (lib, func) => lib.mdb_set_dupsort(tran._handle, db._handle, func));
        }
    }
}
