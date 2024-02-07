using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace core.filed_type
{
    public class InternalArrayType<T> : IFiledType
    {
        public IFiledType InsideType { get; protected set; } = null;
        public EFiledType TypeEnum => EFiledType.Array;
        public bool TryDeduceType(ICell cell)
        {
            return false;
        }

        public object ParseValue(ICell v)
        {
            if(InsideType == null)
                throw new Exception($"Array inside type is null {typeof(T).FullName} row:{v.RowIndex} col:{v.ColumnIndex}");
            if(v.CellType == CellType.String && !string.IsNullOrEmpty(v.StringCellValue))
                return ParseValue(v.StringCellValue);
            throw new Exception($"Array type must be string row:{v.RowIndex} col:{v.ColumnIndex} InsideType:{InsideType.FullTypeName}");
            return null;
        }

        public object ParseValue(string v)
        {
            List<T> res = new List<T>();
            if (InsideType != null)
            {
                var ss = v.Split(';');
                var len = ss[ss.Length - 1].Length == 0 ? ss.Length - 1 : ss.Length;
                for (int i = 0; i < len; i++)
                {
                    //res[i] = (T)InsideType.ParseValue(ss[i]);
                    res.Add((T)InsideType.ParseValue(ss[i]));
                }
            }
            return res;
        }

        public bool IsInternalType => true;
        public bool IsArray => true;
        public bool IsDictionary => false;
        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            if (typeName.EndsWith("[]"))
            {
                var pureTyName = typeName.Substring(0, typeName.Length - 2);
                var pureType = matchOther?.Invoke(pureTyName);
                if (pureType != null && pureType.Type == typeof(T))
                {
                    InsideType = pureType;
                    return true;
                }
            }
            return false;
        }
        public string FullTypeName => $"System.Collections.Generic.List<{typeof(T).FullName}>";
        public object DefaultValue => null;
        public Type Type => typeof(List<T>);
    }
}