using core;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfExporter.core.filed_type
{
    public class PairType<K, V> : IFiledType
    {
        public IFiledType KType;
        public IFiledType VType;

        public EFiledType TypeEnum => EFiledType.CustomType;

        public bool IsInternalType => true;

        public bool IsArray => false;

        public bool IsDictionary => false;

        public string FullTypeName => $"System.Collections.Generic.KeyValuePair<{KType.FullTypeName},{VType.FullTypeName}>";

        public object DefaultValue => default;

        public Type Type => typeof(KeyValuePair<K, V>);

        public string[] PluginFiles => null;

        public bool NeedInit => false;

        public string GenInitCode(string objName, bool semicolon)
        {
            throw new NotImplementedException();
        }

        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            if (typeName.StartsWith("Pair<") && typeName[typeName.Length - 1] == '>')
            {
                var inner = typeName.Substring(5, typeName.Length - 6);
                var index = inner.IndexOf(',');
                if (index < 0)
                    return false;
                var keyTypeName = inner.Substring(0, index);
                var valTypeName = inner.Substring(index + 1);
                var keyType = matchOther?.Invoke(keyTypeName);
                var valType = matchOther?.Invoke(valTypeName);
                if (keyType != null && valType != null && keyType.Type == typeof(K) && valType.Type == typeof(V))
                {
                    KType = keyType;
                    VType = valType;
                    return true;
                }
            }
            return false;
        }

        public object ParseValue(ICell v)
        {
            if (v.CellType == CellType.String && v.StringCellValue != null && v.StringCellValue.Length > 0)
                return ParseValue(v.StringCellValue);
            return null;
        }

        public object ParseValue(string v)
        {
            var arr = v.Split(',');
            if(arr.Length != 2)
                return null;
            var key = KType.ParseValue(arr[0]);
            var val = KType.ParseValue(arr[1]);
            return new KeyValuePair<K, V>((K)key, (V)val);
        }

        public bool TryDeduceType(ICell cell)
        {
            return false;
        }
    }
}
