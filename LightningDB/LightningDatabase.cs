using System;
using System.Text;
using LightningDB.BasicExtensions;

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

            UInt32 handle = default(UInt32);
            Native.Execute(() => Native.mdb_dbi_open(tran._handle, name, flags, out handle));

            _name = name ?? DefaultDatabaseName;

            _handle = handle;
            _shouldDispose = true;
                        
            this.IsOpened = true;
            this.Encoding = encoding;
            this.Transaction = tran;
            this.OpenFlags = flags;

            _transactionClosing = this.TransactionClosing;
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
            Native.Execute(() => Native.mdb_drop(this.Transaction._handle, _handle, delete));

            this.Close(false);
        }

        public byte[] Get(byte[] key)
        {
            using (var keyMarshalStruct = new MarshalValueStructure(key))
            {
                var value = default(ValueStructure);
                var keyStructure = keyMarshalStruct.ValueStructure;
                var res = Native.Read(() => Native.mdb_get(Transaction._handle, _handle, ref keyStructure, out value));
                return value.ToByteArray(res);
            }
        }

        public void Put(byte[] key, byte[] value, PutOptions options)
        {
            using (var keyStructureMarshal = new MarshalValueStructure(key))
            using (var valueStructureMarshal = new MarshalValueStructure(value))
            {
                var keyStruct = keyStructureMarshal.ValueStructure;
                var valueStruct = valueStructureMarshal.ValueStructure;

                Native.Execute(() => Native.mdb_put(this.Transaction._handle, _handle, ref keyStruct, ref valueStruct, options));
            }
        }

        public void Delete(byte[] key, byte[] value)
        {
            using (var keyMarshalStruct = new MarshalValueStructure(key))
            {
                var keyStructure = keyMarshalStruct.ValueStructure;
                if (value != null)
                {
                    using (var valueMarshalStruct = new MarshalValueStructure(value))
                    {
                        var valueStructure = valueMarshalStruct.ValueStructure;
                        Native.Execute(() => Native.mdb_del(this.Transaction._handle, _handle, ref keyStructure, ref valueStructure));
                        return;
                    }
                }
                Native.Execute(() => Native.mdb_del(this.Transaction._handle, _handle, ref keyStructure, IntPtr.Zero));
            }
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
