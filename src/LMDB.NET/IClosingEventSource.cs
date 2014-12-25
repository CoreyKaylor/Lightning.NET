using System;

namespace LMDB
{
    /// <summary>
    /// Contains Closing event.
    /// </summary>
    public interface IClosingEventSource
    {
        /// <summary>
        /// Triggers when hen closable object is closing.
        /// </summary>
        event EventHandler<LightningClosingEventArgs> Closing;
    }
}
