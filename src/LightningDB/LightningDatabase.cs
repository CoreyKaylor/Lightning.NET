using System;
using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
/// Represents a database in the Lightning environment, providing mechanisms to perform
/// various operations such as opening, dropping, and accessing database statistics. Generally,
/// a database can be reused and rarely needs to be disposed.
/// </summary>
public sealed class LightningDatabase : IDisposable
{
    internal uint _handle;
    private bool _disposed;
    private readonly DatabaseConfiguration _configuration;
    private readonly bool _closeOnDispose;
    private readonly LightningTransaction _transaction;
    private readonly IDisposable _pinnedConfig;

    /// <summary>
    /// Creates a LightningDatabase instance.
    /// </summary>
    /// <param name="name">Database name.</param>
    /// <param name="transaction">Active transaction.</param>
    /// <param name="configuration">Options for the database, like encoding, option flags, and comparison logic.</param>
    /// <param name="closeOnDispose">Close database handle on dispose</param>
    internal LightningDatabase(string name, LightningTransaction transaction, DatabaseConfiguration configuration,
        bool closeOnDispose)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        Name = name;
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _closeOnDispose = closeOnDispose;
        Environment = transaction.Environment;
        _transaction = transaction;
        mdb_dbi_open(transaction._handle, name, _configuration.Flags, out _handle).ThrowOnError();
        _pinnedConfig = _configuration.ConfigureDatabase(transaction, this);
        IsOpened = true;
    }

    /// <summary>
    /// Whether the database handle has been release from Dispose, or from unsuccessful OpenDatabase call.
    /// </summary>
    public bool IsReleased => _handle == default;

    /// <summary>
    /// Is database opened.
    /// </summary>
    public bool IsOpened { get; private set; }

    public Stats DatabaseStats => _transaction.GetStats(this);

    /// <summary>
    /// Database name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Environment in which the database was opened.
    /// </summary>
    public LightningEnvironment Environment { get; }

    /// <summary>
    /// Gets the flags used when opening this database.
    /// </summary>
    /// <param name="transaction">The transaction to use for retrieving the flags</param>
    /// <returns>The database flags</returns>
    public DatabaseOpenFlags GetFlags(LightningTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        mdb_dbi_flags(transaction._handle, _handle, out var flags).ThrowOnError();
        return (DatabaseOpenFlags)flags;
    }

    /// <summary>
    /// Drops the database.
    /// </summary>
    public MDBResultCode Drop(LightningTransaction transaction)
    {
        var result = mdb_drop(transaction._handle, _handle, true);
        IsOpened = false;
        _handle = default;
        return result;
    }

    /// <summary>
    /// Truncates all data from the database.
    /// </summary>
    public MDBResultCode Truncate(LightningTransaction transaction)
    {
        return mdb_drop(transaction._handle, _handle, false);
    }

    /// <summary>
    /// Deallocates resources opened by the database.
    /// </summary>
    /// <param name="disposing">true if called from Dispose.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;
        if (!IsOpened)
            return;
        if (!Environment.IsOpened && _closeOnDispose && disposing)
            throw new InvalidOperationException("A database must be disposed before closing the environment");

        IsOpened = false;
        _pinnedConfig?.Dispose();

        if (_closeOnDispose && Environment.IsOpened)
            mdb_dbi_close(Environment._handle, _handle);

        if (disposing)
            _handle = default;
    }

    /// <summary>
    /// Deallocates resources opened by the database.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    ~LightningDatabase()
    {
        Dispose(false);
    }
}
