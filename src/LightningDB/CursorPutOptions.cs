namespace LightningDB
{
    /// <summary>
    /// Special options for cursor put operation.
    /// </summary>
    public enum CursorPutOptions
    {
        /// <summary>
        /// No special behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Overwrite the current key/data pair
        /// </summary>
        Current = 0x40,

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
        AppendDuplicateData = 0x40000,

        /// <summary>
        /// Store multiple data items in one call. Only for MDB_DUPFIXED.
        /// </summary>
        MultipleData = 0x80000
    }
}
