namespace LightningDB
{
    public class LightningVersionInfo
    {
        public LightningVersionInfo()
        {
            int minor, major, patch;
            this.Version = Native.mdb_version(out major, out minor, out patch);

            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string Version { get; private set; }
    }
}
