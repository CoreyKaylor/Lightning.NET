namespace LightningDB
{
    public class Stats
    {
        public long PageSize { get; set; }
        public long BTreeDepth { get; set; }
        public long BranchPages { get; set; }
        public long LeafPages { get; set; }
        public long OverflowPages { get; set; }
        public long Entries { get; set; }
    }
}