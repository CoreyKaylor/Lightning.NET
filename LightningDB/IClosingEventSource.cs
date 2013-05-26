using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public interface IClosingEventSource
    {
        event EventHandler<LightningClosingEventArgs> Closing;
    }
}
