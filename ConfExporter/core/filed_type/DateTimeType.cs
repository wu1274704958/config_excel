using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.filed_type
{
    public class DateTimeType : IFiledType
    {
        public EFiledType TypeEnum => EFiledType.CustomType;

        public bool IsInternalType => true;

        public bool IsArray => false;
        public bool IsDictionary => false;

        public string FullTypeName => typeof(DateTime).FullName;

        public object DefaultValue => DateTime.MinValue;

        public Type Type => typeof(DateTime);

        public string[] PluginFiles => null;

        public bool NeedInit => false;

        public string GenInitCode(string objName, bool semicolon)
        {
            throw new NotImplementedException();
        }

        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            return typeof(DateTime).Name == typeName;
        }

        public object ParseValue(ICell v)
        {
            if(v != null && v.CellType == CellType.String && v.StringCellValue != null)
            {
                return ParseValue(v.StringCellValue);
            }
            return DefaultValue;
        }

        public object ParseValue(string v)
        {
            if(v.Length == 0)
                return DefaultValue;
            try
            {
                if (v.IndexOf(';') >= 0)
                {
                    var arr = v.Split(';');
                    return DateTime.ParseExact(arr[0], arr[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    return DateTime.Parse(v);
                }
            }catch(Exception e)
            {
                Console.WriteLine($"DateTime parse failed!!! v = {v} {e}");
                return DefaultValue;
            }
        }

        public bool TryDeduceType(ICell cell)
        {
            return false;
        }
    }
}
