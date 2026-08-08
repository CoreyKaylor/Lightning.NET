namespace LightningDB;

public enum MDBResultCode
{
    /// <summary>
    /// Successful result
    /// </summary>
    Success = 0,
    /// <summary>
    /// key/data pair already exists
    /// </summary>
    KeyExist = -30799,
    /// <summary>
    /// key/data pair not found (EOF)
    /// </summary>
    NotFound = -30798,
    /// <summary>
    /// Requested page not found - this usually indicates corruption
    /// </summary>
    PageNotFound = -30797,
    /// <summary>
    /// Located page was wrong type
    /// </summary>
    Corrupted = -30796,
    /// <summary>
    /// Update of meta page failed or environment had fatal error
    /// </summary>
    Panic = -30795,
    /// <summary>
    /// Environment version mismatch
    /// </summary>
    VersionMismatch = -30794,
    /// <summary>
    /// File is not a valid LMDB file
    /// </summary>
    Invalid = -30793,
    /// <summary>
    /// Environment mapsize reached
    /// </summary>
    MapFull = -30792,
    /// <summary>
    /// Environment maxdbs reached
    /// </summary>
    DbsFull = -30791,
    /// <summary>
    /// Environment maxreaders reached
    /// </summary>
    ReadersFull = -30790,
    /// <summary>
    /// Too many TLS keys in use - Windows only
    /// </summary>
    TLSFull = -30789,
    /// <summary>
    /// Txn has too many dirty pages
    /// </summary>
    TxnFull = -30788,
    /// <summary>
    /// Cursor stack too deep - internal error
    /// </summary>
    CursorFull = -30787,
    /// <summary>
    /// Page has not enough space - internal error
    /// </summary>
    PageFull = -30786,
    /// <summary>
    /// Database contents grew beyond environment mapsize
    /// </summary>
    MapResized = -30785,
    /// <summary>
    /// Operation and DB incompatible, or DB type changed. This can mean:
    ///	- The operation expects an #MDB_DUPSORT / #MDB_DUPFIXED database.
    /// - Opening a named DB when the unnamed DB has #MDB_DUPSORT / #MDB_INTEGERKEY.
    /// - Accessing a data record as a database, or vice versa.
    /// - The database was dropped and recreated with different flags.
    /// </summary>
    Incompatible = -30784,
    /// <summary>
    /// Invalid reuse of reader locktable slot
    /// </summary>
    BadRSlot = -30783,
    /// <summary>
    /// Transaction must abort, has a child, or is invalid
    /// </summary>
    BadTxn = -30782,
    /// <summary>
    /// Unsupported size of key/DB name/data, or wrong DUPFIXED size
    /// </summary>
    BadValSize = -30781,
    /// <summary>
    /// The specified DBI was changed unexpectedly
    /// </summary>
    BadDBI = -30780,
    /// <summary>
    /// Unexpected problem - txn should abort
    /// </summary>
    Problem = -30779,
    /// <summary>
    /// Page checksum incorrect
    /// </summary>
    BadChecksum = -30778,
    /// <summary>
    /// Encryption/decryption failed
    /// </summary>
    CryptoFail = -30777,
    /// <summary>
    /// Environment encryption mismatch
    /// </summary>
    EnvEncryption = -30776,
    /// <summary>
    /// Transaction was already prepared
    /// </summary>
    TxnPending = -30775,
    /// <summary>
    /// Environment can't rollback the last transaction
    /// </summary>
    CantRollback = -30774,
    /// <summary>
    /// Can't drop the main DBI while other DBIs are open
    /// </summary>
    DbisBusy = -30773,
    /// <summary>
    /// Write was incomplete
    /// </summary>
    ShortWrite = -30772,
    /// <summary>
    /// Environment is busy, can't use previous snapshot
    /// </summary>
    EnvBusy = -30771,
    /// <summary>
    /// Environment or transaction is read-only, can't write
    /// </summary>
    ReadOnly = -30770,
    /// <summary>
    /// The requested map address is unavailable
    /// </summary>
    AddressBusy = -30769,
    /// <summary>
    /// ENOENT error from C-runtime
    /// </summary>
    FileNotFound = 2,
    /// <summary>
    /// EIO error from C-runtime
    /// </summary>
    AccessDenied = 5,
    /// <summary>
    /// ENOMEM error from C-runtime
    /// </summary>
    InvalidAccess = 12,
    /// <summary>
    /// EACCES error from C-runtime
    /// </summary>
    InvalidData = 13,
    /// <summary>
    /// EBUSY error from C-runtime
    /// </summary>
    CurrentDirectory = 16,
    /// <summary>
    /// EINVAL error from C-runtime
    /// </summary>
    BadCommand = 22, 
    /// <summary>
    /// ENOSPC error from C-runtime
    /// </summary>
    OutOfPaper = 28
}