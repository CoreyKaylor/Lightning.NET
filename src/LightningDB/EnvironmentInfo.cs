namespace LightningDB
{
    /// <summary>
    /// Information about the environment. 
    /// </summary>
    public class EnvironmentInfo
    {
        /// <summary>
        /// ID of the last used page 
        /// </summary>
        public int LastPageNumber { get; set; }

        /// <summary>
        /// ID of the last committed transaction  
        /// </summary>
        public int LastTransactionId { get; set; }

        /// <summary>
        /// Size of the data memory map 
        /// </summary>
        public int MapSize { get; set; }
    }
}