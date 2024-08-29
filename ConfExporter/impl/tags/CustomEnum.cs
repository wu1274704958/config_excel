using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using core;
using NPOI.SS.UserModel;

namespace impl.tags
{
    public class Flags : BaseTagParser<Flags>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return true;
        }
        public override Type ValueType => typeof(bool);
    }
    public class CustomEnum : BaseTagParser<CustomEnum>
    {
        public class CustomEnumMetaData
        {
            public Dictionary<string, (List<(string, string)>, Dictionary<string, object>)> Data = new Dictionary<string, (List<(string, string)>, Dictionary<string, object>)>();
        }
        public override object GetDefaultValue(IFiledType type)
        {
            throw new Exception("CustomEnum can not get default value");
        }

        public override Type ValueType => typeof(CustomEnumMetaData);
        public static readonly List<ITagParser> TagParsers = new List<ITagParser>() { new Flags() };
        public Dictionary<string,object> Tags { get; set; }
        protected override object ParseValue_inner(string v)
        {
            var dict = new CustomEnumMetaData();
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

        private void ParseEnumMetaData(ISheet sheet, ref CustomEnumMetaData res)
        {
            Tags = null;
            var name = sheet.SheetName.Replace('_','E');
            var val = new List<(string, string)>();
            for (int i = 0; i <= sheet.LastRowNum; i++)
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
            IRow firstRow = null;
            ICell tagCell = null;
            if (sheet.LastRowNum >= 0 && (tagCell = (firstRow = sheet.GetRow(0)).GetCell(2)) != null)
            {
                Tags = DefGenMeta.ParseTags(tagCell, null, TagParsers);
            }
            res.Data.Add(name,(val,Tags));
        }
    }
}