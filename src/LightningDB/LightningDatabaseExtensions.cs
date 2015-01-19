using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightningDB.Converters;

namespace LightningDB
{
    public static class LightningDatabaseExtensions
    {
        private static Func<LightningDatabase, byte[], byte[], int> CreateComparisonFunction<TKey>(
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

        private static Func<LightningDatabase, byte[], byte[], int> CreateComparisonFunction<TKey>(
            IComparer<TKey> comparer)
        {
            if (comparer == null)
                return null;

            return CreateComparisonFunction<TKey>(comparer.Compare);
        }

        private static Func<LightningDatabase, byte[], byte[], int> CreateComparisonFunction(
            Func<byte[], byte[], int> comparer)
        {
            if (comparer == null)
                return null;

            return (db, left, right) => comparer.Invoke(left, right);
        }

        public static LightningDatabase OpenDatabase(
            this LightningTransaction tran,
            string name = null,
            DatabaseOpenFlags? flags = null,
            Encoding encoding = null,
            Func<byte[], byte[], int> comparer = null)
        {
            return tran.OpenDatabase(name, flags, encoding, CreateComparisonFunction(comparer));
        }

        public static LightningDatabase OpenDatabase<TKey>(
            this LightningTransaction tran,
            string name = null,
            DatabaseOpenFlags? flags = null,
            Encoding encoding = null,
            Func<TKey, TKey, int> comparer = null)
        {
            return tran.OpenDatabase(name, flags, encoding, CreateComparisonFunction(comparer));
        }

        public static LightningDatabase OpenDatabase<TKey>(
            this LightningTransaction tran,
            string name = null,
            DatabaseOpenFlags? flags = null,
            Encoding encoding = null,
            IComparer<TKey> comparer = null)
        {
            return tran.OpenDatabase(name, flags, encoding, CreateComparisonFunction(comparer));
        }
    }
}
