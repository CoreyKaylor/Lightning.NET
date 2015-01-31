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

        public Encoding Encoding { get; set; }

        public DatabaseOpenFlags Flags { get; set; }

        #endregion

        private static CompareFunction CreateNativeCompareFunction(
            LightningDatabase db, LightningCompareDelegate compare)
        {
            return (IntPtr left, IntPtr right) =>
                compare.Invoke(db, NativeMethods.ValueByteArrayFromPtr(left), NativeMethods.ValueByteArrayFromPtr(right));
        }

        private static CompareFunction SetNativeCompareFunction(
            LightningTransaction tran, LightningDatabase db, LightningCompareDelegate compare)
        {
            var compareFunction = CreateNativeCompareFunction(db, compare);

            NativeMethods.Execute(lib =>
                lib.mdb_set_compare(tran._handle, db._handle, compareFunction));

            return compareFunction;
        }

        internal void SetComparer(LightningTransaction tran, LightningDatabase db)
        {
            if (Compare == null)
                return;

            var comparer = Compare.Invoke(new CompareFunctionBuilder());
            if (comparer == null)
                return;

            var nativeComparer = SetNativeCompareFunction(tran, db, comparer);
            tran.SubTransactionsManager.StoreComparer(comparer);
        }
    }
}
