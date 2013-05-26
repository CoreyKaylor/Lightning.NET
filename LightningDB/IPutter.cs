using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public interface IPutter : IDatabaseAttributesProvider
    {
        void Put(byte[] key, byte[] value, PutOptions options);
    }
}
