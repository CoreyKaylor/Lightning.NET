namespace LightningDB
{
    /// <summary>
    /// A struct describing the result of a TryGet operation
    /// </summary>
    public readonly struct GetResult
    {
        public GetResult(GetResultCode resultCode, int valueLength)
        {
            ResultCode = resultCode;
            ValueLength = valueLength;
        }

        /// <summary>
        /// A success/error code for the Get operation
        /// </summary>
        public GetResultCode ResultCode { get; }

        /// <summary>
        /// If the key was found, this represents the length of the value
        /// in the database.
        /// 
        /// <para>
        ///     If the key was not found, this value is 0 and has no meaning.
        /// </para>
        /// </summary>
        public int ValueLength { get; }
    }

    /// <summary>
    /// An enumeration describing the result of a TryGet operation
    /// </summary>
    public enum GetResultCode
    {
        /// <summary>
        /// Success! The requested key was found and the value was copied to 
        /// the provided buffer.
        /// <para>
        ///     GetResult.Length represents the length of the value found in
        ///     the database. (This is also the length of data copied into
        ///     the provided buffer.)
        /// </para>
        /// </summary>
        Success,

        /// <summary>
        /// Failure. The requested key was found, but the provided buffer was
        /// too small to contain the full value. No data has been retrived.
        /// The Get operation should be retried with a buffer at least the 
        /// length represented in GetResult.Length
        /// <para>
        ///     GetResult.Length represents the length of the value found in
        ///     the database. (This is the minimum buffer length needed for 
        ///     the operation to succeed)
        /// </para>
        /// </summary>
        DestinationTooSmall,

        /// <summary>
        /// Failure. The requested key was not found in the database. No data has 
        /// been retrived.
        /// <para>
        ///     GetResult.Length has no meaning for this result. It's value
        ///     is set to zero.
        /// </para>
        /// </summary>
        KeyNotFound
    }

}
