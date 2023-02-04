﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB;

public class DatabaseConfiguration
{
    private IComparer<MDBValue> _comparer;
    private IComparer<MDBValue> _duplicatesComparer;

    public DatabaseConfiguration()
    {
        Flags = DatabaseOpenFlags.None;
    }

    public DatabaseOpenFlags Flags { get; set; }


    internal IDisposable ConfigureDatabase(LightningTransaction tx, LightningDatabase db)
    {
        var pinnedComparer = new ComparerKeepAlive();
        if (_comparer != null)
        {
            CompareFunction compare = Compare;
            pinnedComparer.AddComparer(compare);
            mdb_set_compare(tx.Handle(), db.Handle(), compare);
        }

        if (_duplicatesComparer == null) return pinnedComparer;
        CompareFunction dupCompare = IsDuplicate;
        pinnedComparer.AddComparer(dupCompare);
        mdb_set_dupsort(tx.Handle(), db.Handle(), dupCompare);
        return pinnedComparer;
    }

    private int Compare(ref MDBValue left, ref MDBValue right)
    {
        return _comparer.Compare(left, right);
    }

    private int IsDuplicate(ref MDBValue left, ref MDBValue right)
    {
        return _duplicatesComparer.Compare(left, right);
    }

    public void CompareWith(IComparer<MDBValue> comparer)
    {
        _comparer = comparer;
    }

    public void FindDuplicatesWith(IComparer<MDBValue> comparer)
    {
        _duplicatesComparer = comparer;
    }

    private class ComparerKeepAlive : IDisposable
    {
        private readonly List<GCHandle> _comparisons = new();

        public void AddComparer(CompareFunction compare)
        {
            var handle = GCHandle.Alloc(compare);
            _comparisons.Add(handle);
        }

        public void Dispose()
        {
            for (var i = 0; i < _comparisons.Count; ++i)
            {
                _comparisons[i].Free();
            }
        }
    }
}