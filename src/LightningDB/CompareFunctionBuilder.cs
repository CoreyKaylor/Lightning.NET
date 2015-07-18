using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
	/// <summary>
	/// Compare function builder.
	/// </summary>
    public class CompareFunctionBuilder
    {
        private static LightningCompareDelegate CreateComparisonFunction<TKey>(
            Func<TKey, TKey, int> comparer)
        {
            if (comparer == null)
                return null;

            return (db, left, right) =>
            {
                var converter = db.Environment.ConverterStore.GetFromBytes<TKey>();

                var leftTyped = converter.Convert(db, left);
                var rightTyped = converter.Convert(db, right);

                return comparer.Invoke(leftTyped, rightTyped);
            };
        }

        private static LightningCompareDelegate CreateComparisonFunction(
            Func<byte[], byte[], int> comparer)
        {
            if (comparer == null)
                return null;

            return (db, left, right) => comparer.Invoke(left, right);
        }

        private static LightningCompareDelegate CreateComparisonFunction<TKey>(
            IComparer<TKey> comparer)
        {
            if (comparer == null)
                return null;

            return CreateComparisonFunction<TKey>(comparer.Compare);
        }

        public LightningCompareDelegate FromFunc<TKey>(Func<TKey, TKey, int> comparer)
        {
            return CreateComparisonFunction(comparer);
        }

        public LightningCompareDelegate FromFunc(Func<byte[], byte[], int> comparer)
        {
            return CreateComparisonFunction(comparer);
        }

        public LightningCompareDelegate FromComparer<TKey>(IComparer<TKey> comparer)
        {
            return CreateComparisonFunction(comparer);
        }

        public LightningCompareDelegate FromDelegate(LightningCompareDelegate comparer)
        {
            return comparer;
        }
    }
}
