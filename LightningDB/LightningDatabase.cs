using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightningDB
{
    public class LightningDatabase : IDatabaseAttributesProvider, IPutter, IDisposable
    {
        public const string DefaultDatabaseName = "master";

        internal UInt32 _handle;

        private string _name;
        private bool _shouldDispose;
        private EventHandler<LightningClosingEventArgs> _transactionClosing;

        public LightningDatabase(string name, DatabaseOpenFlags flags, LightningTransaction tran)
            : this (name, flags, tran, Encoding.UTF8)
        {
        }

        public LightningDatabase(string name, DatabaseOpenFlags flags, LightningTransaction tran, Encoding encoding)
        {
            if (tran == null)
                throw new ArgumentNullException("tran");

            UInt32 handle;
            var res = Native.mdb_dbi_open(tran._handle, name, flags, out handle);
            if (res != 0)
                throw new LightningException(res);

            _name = name ?? DefaultDatabaseName;

            _handle = handle;
            _shouldDispose = true;
                        
            this.IsOpened = true;
            this.Encoding = encoding;
            this.Transaction = tran;
            this.OpenFlags = flags;

            _transactionClosing = new EventHandler<LightningClosingEventArgs>(this.TransactionClosing);
            this.Transaction.Closing += _transactionClosing;
        }

        private void TransactionClosing(object sender, LightningClosingEventArgs e)
        {
            try
            {
                this.Close(e.EnvironmentClosing);
            }
            catch { }
        }

        public bool IsOpened { get; private set; }

        public string Name { get { return _name; } }

        public Encoding Encoding { get; private set; }

        public LightningEnvironment Environment { get { return this.Transaction.Environment; } }

        public LightningTransaction Transaction { get; private set; }

        public DatabaseOpenFlags OpenFlags { get; private set; }

        public void DropDatabase(bool delete)
        {
            var res = Native.mdb_drop(this.Transaction._handle, _handle, delete);
            if (res != 0)
                throw new LightningException(res);

            this.Close(false);
        }

        public byte[] Get(byte[] key)
        {
            var keyStructure = new ValueStructure
            {
                data = Marshal.AllocHGlobal(key.Length),
                size = key.Length
            };

            try
            {
                Marshal.Copy(key, 0, keyStructure.data, key.Length);

                ValueStructure value;
                var res = Native.mdb_get(this.Transaction._handle, this._handle, ref keyStructure, out value);
                if (res == Native.MDB_NOTFOUND)
                    return null;
                else if (res != 0)
                    throw new LightningException(res);

                var buffer = new byte[value.size];
                Marshal.Copy(value.data, buffer, 0, value.size);

                //TODO: Possible leak. Is the original data which is copied to buffer stays in memory?
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(keyStructure.data);
            }
        }

        public void Put(byte[] key, byte[] value, PutOptions options)
        {
            var keyStruct = new ValueStructure
            {
                data = Marshal.AllocHGlobal(key.Length),
                size = key.Length
            };

            var valueStruct = new ValueStructure
            {
                data = Marshal.AllocHGlobal(value.Length),
                size = value.Length
            };

            try
            {
                Marshal.Copy(key, 0, keyStruct.data, key.Length);
                Marshal.Copy(value, 0, valueStruct.data, value.Length);

                var res = Native.mdb_put(this.Transaction._handle, _handle, ref keyStruct, ref valueStruct, options);
                if (res != 0)
                    throw new LightningException(res);
            }
            finally
            {
                Marshal.FreeHGlobal(keyStruct.data);
                Marshal.FreeHGlobal(valueStruct.data);
            }
        }

        public void Delete(byte[] key, byte[] value)
        {
            var keyStructure = new ValueStructure
            {
                data = Marshal.AllocHGlobal(key.Length),
                size = key.Length
            };

            int res;

            try
            {
                Marshal.Copy(key, 0, keyStructure.data, key.Length);
                
                if (value != null)
                {
                    var valueStructure = new ValueStructure
                    {
                        data = Marshal.AllocHGlobal(value.Length),
                        size = value.Length
                    };

                    try
                    {
                        Marshal.Copy(value, 0, valueStructure.data, value.Length);

                        res = Native.mdb_del(this.Transaction._handle, _handle, ref keyStructure, ref valueStructure);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(valueStructure.data);
                    }
                }
                else
                {
                    res = Native.mdb_del(this.Transaction._handle, _handle, ref keyStructure, IntPtr.Zero);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(keyStructure.data);
            }

            if (res != 0)
                throw new LightningException(res);
        }

        public void Close()
        {
            this.Close(true);
        }

        private void Close(bool releaseHandle)
        {
            lock (this.Environment)
            {
                if (!this.IsOpened)
                    return;

                try
                {
                    if (releaseHandle)
                        Native.mdb_dbi_close(this.Environment._handle, _handle);
                }
                finally
                {
                    this.IsOpened = false;

                    this.Environment.ReuseDatabase(this);
                    if (releaseHandle)
                        this.Environment.ReleaseDatabase(this);

                    this.Transaction.Closing -= _transactionClosing;

                    if (releaseHandle)
                        _shouldDispose = false;
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
