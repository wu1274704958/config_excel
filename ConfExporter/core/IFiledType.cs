using System;
using System.Globalization;
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
        Array,
        Dictionary
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
        object ParseValue(ICell v);
        object ParseValue(string v);
        bool IsInternalType { get; }
        bool IsArray { get; }
        bool IsDictionary { get; }
        bool IsMatch(string typeName,Func<string,IFiledType> matchOther=null);
        string FullTypeName { get; }
        object DefaultValue { get; }
        Type Type { get; }
    }

    public class BaseInternalFiledType<T> : IFiledType
    {
        private readonly Type _type = typeof(T);
        public Type Type => _type;
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

        public object ParseValue(ICell v)
        {
            try
            {
                if (v.CellType == CellType.String)
                    return ParseValue(v.StringCellValue);
                switch (_type)
                {
                    case Type t when t == typeof(string):
                        switch (v.CellType)
                        {
                            case CellType.Numeric:
                                return Convert.ToString(v.NumericCellValue, CultureInfo.InvariantCulture);
                            case CellType.String:
                                return v.StringCellValue;
                            case CellType.Boolean:
                                return v.BooleanCellValue.ToString();
                            default:
                                throw new Exception($"Parse value error type is {_type}: content:{v}");
                        }
                    case Type t when t == typeof(double):
                        return v.CellType == CellType.Numeric ? v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(float):
                        return v.CellType == CellType.Numeric ? (float)v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(int):
                        return v.CellType == CellType.Numeric ? (int)v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(short):
                        return v.CellType == CellType.Numeric ? (short)v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(byte): 
                        return v.CellType == CellType.Numeric ? (byte)v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(long): 
                        return v.CellType == CellType.Numeric ? (long)v.NumericCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(DateTime): 
                        return v.CellType == CellType.Numeric && v.CellStyle.DataFormat == 14 ? v.DateCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                    case Type t when t == typeof(bool): 
                        return v.CellType == CellType.Boolean ? v.BooleanCellValue : throw new Exception($"Parse value error type is {_type}: content:{v}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Parse value error type is {_type}: content:{v}", e);
            }
            return null;
        }

        public object ParseValue(string v)
        {
            try
            {
                switch (_type)
                {
                    case Type t when t == typeof(string):
                        return v;
                    case Type t when t == typeof(double):
                        return double.Parse(v);
                    case Type t when t == typeof(float):
                        return float.Parse(v);
                    case Type t when t == typeof(int):
                        return int.Parse(v);
                    case Type t when t == typeof(short):
                        return short.Parse(v);
                    case Type t when t == typeof(byte): 
                        return byte.Parse(v);
                    case Type t when t == typeof(long): 
                        return long.Parse(v);
                    case Type t when t == typeof(DateTime): 
                        return Convert.ToDateTime(v);
                    case Type t when t == typeof(bool): 
                        return bool.Parse(v);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Parse value error type is {_type}: content:{v}", e);
            }
            return null;
        }

        public bool IsInternalType => TypeEnum.IsInternalType();
        public bool IsArray => false;
        public bool IsDictionary => false;
        public bool IsMatch(string typeName,Func<string,IFiledType> matchOther = null)
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