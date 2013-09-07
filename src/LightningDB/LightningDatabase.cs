using System;
using System.Text;

namespace LightningDB
{
    public class LightningDatabase : IDisposable
    {
        public const string DefaultDatabaseName = "master";

        internal UInt32 _handle;

        private readonly string _name;
        private bool _shouldDispose;

        public LightningDatabase(string name, DatabaseOpenFlags flags, LightningTransaction tran)
            : this (name, flags, tran, Encoding.UTF8)
        {
        }

        public LightningDatabase(string name, DatabaseOpenFlags flags, LightningTransaction tran, Encoding encoding)
        {
            if (tran == null)
                throw new ArgumentNullException("tran");

            UInt32 handle = default(UInt32);
            Native.Execute(lib => lib.mdb_dbi_open(tran._handle, name, flags, out handle));

            _name = name ?? DefaultDatabaseName;

            _handle = handle;
            _shouldDispose = true;
                        
            this.IsOpened = true;
            this.Encoding = encoding;
            this.OpenFlags = flags;
            this.Environment = tran.Environment;
        }

        public bool IsOpened { get; private set; }

        public string Name { get { return _name; } }

        public Encoding Encoding { get; private set; }

        public LightningEnvironment Environment { get; private set; }

        public DatabaseOpenFlags OpenFlags { get; private set; }

        public void Close()
        {
            this.Close(true);
        }

        internal void Close(bool releaseHandle)
        {
            lock (this.Environment)
            {
                if (!this.IsOpened)
                    return;

                try
                {
                    if (releaseHandle)
                        Native.Library.mdb_dbi_close(this.Environment._handle, _handle);
                }
                finally
                {
                    this.IsOpened = false;

                    this.Environment.ReuseDatabase(this);
                    if (releaseHandle)
                    {
                        this.Environment.ReleaseDatabase(this);
                        _shouldDispose = false;
                    }
                }
            }
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (!shouldDispose)
                return;

            this.Close(true);
        }

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
