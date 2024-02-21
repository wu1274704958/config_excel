using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using core;
using NPOI.SS.UserModel;

namespace impl.tags
{
    public class CustomEnum : BaseTagParser<CustomEnum>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            throw new Exception("CustomEnum can not get default value");
        }

        public override Type ValueType => typeof(Dictionary<string,List<(string,string)>>);
        protected override object ParseValue_inner(string v)
        {
            var dict = new Dictionary<string,List<(string,string)>>();
            try
            {
                var ss = v.Split(',');
                foreach (var s in ss)
                {
                    var sheet = Context.Instance.CurrentBook.GetSheet(s);
                    if(sheet == null)
                        throw new Exception("CustomEnum Can not find sheet: " + s);
                    ParseEnumMetaData(sheet,ref dict);
                }
            }
            catch (Exception e)
            {
                throw new Exception("CustomEnum Parse value error: " + v, e);
            }
            return dict;
        }

        private void ParseEnumMetaData(ISheet sheet, ref Dictionary<string,List<(string, string)>> dict)
        {
            var name = sheet.SheetName.Replace('_','E');
            var val = new List<(string, string)>();
            for (int i = 0; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                ICell cell = null;
                if(row == null || (cell = row.GetCell(0)) == null || cell.CellType != CellType.String)
                    break;
                var vv = row.GetCell(1);
                string v = null;
                if(vv != null && vv.CellType == CellType.String)
                    v = vv.StringCellValue;
                if(vv != null && (vv.CellType == CellType.Numeric || vv.CellType == CellType.Formula))
                    v = ((int)Math.Truncate(vv.NumericCellValue)).ToString();
                val.Add((cell.StringCellValue,v));
            }
            dict.Add(name,val);
        }
    }
}