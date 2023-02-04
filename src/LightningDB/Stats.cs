namespace LightningDB;

/// <summary>
/// Statistics for a database in the environment. 
/// </summary>
public class Stats
{
    /// <summary>
    /// Size of a database page. This is currently the same for all databases. 
    /// </summary>
    public long PageSize { get; set; }

    /// <summary>
    /// Depth (height) of the B-tree 
    /// </summary>
    public long BTreeDepth { get; set; }

    /// <summary>
    /// Number of internal (non-leaf) pages 
    /// </summary>
    public long BranchPages { get; set; }

    /// <summary>
    /// Number of leaf pages 
    /// </summary>
    public long LeafPages { get; set; }

    /// <summary>
    /// Number of overflow pages 
    /// </summary>
    public long OverflowPages { get; set; }

    /// <summary>
    /// Number of data items 
    /// </summary>
    public long Entries { get; set; }
}