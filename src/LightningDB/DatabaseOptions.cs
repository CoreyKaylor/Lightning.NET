using System;
using System.Text;

namespace LightningDB
{
    public class DatabaseOptions
    {
        public DatabaseOptions()
        {
            Flags = LightningConfig.Database.DefaultOpenFlags;
            Encoding = LightningConfig.Database.DefaultEncoding;
        }

        #region IDatabaseOptions Members

        public Func<CompareFunctionBuilder, LightningCompareDelegate> Compare { get; set; }

        public Func<CompareFunctionBuilder, LightningCompareDelegate> DuplicatesSort { get; set; }

        public Encoding Encoding { get; set; }

        public DatabaseOpenFlags Flags { get; set; }

        #endregion
    }
}
