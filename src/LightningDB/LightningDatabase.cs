using System;
using System.Text;
using LightningDB.Factories;
using LightningDB.Native;

namespace LightningDB
{
    /// <summary>
    /// Lightning database.
    /// </summary>
    public class LightningDatabase : IDisposable
    {
        /// <summary>
        /// Database name by default.
        /// </summary>
        public const string DefaultDatabaseName = "master";

        internal UInt32 _handle;

        private readonly string _name;
        private bool _shouldDispose;

        /// <summary>
        /// Creates a LightningDatabase instance.
        /// </summary>
        /// <param name="name">Database name.</param>
        /// <param name="flags">Database open flags/</param>
        /// <param name="tran">Active transaction.</param>
        /// <param name="encoding">Default strings encoding.</param>
        internal LightningDatabase(string name, LightningTransaction tran, DatabaseHandleCacheEntry entry, Encoding encoding)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (tran == null)
                throw new ArgumentNullException("tran");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _name = name;
            _handle = entry.Handle;
            _shouldDispose = true;
                        
            this.Encoding = encoding;
            this.OpenFlags = entry.OpenFlags;
            this.Environment = tran.Environment;
        }

        internal bool IsReleased { get { return !_shouldDispose; } }

        /// <summary>
        /// Is database opened.
        /// </summary>
        public bool IsOpened { get { return this.Environment.DatabaseManager.IsOpen(this); } }

        /// <summary>
        /// Database name.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Default strings encoding.
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// Environment in which the database was opened.
        /// </summary>
        public LightningEnvironment Environment { get; private set; }

        /// <summary>
        /// Flags with which the database was opened.
        /// </summary>
        public DatabaseOpenFlags OpenFlags { get; private set; }

        /// <summary>
        /// Clode the database.
        /// </summary>
        public void Close()
        {
            this.Close(true);
        }

        internal void Close(bool releaseHandle)
        {
            this.Environment.DatabaseManager.Close(this, releaseHandle);
            _shouldDispose = false;
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        /// <param name="shouldDispose">true if not disposed yet.</param>
        protected virtual void Dispose(bool shouldDispose)
        {
            if (!shouldDispose)
                return;

            this.Close(true);
        }

        /// <summary>
        /// Deallocates resources opeened by the database.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
