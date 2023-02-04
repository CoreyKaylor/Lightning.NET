namespace LightningDB;

/// <summary>
/// Transaction state.
/// </summary>
public enum LightningTransactionState
{
    /// <summary>
    /// Transaction is currently active.
    /// </summary>
    Active,

    /// <summary>
    /// Transaction is currently reset.
    /// </summary>
    Reset,

    /// <summary>
    /// Transaction is aborted.
    /// </summary>
    Aborted,

    /// <summary>
    /// Transaction is committed.
    /// </summary>
    Committed
}