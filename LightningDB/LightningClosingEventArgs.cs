using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public class LightningClosingEventArgs : EventArgs
    {
        public bool EnvironmentClosing { get; private set; }

        public LightningClosingEventArgs(bool environmentClosing)
        {
            this.EnvironmentClosing = environmentClosing;
        }
    }
}
