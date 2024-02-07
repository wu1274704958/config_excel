using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;

namespace core.filed_type
{
    public class InternalDictType<K,V> : IFiledType
    {
        public IFiledType InsideKeyType { get; protected set; } = null;
        public IFiledType InsideValType { get; protected set; } = null;
        public EFiledType TypeEnum => EFiledType.Dictionary;
        public bool TryDeduceType(ICell cell)
        {
            return false;
        }

        public object ParseValue(ICell v)
        {
            if(InsideKeyType == null )
                throw new Exception($"Dictionary inside Key type is null {typeof(K).FullName} row:{v.RowIndex} col:{v.ColumnIndex}");
            if(InsideValType == null)
                throw new Exception($"Dictionary inside Val type is null {typeof(V).FullName} row:{v.RowIndex} col:{v.ColumnIndex}");
            if(v.CellType == CellType.String && !string.IsNullOrEmpty(v.StringCellValue))
                return ParseValue(v.StringCellValue);
            throw new Exception($"Dictionary type must be string row:{v.RowIndex} col:{v.ColumnIndex} KeyType:{InsideKeyType.FullTypeName} ValType:{InsideValType.FullTypeName}");
            return null;
        }

        public object ParseValue(string v)
        {
            Dictionary<K, V> res = new Dictionary<K, V>();
            if (InsideKeyType != null && InsideValType != null)
            {
                var ss = v.Split(';');
                var len = ss[ss.Length - 1].Length == 0 ? ss.Length - 1 : ss.Length;
                for (int i = 0; i < len; i++)
                {
                    var mid = ss[i].IndexOf(':');
                    if(mid < 0) throw new Exception($"Parse dictionary value error: {ss[i]} KeyType:{InsideKeyType.FullTypeName} ValType:{InsideValType.FullTypeName}");
                    var key = InsideKeyType.ParseValue(ss[i].Substring(0, mid));
                    if(key == null)
                        throw new Exception($"Parse dictionary key error: {ss[i]} KeyType:{InsideKeyType.FullTypeName} ValType:{InsideValType.FullTypeName}");
                    res.Add((K)key, (V)InsideValType.ParseValue(ss[i].Substring(mid + 1)));
                }
            }
            return res;
        }

        public bool IsInternalType => true;
        public bool IsArray => true;
        public bool IsDictionary => false;
        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            var match = Reg.Match(typeName);
            if (match.Success && match.Groups.Count == 3)
            {
                var keyTypeName = match.Groups[1].Value;
                var valTypeName = match.Groups[2].Value;
                var keyType = matchOther?.Invoke(keyTypeName);
                var valType = matchOther?.Invoke(valTypeName);
                if (keyType != null && valType != null &&  keyType.Type == typeof(K) && valType.Type == typeof(V))
                {
                    InsideKeyType = keyType;
                    InsideValType = valType;
                    return true;
                }
            }
            return false;
        }

        public static readonly Regex Reg = new Regex("Dict<(.+),(.+)>");

        public string FullTypeName => $"System.Collections.Generic.Dictionary<{typeof(K).FullName},{typeof(V).FullName}>";
        public object DefaultValue => null;
        public Type Type => typeof(Dictionary<K,V>);
    }
}