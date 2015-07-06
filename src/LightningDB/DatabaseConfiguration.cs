using System.Collections.Generic;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB
{
    public class DatabaseConfiguration
    {
        private IComparer<byte[]> _comparer;
        private IComparer<byte[]> _duplicatesComparer;

        public DatabaseConfiguration()
        {
            Flags = DatabaseOpenFlags.None;
        }

        public DatabaseOpenFlags Flags { get; set; }


        internal void ConfigureDatabase(LightningTransaction tx, LightningDatabase db)
        {
            if (_comparer != null)
            {
                mdb_set_compare(tx.Handle(), db.Handle(), Compare);
            }
            if (_duplicatesComparer != null)
            {
                mdb_set_dupsort(tx.Handle(), db.Handle(), IsDuplicate);
            }
        }

        private int Compare(ref ValueStructure left, ref ValueStructure right)
        {
            return _comparer.Compare(left.GetBytes(), right.GetBytes());
        }

        private int IsDuplicate(ref ValueStructure left, ref ValueStructure right)
        {
            return _duplicatesComparer.Compare(left.GetBytes(), right.GetBytes());
        }

        public void CompareWith(IComparer<byte[]> comparer)
        {
            _comparer = comparer;
        }

        public void FindDuplicatesWith(IComparer<byte[]> comparer)
        {
            _duplicatesComparer = comparer;
        }
    }
}
