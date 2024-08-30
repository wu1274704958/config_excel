using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.filed_type
{
    public class AnyArrayType : IFiledType
    {
        public EFiledType TypeEnum => EFiledType.CustomType;

        public bool IsInternalType => false;

        public bool IsArray => false;

        public bool IsDictionary => false;

        public string FullTypeName => typeof(AnyArray).FullName;

        public object DefaultValue => null;

        public Type Type => typeof(AnyArray);

        public string[] PluginFiles => new string[] { "AnyArray.cs" };

        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            return typeName == typeof(AnyArray).Name;
        }

        public object ParseValue(ICell v)
        {
            if(v.CellType == CellType.String)
            {
                return ParseValue(v.StringCellValue);
            }
            return null;
        }

        public object ParseValue(string v)
        {
            if (v == null || v.Length == 0)
                return null;
            try
            {
                var arr = new AnyArray(v);
                return arr;
            }catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool TryDeduceType(ICell cell)
        {
            return false;
        }
    }
}
