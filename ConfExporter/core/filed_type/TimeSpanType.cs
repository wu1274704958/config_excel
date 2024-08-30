using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.filed_type
{
    public class TimeSpanType : IFiledType
    {
        public EFiledType TypeEnum => EFiledType.CustomType;

        public bool IsInternalType => true;

        public bool IsArray => false;
        public bool IsDictionary => false;

        public string FullTypeName => typeof(TimeSpan).FullName;

        public object DefaultValue => TimeSpan.Zero;

        public Type Type => typeof(TimeSpan);

        public string[] PluginFiles => null;

        public bool NeedInit => false;

        public string GenInitCode(string objName, bool semicolon)
        {
            throw new NotImplementedException();
        }

        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            return typeof(TimeSpan).Name == typeName;
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
                    return TimeSpan.ParseExact(arr[0], arr[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    return TimeSpan.Parse(v);
                }
            }catch(Exception e)
            {
                return SelfParse(v);
            }
        }

        private object SelfParse(string input)
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            if (input.EndsWith("m"))
            {
                int minutes = int.Parse(input.TrimEnd('m'));
                timeSpan = TimeSpan.FromMinutes(minutes);
            }
            else if (input.EndsWith("h"))
            {
                int hours = int.Parse(input.TrimEnd('h'));
                timeSpan = TimeSpan.FromHours(hours);
            }
            else if (input.EndsWith("ms"))
            {
                int milliseconds = int.Parse(input.Substring(0,input.Length - 2));
                timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            }
            else if (input.EndsWith("s"))
            {
                int seconds = int.Parse(input.TrimEnd('s'));
                timeSpan = TimeSpan.FromSeconds(seconds);
            }
            else if (input.EndsWith("d"))
            {
                int days = int.Parse(input.TrimEnd('d'));
                timeSpan = TimeSpan.FromDays(days);
            }
            return timeSpan;
        }

        public bool TryDeduceType(ICell cell)
        {
            return false;
        }
    }
}
