using System;
using System.Runtime.InteropServices;

namespace LightningDB
{
    internal class MarshalValueStructure : IDisposable
    {
        private bool _shouldDispose;
		private byte[] _value;

		private ValueStructure _structure;

        public MarshalValueStructure(byte[] value)
        {
			if (value == null)
				throw new ArgumentNullException ("value");

            _shouldDispose = true;
			_value = value;

            this.ValueInitialized = false;
        }

        public bool ValueInitialized { get; private set; }

        //Lazy initialization prevents possible leak.
        //If initialization was in ctor, Dispose could never be called
        //due to possible exception thrown by Marshal.Copy
		public ValueStructure ValueStructure 
        { 
            get 
            {
                if (!ValueInitialized)
                {
                    _structure = new ValueStructure
                    {
                        data = Marshal.AllocHGlobal(_value.Length),
                        size = _value.Length
                    };

                    try
                    {
                        Marshal.Copy(_value, 0, _structure.data, _value.Length);
                    }
                    finally
                    {
                        this.Dispose();
                    }

                    ValueInitialized = true;
                }

                return _structure; 
            } 
        }

        protected virtual void Dispose(bool shouldDispose)
        {
            if (!shouldDispose)
                return;

            try
			{
            	Marshal.FreeHGlobal(ValueStructure.data);
			}
			catch {}
        }

        public void Dispose()
        {
            this.Dispose(_shouldDispose);
            _shouldDispose = false;
        }
    }
}