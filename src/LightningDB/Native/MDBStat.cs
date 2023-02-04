namespace LightningDB.Native;

/// <summary>
/// Statistics for a database in the environment. 
/// </summary>
public struct MDBStat
{
    /// <summary>
    /// Size of a database page. This is currently the same for all databases.
    /// </summary>
    public uint ms_psize;

    /// <summary>
    /// Depth (height) of the B-tree
    /// </summary>
    public uint ms_depth;

    /// <summary>
    /// Number of internal (non-leaf) pages
    /// </summary>
    public nint ms_branch_pages;

    /// <summary>
    /// Number of leaf pages
    /// </summary>
    public nint ms_leaf_pages;

    /// <summary>
    /// Number of overflow pages
    /// </summary>
    public nint ms_overflow_pages;

    /// <summary>
    /// Number of data items
    /// </summary>
    public nint ms_entries;
}