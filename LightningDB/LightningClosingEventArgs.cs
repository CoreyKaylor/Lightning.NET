using System;

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
