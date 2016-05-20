using System;

namespace LightningDB
{
    /// <summary>
    /// An exception caused by lmdb operations.
    /// </summary>
    public class LightningException : Exception
    {
        internal LightningException(string message, int statusCode) : base (message)
        {
            StatusCode = statusCode;
        }

        internal LightningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// The status code LMDB returned from an operation.
        /// </summary>
        public int StatusCode { get; }

        public override string ToString()
        {
            return $"LightningDB {StatusCode}: {Message}";
        }
    }
}
