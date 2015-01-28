using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal class MarshalMultipleValueStructure : IDisposable
    {
        private bool _shouldDispose;
        private byte[][] _values;

        private ValueStructure[] _structures;

        public MarshalMultipleValueStructure(byte[][] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            _shouldDispose = false;
            _values = values;

            this.ValueInitialized = false;
        }

        public bool ValueInitialized { get; private set; }

        private int GetSize()
        {
            if (_values.Length == 0 || _values[0] == null || _values[0].Length == 0)
                return 0;

            return _values[0].Length;
        }

        private int GetCount()
        {
            return _values.Length;
        }

        //Lazy initialization prevents possible leak.
        //If initialization was in ctor, Dispose could never be called
        //due to possible exception thrown by Marshal.Copy
        public ValueStructure[] ValueStructures
        {
            get
            {
                if (!this.ValueInitialized)
                {
                    var size = GetSize();
                    var count = GetCount();
                    var totalLength = size * count;
                    
                    _structures = new [] 
                    {
                        new ValueStructure 
                        { 
                            size = new IntPtr(size), 
                            data = Marshal.AllocHGlobal(totalLength) 
                        },
                        new ValueStructure 
                        { 
                            size = new IntPtr(count )
                        }
                    };
                    
                    _shouldDispose = true;

                    try
                    {
                        var baseAddress = _structures[0].data.ToInt64();
                        for (var i = 0; i < count; i++)
                        {
                            if (_values[i].Length != size)
                                throw new InvalidOperationException("all data items should be of the same length");

                            var address = new IntPtr(baseAddress + i * size);

                            Marshal.Copy(_values[i], 0, address, size);
                        }                        
                    }
                    catch
                    {
                        this.Dispose();
                    }

                    this.ValueInitialized = true;
                }

                return _structures;
            }
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (!shouldDispose)
                return;

            try
            {
                Marshal.FreeHGlobal(_structures[0].data);
            }
            catch { }
        }

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}
