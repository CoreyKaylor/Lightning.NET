using System;

namespace LightningDB
{
    public interface IClosingEventSource
    {
        event EventHandler<LightningClosingEventArgs> Closing;
    }
}
