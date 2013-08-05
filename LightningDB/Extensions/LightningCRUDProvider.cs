using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningDB.Extensions
{
    public abstract class LightningCRUDProvider<TKey>
    {
        private static class EmitConverterBase<TFrom, TTo>
        {
            internal static Func<TFrom, TTo> CreateEmitConverter(bool convert)
            {
                var method = new DynamicMethod(string.Empty, typeof(TTo), new[] { typeof(TTo) });
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                if (convert)
                    il.Emit(OpCodes.Conv_R8);
                il.Emit(OpCodes.Ret);

                return (Func<TFrom, TTo>)method.CreateDelegate(typeof(Func<TFrom, TTo>));
            }
        }

        private static class EmitConverter<TFrom, TTo>
        {
            internal readonly static Func<TFrom, TTo> Convert = EmitConverterBase<TFrom, TTo>.CreateEmitConverter(false);
        }

        private static class EmitEnumConverter<TFrom, TTo>
        {
            internal readonly static Func<TFrom, TTo> Convert = EmitConverterBase<TFrom, TTo>.CreateEmitConverter(true);
        }

        private const string InvalidByteCountText = "Invalid byte count";

        protected abstract byte[] GetKeyBytes(IDatabaseAttributesProvider db, TKey key);

        protected abstract TKey GetKeyFromBytes(IDatabaseAttributesProvider db, byte[] key);

        public string GetString(Encoding encoding, byte[] bytes)
        {
            return encoding.GetString(bytes);
        }

        public Int16 GetInt16(byte[] bytes)
        {
            if (bytes.Length != sizeof(Int16))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToInt16(bytes, 0);
        }

        public Int32 GetInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(Int32))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToInt32(bytes, 0);
        }

        public Int64 GetInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(Int64))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToInt64(bytes, 0);
        }

        public UInt16 GetUInt16(byte[] bytes)
        {            
            if (bytes.Length != sizeof(UInt16))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToUInt16(bytes, 0);
        }

        public UInt32 GetUInt32(byte[] bytes)
        {
            if (bytes.Length != sizeof(UInt32))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public UInt64 GetUInt64(byte[] bytes)
        {
            if (bytes.Length != sizeof(UInt64))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToUInt64(bytes, 0);
        }

        public Char GetChar(byte[] bytes)
        {
            if (bytes.Length != sizeof(Char))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToChar(bytes, 0);
        }

        public Byte GetByte(byte[] bytes)
        {
            if (bytes.Length != sizeof(Byte))
                throw new InvalidCastException(InvalidByteCountText);

            return bytes[0];
        }

        public SByte GetSByte(byte[] bytes)
        {
            if (bytes.Length != sizeof(Byte))
                throw new InvalidCastException(InvalidByteCountText);

            return (SByte) bytes[0];
        }

        public Boolean GetBoolean(byte[] bytes)
        {            
            if (bytes.Length != sizeof(Boolean))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToBoolean(bytes, 0);
        }

        public Single GetSingle(byte[] bytes)
        {            
            if (bytes.Length != sizeof(Single))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToSingle(bytes, 0);
        }

        public Double GetDouble(byte[] bytes)
        {            
            if (bytes.Length != sizeof(Double))
                throw new InvalidCastException(InvalidByteCountText);

            return BitConverter.ToDouble(bytes, 0);
        }

        public Guid GetGuid(byte[] bytes)
        {            
            if (bytes.Length != 16)
                throw new InvalidCastException(InvalidByteCountText);

            return new Guid(bytes);
        }

        public TEnum GetEnum<TEnum>(byte[] bytes)
            where TEnum: struct
        {
            return this.GetEnumInternal<TEnum>(bytes);
        }

        private TEnum GetEnumInternal<TEnum>(byte[] bytes)
        {
            var enumType = Enum.GetUnderlyingType(typeof(TEnum));
                        
            switch (bytes.Length)
            {
                case 1:
                    return enumType == typeof(sbyte)
                        ? EmitEnumConverter<sbyte, TEnum>.Convert((sbyte)bytes[0])
                        : EmitEnumConverter<byte, TEnum>.Convert(bytes[0]);
                case 2:
                    return enumType == typeof(ushort)
                        ? EmitEnumConverter<ushort, TEnum>.Convert(bytes[0])
                        : EmitEnumConverter<short, TEnum>.Convert(bytes[0]);
                case 4:
                    return enumType == typeof(ushort)
                        ? EmitEnumConverter<uint, TEnum>.Convert(bytes[0])
                        : EmitEnumConverter<int, TEnum>.Convert(bytes[0]);
                case 8:
                    return enumType == typeof(ulong)
                        ? EmitEnumConverter<ulong, TEnum>.Convert(bytes[0])
                        : EmitEnumConverter<long, TEnum>.Convert(bytes[0]);
                default:
                    throw new InvalidCastException(InvalidByteCountText);
            }
        }

        private byte[] GetEnumBytes<TEnum>(TEnum value)
        {
            var enumType = Enum.GetUnderlyingType(typeof(TEnum));
            if (enumType.Equals(typeof(sbyte)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, sbyte>.Convert(value));
            if (enumType.Equals(typeof(byte)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, byte>.Convert(value));
            if (enumType.Equals(typeof(short)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, short>.Convert(value));
            if (enumType.Equals(typeof(ushort)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, ushort>.Convert(value));
            if (enumType.Equals(typeof(int)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, int>.Convert(value));
            if (enumType.Equals(typeof(uint)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, uint>.Convert(value));
            if (enumType.Equals(typeof(long)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, long>.Convert(value));
            if (enumType.Equals(typeof(ulong)))
                return BitConverter.GetBytes(EmitEnumConverter<TEnum, ulong>.Convert(value));

            throw new InvalidCastException(InvalidByteCountText);
        }

        public TValue Get<TValue>(LightningDatabase db, TKey key)
        {
            var keyBytes = this.GetKeyBytes(db, key);
            var valueBytes = db.Get(keyBytes);

            return this.GetValueFromBytes<TValue>(db, valueBytes);
        }

        public KeyValuePair<TKey, TValue> Get<TValue>(LightningCursor cursor, CursorOperation operation)
        {
            var pair = cursor.Get(operation);

            var key = this.GetKeyFromBytes(cursor, pair.Key);
            var value = this.GetValueFromBytes<TValue>(cursor, pair.Value);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        private byte[] GetValueBytes<TValue>(IDatabaseAttributesProvider db, TValue value)
        {
            var valueType = typeof(TValue);

            //string
            if (valueType.Equals(typeof(string)))
                return db.Encoding.GetBytes((string)((object)value));
            //bool
            if (valueType.Equals(typeof(Boolean)))
                return BitConverter.GetBytes(EmitConverter<TValue, Boolean>.Convert(value));
            //sbyte
            if (valueType.Equals(typeof(SByte)))
                return BitConverter.GetBytes(EmitConverter<TValue, SByte>.Convert(value));
            //byte
            if (valueType.Equals(typeof(Byte)))
                return BitConverter.GetBytes(EmitConverter<TValue, Byte>.Convert(value));
            //char
            if (valueType.Equals(typeof(Char)))
                return BitConverter.GetBytes(EmitConverter<TValue, Char>.Convert(value));
            //short
            if (valueType.Equals(typeof(Int16)))
                return BitConverter.GetBytes(EmitConverter<TValue, Int16>.Convert(value));
            //ushort
            if (valueType.Equals(typeof(UInt16)))
                return BitConverter.GetBytes(EmitConverter<TValue, UInt16>.Convert(value));
            //int
            if (valueType.Equals(typeof(Int32)))
                return BitConverter.GetBytes(EmitConverter<TValue, Int32>.Convert(value));
            //uint
            if (valueType.Equals(typeof(UInt32)))
                return BitConverter.GetBytes(EmitConverter<TValue, UInt32>.Convert(value));
            //long
            if (valueType.Equals(typeof(Int64)))
                return BitConverter.GetBytes(EmitConverter<TValue, Int64>.Convert(value));
            //ulong
            if (valueType.Equals(typeof(UInt64)))
                return BitConverter.GetBytes(EmitConverter<TValue, UInt64>.Convert(value));
            //float
            if (valueType.Equals(typeof(Single)))
                return BitConverter.GetBytes(EmitConverter<TValue, Single>.Convert(value));
            //double
            if (valueType.Equals(typeof(Double)))
                return BitConverter.GetBytes(EmitConverter<TValue, Double>.Convert(value));
            //enum
            if (typeof(Enum).IsAssignableFrom(valueType))
                return this.GetEnumBytes(value);
            //Guid
            if (typeof(Guid).Equals(valueType))
                return EmitConverter<TValue, Guid>.Convert(value).ToByteArray();

            throw new NotSupportedException("Type " + valueType.FullName + " is not supported");
        }

        public TValue GetValueFromBytes<TValue>(IDatabaseAttributesProvider db, byte[] bytes)
        {
            var valueType = typeof(TValue);

            //string
            if (valueType.Equals(typeof(string)))
                return (TValue)((object)this.GetString(db.Encoding, bytes));
            //bool
            if (valueType.Equals(typeof(Boolean)))
                return EmitConverter<Boolean, TValue>.Convert(this.GetBoolean(bytes));
            //sbyte
            if (valueType.Equals(typeof(SByte)))
                return EmitConverter<SByte, TValue>.Convert(this.GetSByte(bytes));
            //byte
            if (valueType.Equals(typeof(Byte)))
                return EmitConverter<Byte, TValue>.Convert(this.GetByte(bytes));
            //char
            if (valueType.Equals(typeof(Char)))
                return EmitConverter<Char, TValue>.Convert(this.GetChar(bytes));
            //short
            if (valueType.Equals(typeof(Int16)))
                return EmitConverter<Int16, TValue>.Convert(this.GetInt16(bytes));
            //ushort
            if (valueType.Equals(typeof(UInt16)))
                return EmitConverter<UInt16, TValue>.Convert(this.GetUInt16(bytes));
            //int
            if (valueType.Equals(typeof(Int32)))
                return EmitConverter<Int32, TValue>.Convert(this.GetInt32(bytes));
            //uint
            if (valueType.Equals(typeof(UInt32)))
                return EmitConverter<UInt32, TValue>.Convert(this.GetUInt32(bytes));
            //long
            if (valueType.Equals(typeof(Int64)))
                return EmitConverter<Int64, TValue>.Convert(this.GetInt64(bytes));
            //ulong
            if (valueType.Equals(typeof(UInt64)))
                return EmitConverter<UInt64, TValue>.Convert(this.GetUInt64(bytes));
            //float
            if (valueType.Equals(typeof(Single)))
                return EmitConverter<Single, TValue>.Convert(this.GetSingle(bytes));
            //double
            if (valueType.Equals(typeof(Double)))
                return EmitConverter<Double, TValue>.Convert(this.GetDouble(bytes));
            //enum
            if (typeof(Enum).IsAssignableFrom(valueType))
                return this.GetEnumInternal<TValue>(bytes);
            //Guid
            if (typeof(Guid).Equals(valueType))
                return EmitConverter<Guid, TValue>.Convert(this.GetGuid(bytes));

            throw new NotSupportedException("Type " + valueType.FullName + " is not supported");
        }

        public void Delete<TValue>(LightningDatabase db, TKey key, TValue value)
        {
            var keyBytes = this.GetKeyBytes(db, key);
            var valueBytes = this.GetValueBytes(db, value);

            db.Delete(keyBytes, valueBytes);
        }

        public void Delete(LightningDatabase db, TKey key)
        {
            var keyBytes = this.GetKeyBytes(db, key);

            db.Delete(keyBytes, null);
        }

        public void Put<TValue>(IPutter db, TKey key, TValue value, PutOptions options)
        {
            var keyBytes = this.GetKeyBytes(db, key);
            var valueBytes = this.GetValueBytes(db, value);

            db.Put(keyBytes, valueBytes, options);
        }
    }
}
