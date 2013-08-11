using LightningDB.BasicExtensions;
using System;

namespace LightningDB.Extensions
{
    //TODO: tests
    public class LightningCRUDIntKeyProvider : LightningCRUDProvider<Int32>
    {
        protected override byte[] GetKeyBytes(IDatabaseAttributesProvider db, int key)
        {
            return LightningDatabaseBasicExtensions.GetKey(db, key);
        }

        protected override int GetKeyFromBytes(IDatabaseAttributesProvider db, byte[] key)
        {
            return LightningDatabaseBasicExtensions.GetIntKey(db, key);
        }
    }
}
