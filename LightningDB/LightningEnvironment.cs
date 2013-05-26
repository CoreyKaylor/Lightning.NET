using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public class LightningEnvironment : IClosingEventSource, IDisposable
    {
        static LightningEnvironment()
        {
            _version = new Lazy<LightningVersionInfo>(() => new LightningVersionInfo());
        }

        private static Lazy<LightningVersionInfo> _version;

        public const int DefaultMapSize = 10485760;
        public const int DefaultMaxReaders = 126;
        public const int DefaultMaxDatabases = 0;
                
        private EnvironmentOpenFlags _openFlags;
        internal IntPtr _handle;

        private bool _shouldDispose;

        private int _mapSize;
        private int _maxDbs;

        private ConcurrentDictionary<string, LightningDatabase> _openedDatabases;
        private HashSet<uint> _databasesForReuse;

        public LightningEnvironment(string directory, EnvironmentOpenFlags openFlags)
        {
            if (String.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Invalid directory name");

            IntPtr handle;
            var res = Native.mdb_env_create(out handle);
            if (res != 0)
                throw new LightningException(res);

            _shouldDispose = true;
            
            _handle = handle;

            this.Directory = directory;
            _openFlags = openFlags;

            _mapSize = DefaultMapSize;
            _maxDbs = DefaultMaxDatabases;

            _openedDatabases = new ConcurrentDictionary<string, LightningDatabase>();
            _databasesForReuse = new HashSet<uint>();
        }

        public event EventHandler<LightningClosingEventArgs> Closing;

        public bool IsOpened { get; private set; }

        public LightningVersionInfo Version { get { return _version.Value; } }

        public int MapSize
        {
            get { return _mapSize; }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MapSize of opened environment");

                if (value != _mapSize)
                {
                    var res = Native.mdb_env_set_mapsize(_handle, value);
                    if (res != 0)
                        throw new LightningException(res);

                    _mapSize = value;
                }
            }
        }

        public int MaxReaders
        {
            get
            {
                UInt32 readers;
                var res = Native.mdb_env_get_maxreaders(_handle, out readers);
                if (res != 0)
                    throw new LightningException(res);

                return (int)readers;
            }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MaxReaders of opened environment");

                var res = Native.mdb_env_set_maxreaders(_handle, (UInt32) value);
                if (res != 0)
                    throw new LightningException(res);
            }
        }

        public int MapDatabases
        {
            get { return _maxDbs; }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MaxDatabases of opened environment");

                if (value != _maxDbs)
                {
                    var res = Native.mdb_env_set_maxdbs(_handle, (UInt32) value);
                    if (res != 0)
                        throw new LightningException(res);

                    _maxDbs = value;
                }
            }
        }

        public string Directory { get; private set; }

        public void Open()
        {
            if (!System.IO.Directory.Exists(this.Directory))
                System.IO.Directory.CreateDirectory(this.Directory);

            var res = Native.mdb_env_open(_handle, this.Directory, _openFlags, 666);
            if (res != 0)
                throw new LightningException(res);

            this.IsOpened = true;
        }

        public void Close()
        {
            if (!this.IsOpened)
                return;

            this.OnClosing();

            foreach (var hdb in _databasesForReuse)
                Native.mdb_dbi_close(_handle, hdb);

            Native.mdb_env_close(_handle);

            this.IsOpened = false;

            _shouldDispose = false;
        }

        protected virtual void OnClosing()
        {
            if (this.Closing != null)
                this.Closing(this, new LightningClosingEventArgs(true));
        }

        public LightningTransaction BeginTransaction(LightningTransaction parent, TransactionBeginFlags beginFlags)
        {
            this.EnsureOpened();

            return new LightningTransaction(this, parent, beginFlags);
        }

        public LightningTransaction BeginTransaction(TransactionBeginFlags beginFlags)
        {
            return this.BeginTransaction(null, beginFlags);
        }

        public LightningTransaction BeginTransaction()
        {
            return this.BeginTransaction(null, LightningTransaction.DefaultTransactionBeginFlags);
        }

        internal void ReuseDatabase(LightningDatabase db)
        {
            _openedDatabases.TryRemove(db.Name, out db);
        }

        internal void ReleaseDatabase(LightningDatabase db)
        {
            _databasesForReuse.Remove(db._handle);
        }

        //TODO: Upgrade db flags?
        internal LightningDatabase OpenDatabase(string name, DatabaseOpenFlags flags, LightningTransaction tran)
        {
            var internalName = name ?? LightningDatabase.DefaultDatabaseName;
            var db = _openedDatabases.GetOrAdd(internalName, n => 
            {
                var ldb = new LightningDatabase(name, flags, tran);
                _databasesForReuse.Add(ldb._handle);

                return ldb;
            });

            if (db.OpenFlags != flags)
                throw new InvalidOperationException("Database " + internalName + " already opened with different flags");

            if (db.Transaction != tran)
                throw new InvalidOperationException("Database " + internalName + " already opened in another transaction");

            return db;
        }

        public void CopyTo(string path)
        {
            this.EnsureOpened();

            var res = Native.mdb_env_copy(_handle, path);
            if (res != 0)
                throw new LightningException(res);
        }

        //TODO: tests
        public void Flush(bool force)
        {
            var res = Native.mdb_env_sync(_handle, force);
            if (res != 0)
                throw new LightningException(res);
        }

        private void EnsureOpened()
        {
            if (!this.IsOpened)
                throw new InvalidOperationException("Environment should be opened");
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (shouldDispose && !IntPtr.Zero.Equals(_handle))
            {
                try
                {
                    this.Close();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
