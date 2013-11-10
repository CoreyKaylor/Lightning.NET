using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LightningDB.Converters;

namespace LightningDB
{
    public class LightningEnvironment : IClosingEventSource, IDisposable
    {
        private readonly UnixAccessMode _accessMode;
        private readonly EnvironmentOpenFlags _openFlags;
        internal IntPtr _handle;

        private bool _shouldDispose;

        private long _mapSize;
        private int _maxDbs;

        private readonly ConcurrentDictionary<string, LightningDatabase> _openedDatabases;
        private readonly HashSet<uint> _databasesForReuse;

        public LightningEnvironment(string directory, EnvironmentOpenFlags openFlags, UnixAccessMode accessMode = UnixAccessMode.Default)
        {
            if (String.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Invalid directory name");

            IntPtr handle = default(IntPtr);
            Native.Execute(lib => lib.mdb_env_create(out handle));

            _shouldDispose = true;
            
            _handle = handle;
            _accessMode = accessMode;

            this.Directory = directory;
            _openFlags = openFlags;

            if (LightningConfig.Environment.DefaultMapSize != LightningConfig.Environment.LibDefaultMapSize)
                this.MapSize = LightningConfig.Environment.DefaultMapSize;
            else
                _mapSize = LightningConfig.Environment.LibDefaultMapSize;

            if (LightningConfig.Environment.DefaultMaxDatabases != LightningConfig.Environment.LibDefaultMaxDatabases)
                this.MapDatabases = LightningConfig.Environment.DefaultMaxDatabases;
            else
                _maxDbs = LightningConfig.Environment.LibDefaultMaxDatabases;

            _openedDatabases = new ConcurrentDictionary<string, LightningDatabase>();
            _databasesForReuse = new HashSet<uint>();

            ConverterStore = new ConverterStore();
            var defaultConverters = new DefaultConverters();
            defaultConverters.RegisterDefault(this);
        }

        public event EventHandler<LightningClosingEventArgs> Closing;

        public bool IsOpened { get; private set; }

        public LightningVersionInfo Version { get { return Native.LibraryVersion; } }

        public long MapSize
        {
            get { return _mapSize; }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MapSize of opened environment");

                if (value == _mapSize) 
                    return;                

                Native.Execute(lib => lib.mdb_env_set_mapsize(_handle, value));

                _mapSize = value;
            }
        }

        public int MaxReaders
        {
            get
            {
                UInt32 readers = default(UInt32);
                Native.Execute(lib => lib.mdb_env_get_maxreaders(_handle, out readers));

                return (int)readers;
            }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MaxReaders of opened environment");

                Native.Execute(lib => lib.mdb_env_set_maxreaders(_handle, (UInt32)value));
            }
        }

        public int MapDatabases
        {
            get { return _maxDbs; }
            set
            {
                if (this.IsOpened)
                    throw new InvalidOperationException("Can't change MaxDatabases of opened environment");

                if (value == _maxDbs) 
                    return;

                Native.Execute(lib => lib.mdb_env_set_maxdbs(_handle, (UInt32)value));

                _maxDbs = value;
            }
        }

        public string Directory { get; private set; }

        public ConverterStore ConverterStore { get; private set; }

        public void Open()
        {
            if (!System.IO.Directory.Exists(this.Directory))
                System.IO.Directory.CreateDirectory(this.Directory);

            if (!this.IsOpened)
				Native.Execute(lib => lib.mdb_env_open(_handle, this.Directory, _openFlags, _accessMode));

            this.IsOpened = true;
        }

        public void Close()
        {
            if (!this.IsOpened)
                return;

            this.OnClosing();

            foreach (var hdb in _databasesForReuse)
                Native.Library.mdb_dbi_close(_handle, hdb);

            Native.Library.mdb_env_close(_handle);

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

            return db;
        }

        public void CopyTo(string path)
        {
            this.EnsureOpened();

            Native.Execute(lib => lib.mdb_env_copy(_handle, path));
        }

        //TODO: tests
        public void Flush(bool force)
        {
            Native.Execute(lib => lib.mdb_env_sync(_handle, force));
        }

        private void EnsureOpened()
        {
            if (!this.IsOpened)
                throw new InvalidOperationException("Environment should be opened");
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (!shouldDispose || IntPtr.Zero.Equals(_handle)) 
                return;
            try
            {
                this.Close();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}