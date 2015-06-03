using System;

namespace LightningDB
{
    /// <summary>
    /// An exception caused by lmdb operations.
    /// </summary>
    public class LightningException : Exception
    {
        internal LightningException(string message) : base (message)
        {
        }
    }
}
