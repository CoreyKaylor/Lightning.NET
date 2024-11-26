namespace LightningDB;

/// <summary>
/// Transaction state.
/// </summary>
public enum LightningTransactionState
{
    /// <summary>
    /// Transaction is ready.
    /// </summary>
    Ready,

    /// <summary>
    /// Transaction is currently reset.
    /// </summary>
    Reset,

    /// <summary>
    /// Transaction is done.
    /// </summary>
    Done,

    /// <summary>
    /// Transaction is committed.
    /// </summary>
    Released
}