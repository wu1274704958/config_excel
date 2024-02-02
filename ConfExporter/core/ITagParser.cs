using System;

namespace core
{

    public interface ITagParser
    {
        bool MatchKey(string key);
        object DefaultValue { get; }
        object ParseValue(string v);
    }
    public abstract class BaseTagParser<T> : ITagParser
    {
        public virtual bool MatchKey(string key)
        {
            return key.Equals(typeof(T).Name);
        }
        public abstract Type ValueType { get; }
        public abstract object DefaultValue { get; }
        
        public virtual object ParseValue(string v)
        {
            switch (ValueType)
            {
                case Type t when t == typeof(string):
                    return v;
                case Type t when t == typeof(bool):
                    return bool.Parse(v);
                case Type t when t == typeof(int):
                    return int.Parse(v);
                case Type t when t == typeof(long):
                    return long.Parse(v);
                case Type t when t == typeof(float):
                    return float.Parse(v);
                case Type t when t == typeof(double):
                    return double.Parse(v);
                case Type t when t == typeof(short):
                    return short.Parse(v);
                case Type t when t == typeof(byte):
                    return byte.Parse(v);
                default:
                    return ParseValue_inner(v);
            }
        }
        protected virtual object ParseValue_inner(string v)
        {
            throw new Exception("Not support type: " + ValueType + " value: " + v);
        }
    }
}