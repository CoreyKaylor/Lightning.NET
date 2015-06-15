using System;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Flags to open a database with.
    /// </summary>
    [Flags]
    public enum DatabaseOpenFlags
    {
        /// <summary>
        /// No special options.
        /// </summary>
        None = 0,
                
        /// <summary>
        /// MDB_REVERSEKEY. Keys are strings to be compared in reverse order, from the end of the strings to the beginning. By default, Keys are treated as strings and compared from beginning to end.
        /// </summary>
        ReverseKey = 0x02,

        /// <summary>
        /// MDB_DUPSORT. Duplicate keys may be used in the database. (Or, from another perspective, keys may have multiple data items, stored in sorted order.) By default keys must be unique and may have only a single data item.
        /// </summary>
        DuplicatesSort = Lmdb.MDB_DUPSORT,

        /// <summary>
        /// MDB_INTEGERKEY. Keys are binary integers in native byte order. 
        /// Setting this option requires all keys to be the same size, typically sizeof(int) or sizeof(size_t).
        /// </summary>
        IntegerKey = 0x08,

        /// <summary>
        /// MDB_DUPFIXED. This flag may only be used in combination with MDB_DUPSORT. This option tells the library that the data items for this database are all the same size, which allows further optimizations in storage and retrieval. When all data items are the same size, the MDB_GET_MULTIPLE and MDB_NEXT_MULTIPLE cursor operations may be used to retrieve multiple items at once.
        /// </summary>
        DuplicatesFixed = Lmdb.MDB_DUPSORT | Lmdb.MDB_DUPFIXED,

        /// <summary>
        /// MDB_INTEGERDUP. This option specifies that duplicate data items are also integers, and should be sorted as such.
        /// </summary>
        IntegerDuplicates = 0x20,

        /// <summary>
        /// MDB_REVERSEDUP. This option specifies that duplicate data items should be compared as strings in reverse order.
        /// </summary>
        ReverseDuplicates = 0x40,

        /// <summary>
        /// Create the named database if it doesn't exist. This option is not allowed in a read-only transaction or a read-only environment.
        /// </summary>
        Create = 0x40000
    }
}
