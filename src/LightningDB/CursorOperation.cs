namespace LightningDB
{
    /// <summary>
    /// Cursor operation types
    /// </summary>
    public enum CursorOperation
    {
        /// <summary>
        /// Position at first key/data item
        /// </summary>
        First,

        /// <summary>
        /// Position at first data item of current key. Only for MDB_DUPSORT
        /// </summary>
        FirstDuplicate,

        /// <summary>
        /// Position at key/data pair. Only for MDB_DUPSORT
        /// </summary>
        GetBoth,

        /// <summary>
        /// position at key, nearest data. Only for MDB_DUPSORT
        /// </summary>
        GetBothRange,

        /// <summary>
        /// Return key/data at current cursor position
        /// </summary>
        GetCurrent,

        /// <summary>
        /// Return all the duplicate data items at the current cursor position. Only for MDB_DUPFIXED
        /// </summary>
        GetMultiple,

        /// <summary>
        /// Position at last key/data item
        /// </summary>
        Last,

        /// <summary>
        /// Position at last data item of current key. Only for MDB_DUPSORT
        /// </summary>
        LastDuplicate,

        /// <summary>
        /// Position at next data item
        /// </summary>
        Next,

        /// <summary>
        /// Position at next data item of current key. Only for MDB_DUPSORT
        /// </summary>
        NextDuplicate,

        /// <summary>
        /// Return all duplicate data items at the next cursor position. Only for MDB_DUPFIXED
        /// </summary>
        NextMultiple,

        /// <summary>
        /// Position at first data item of next key. Only for MDB_DUPSORT
        /// </summary>
        NextNoDuplicate,

        /// <summary>
        /// Position at previous data item
        /// </summary>
        Previous,

        /// <summary>
        /// Position at previous data item of current key. Only for MDB_DUPSORT
        /// </summary>
        PreviousDuplicate,

        /// <summary>
        /// Position at last data item of previous key. Only for MDB_DUPSORT
        /// </summary>
        PreviousNoDuplicate,

        /// <summary>
        /// Position at specified key
        /// </summary>
        Set,

        /// <summary>
        /// Position at specified key, return key + data
        /// </summary>
        SetKey,

        /// <summary>
        /// Position at first key greater than or equal to specified key.
        /// </summary>
        SetRange
    }
}
