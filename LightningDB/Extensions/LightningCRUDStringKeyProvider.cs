using LightningDB.BasicExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB.Extensions
{
    //TODO: tests
    public class LightningCRUDStringKeyProvider : LightningCRUDProvider<string>
    {
        protected override byte[] GetKeyBytes(IDatabaseAttributesProvider db, string key)
        {
            return LightningDatabaseBasicExtensions.GetKey(db, key);
        }

        protected override string GetKeyFromBytes(IDatabaseAttributesProvider db, byte[] key)
        {
            return LightningDatabaseBasicExtensions.GetStringKey(db, key);
        }
    }
}
