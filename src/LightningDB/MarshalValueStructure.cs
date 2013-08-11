using System;
using System.Runtime.InteropServices;

namespace LightningDB
{
    internal class MarshalValueStructure : IDisposable
    {
		private byte[] _value;
		private Lazy<ValueStructure> _structure;

        public MarshalValueStructure(byte[] value)
        {
			if (value == null)
				throw new ArgumentNullException ("value");

			_value = value;

			//Lazy initialization prevents possible leak.
			//If initialization was in ctor, Dispose could never be called
			//due to possible exception thrown by Marshal.Copy
			_structure = new Lazy<ValueStructure> (this.CreateStructure);
        }

		private ValueStructure CreateStructure()
		{
			ValueStructure = new ValueStructure
			{
				data = Marshal.AllocHGlobal(value.Length),
				size = value.Length
			};

			Marshal.Copy(value, 0, ValueStructure.data, value.Length);
		}

		public ValueStructure ValueStructure { get { return _structure.Value; } }

        public void Dispose()
        {
			try
			{
            	Marshal.FreeHGlobal(ValueStructure.data);
			}
			catch {}
        }
    }
}