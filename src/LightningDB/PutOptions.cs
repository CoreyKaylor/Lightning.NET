using System;

namespace LightningDB
{
    /// <summary>
    /// Special options for put operation.
    /// </summary>
    [Flags]
    public enum PutOptions
    {
        /// <summary>
        /// No special behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only for MDB_DUPSORT
        /// For put: don't write if the key and data pair already exist.
        /// For mdb_cursor_del: remove all duplicate data items.
        /// </summary>
        NoDuplicateData = 0x20,

        /// <summary>
        /// For put: Don't write if the key already exists.
        /// </summary>
        NoOverwrite = 0x10,

        /// <summary>
        /// For put: Just reserve space for data, don't copy it. Return a pointer to the reserved space.
        /// </summary>
        ReserveSpace = 0x10000,

        /// <summary>
        /// Data is being appended, don't split full pages.
        /// </summary>
        AppendData = 0x20000,

        /// <summary>
        /// Duplicate data is being appended, don't split full pages.
        /// </summary>
        AppendDuplicateData = 0x40000
    }
}
