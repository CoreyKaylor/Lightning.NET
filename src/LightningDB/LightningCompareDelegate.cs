using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
    public delegate int LightningCompareDelegate(LightningDatabase db, byte[] left, byte[] right);
}
