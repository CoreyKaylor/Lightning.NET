using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
/// Represents the configuration for a database in the LightningDB library.
/// Allows setting custom flags and configuring comparer logic for database operations.
/// </summary>
public class DatabaseConfiguration
{
    private IComparer<MDBValue> _comparer;
    private IComparer<MDBValue> _duplicatesComparer;

    public DatabaseConfiguration()
    {
        Flags = DatabaseOpenFlags.None;
    }

    /// <summary>
    /// Gets or sets the configuration flags used when opening a database.
    /// </summary>
    /// <remarks>
    /// The <see cref="Flags"/> property specifies the behavior of the database based on the combination of
    /// values from the <see cref="DatabaseOpenFlags"/> enumeration. These flags determine how the database
    /// should be opened and interacted with, such as creating new databases, sorting duplicates, or using
    /// integer keys. The default value is <see cref="DatabaseOpenFlags.None"/>.
    /// </remarks>
    public DatabaseOpenFlags Flags { get; set; }


    internal IDisposable ConfigureDatabase(LightningTransaction tx, LightningDatabase db)
    {
        var pinnedComparer = new ComparerKeepAlive();
        if (_comparer != null)
        {
            CompareFunction compare = (ref left, ref right) => _comparer.Compare(left, right);
            pinnedComparer.AddComparer(compare);
            mdb_set_compare(tx._handle, db._handle, compare);
        }

        if (_duplicatesComparer == null) return pinnedComparer;
        CompareFunction dupCompare = (ref left, ref right) => _duplicatesComparer.Compare(left, right);
        pinnedComparer.AddComparer(dupCompare);
        mdb_set_dupsort(tx._handle, db._handle, dupCompare);
        return pinnedComparer;
    }

    /// <summary>
    /// Sets a custom comparer for database operations using the specified comparer.
    /// </summary>
    /// <param name="comparer">
    /// The comparer implementation to use for comparing MDBValue objects.
    /// </param>
    public void CompareWith(IComparer<MDBValue> comparer)
    {
        _comparer = comparer;
    }

    /// <summary>
    /// Sets a custom comparer for detecting duplicate records in the database.
    /// </summary>
    /// <param name="comparer">
    /// The comparer implementation to use for identifying duplicates between MDBValue objects.
    /// </param>
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