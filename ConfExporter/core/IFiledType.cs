using System;
using NPOI.SS.UserModel;

namespace core
{
    public enum EFiledType
    {
        None,
        String,
        Numeric,
        DateTime,
        Boolean,
        CustomType,
    }

    public static class FiledTypeUtil
    {
        public static Type GetInternalType(this EFiledType type)
        {
            switch (type)
            {
                case EFiledType.String:
                    return typeof(string);
                case EFiledType.Numeric:
                    return typeof(double);
                case EFiledType.DateTime:
                    return typeof(DateTime);
                case EFiledType.Boolean:
                    return typeof(bool);
            }
            return null;
        }
        public static bool IsInternalType(this EFiledType type)
        {
            switch (type)
            {
                case EFiledType.String:
                case EFiledType.Numeric:
                case EFiledType.DateTime:
                case EFiledType.Boolean:
                    return true;
            }
            return false;
        }
    }
    public interface IFiledType
    {
        EFiledType TypeEnum { get; }
        bool TryDeduceType(ICell cell);
        object ParseValue(string v);
        bool IsInternalType { get; }
        bool IsArray { get; }
        bool IsDictionary { get; }
        bool IsMatch(string typeName);
        string FullTypeName { get; }
        object DefaultValue { get; }
    }

    public class BaseInternalFiledType<T> : IFiledType
    {
        private readonly Type _type = typeof(T);
        protected Type InternalType => _type;
        public virtual EFiledType TypeEnum
        {
            get
            {
                switch (_type)
                {
                    case Type t when t == typeof(string): return EFiledType.String;
                    case Type t when t == typeof(double): return EFiledType.Numeric;
                    case Type t when t == typeof(int): return EFiledType.Numeric;
                    case Type t when t == typeof(long): return EFiledType.Numeric;
                    case Type t when t == typeof(float): return EFiledType.Numeric;
                    case Type t when t == typeof(short): return EFiledType.Numeric;
                    case Type t when t == typeof(byte): return EFiledType.Numeric;
                    case Type t when t == typeof(DateTime): return EFiledType.DateTime;
                    case Type t when t == typeof(bool): return EFiledType.Boolean;
                    default: throw new Exception("Not support type: " + _type.Name);
                }
            }
        }

        public bool TryDeduceType(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (cell.CellStyle.DataFormat == 14)
                        return _type == typeof(DateTime);
                    else
                        return _type == typeof(double)
                               || _type == typeof(int)
                               || _type == typeof(long)
                               || _type == typeof(float)
                               || _type == typeof(short)
                               || _type == typeof(byte)
                               || _type == typeof(float);
                case CellType.String:
                    return _type == typeof(string);
                case CellType.Formula:
                    return _type == typeof(double)
                           || _type == typeof(int)
                           || _type == typeof(long)
                           || _type == typeof(float)
                           || _type == typeof(short)
                           || _type == typeof(byte)
                           || _type == typeof(float);
                case CellType.Boolean:
                    return _type == typeof(bool);
            }
            return false;
        }

        public object ParseValue(string v)
        {
            try
            {
                switch (_type)
                {
                    case Type t when t == typeof(string): return v;
                    case Type t when t == typeof(double): return double.Parse(v);
                    case Type t when t == typeof(float): return (float)double.Parse(v);
                    case Type t when t == typeof(int): return (int)double.Parse(v);
                    case Type t when t == typeof(short): return (short)double.Parse(v);
                    case Type t when t == typeof(byte): return (byte)double.Parse(v);
                    case Type t when t == typeof(long): return (long)double.Parse(v);
                    case Type t when t == typeof(DateTime): return DateTime.Parse(v);
                    case Type t when t == typeof(bool): return bool.Parse(v);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Parse value error type is {_type}: content:{v}" + v, e);
            }
            return null;
        }

        public bool IsInternalType => TypeEnum.IsInternalType();
        public bool IsArray => false;
        public bool IsDictionary => false;
        public bool IsMatch(string typeName)
        {
            return _type.Name.Equals(typeName);
        }
        public string FullTypeName => _type.FullName;

        public object DefaultValue
        {
            get
            {
                switch (_type)
                {
                    case Type t when t == typeof(string): return "";
                    case Type t when t == typeof(double): return 0.0;
                    case Type t when t == typeof(float): return 0.0f;
                    case Type t when t == typeof(int): return 0;
                    case Type t when t == typeof(short): return (short)0;
                    case Type t when t == typeof(byte): return (byte)0;
                    case Type t when t == typeof(long): return 0;
                    case Type t when t == typeof(DateTime): return DateTime.Now;
                    case Type t when t == typeof(bool): return false;
                }
                return null;
            }
        }
    }
}