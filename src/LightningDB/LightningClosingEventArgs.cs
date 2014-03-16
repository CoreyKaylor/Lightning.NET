using System;

namespace LightningDB
{
    /// <summary>
    /// Event arguments for environment's closing events
    /// </summary>
    public class LightningClosingEventArgs : EventArgs
    {
        /// <summary>
        /// True if closing is triggered by LightningEnvironment.Close.
        /// </summary>
        public bool EnvironmentClosing { get; private set; }

        /// <summary>
        /// Creates new LightningClosingEventArgs instance.
        /// </summary>
        /// <param name="environmentClosing">True if closing is triggered by LightningEnvironment.Close</param>
        public LightningClosingEventArgs(bool environmentClosing)
        {
            this.EnvironmentClosing = environmentClosing;
        }
    }
}
