using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public interface IDatabaseAttributesProvider
    {
        DatabaseOpenFlags OpenFlags { get; }

        string Name { get; }

        Encoding Encoding { get; }
    }
}
