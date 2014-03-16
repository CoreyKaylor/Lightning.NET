namespace LightningDB
{
    /// <summary>
    /// Cursor delete operation options
    /// </summary>
    public enum CursorDeleteOption
    {
        /// <summary>
        /// No special behavior
        /// </summary>
        None = 0,

        /// <summary>
        /// Only for MDB_DUPSORT
        /// For put: don't write if the key and data pair already exist.
        /// For mdb_cursor_del: remove all duplicate data items.
        /// </summary>
        NoDuplicateData = 0x20
    }
}
