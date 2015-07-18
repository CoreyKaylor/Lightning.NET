using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB
{
	/// <summary>
	/// Lightning compare delegate.
	/// </summary>
    public delegate int LightningCompareDelegate(LightningDatabase db, byte[] left, byte[] right);
}
