using System;
using System.Collections.Generic;
using core;
using impl.tags;
using NPOI.SS.UserModel;

namespace impl
{
    public struct FiledMetaData
    {
        public string Name { get; set; }
        public EFiledType Type { get; set; }
        // only for custom type and plugin type
        public string TypeClassName { get; set; }
        public bool IsNullable { get; set; }
        public Dictionary<string,object> Tags { get; set; }
    }
    public class DefMetaData
    {
        public string Package { get; set; }
        public string MgrClassName { get; set; }
        public string ClassName { get; set; }
        public List<FiledMetaData> Fileds { get; set; }
        public Dictionary<long,List<object>> Data { get; set; }
        public Dictionary<string,object> Tags { get; set; }
    }
    
    public class DefGenMeta : IGenMeta<DefMetaData>
    {
        public static readonly int NameRow = 0;
        public static readonly int TypeRow = 1;
        public static readonly int FieldTagsRow = 2;
        public static readonly int TagRow = 3;
        public static readonly int FirstDataRow = 4;
        public static readonly string Package = "conf";

        public static readonly List<ITagParser> FieldTagParsers = new List<ITagParser>()
        {
            new NotNull(),
            new Others()
        };
        public DefMetaData GenerateMeta(ISheet sheet)
        {
            if (sheet == null) return null;
            int rowCount = sheet.LastRowNum + 1;
            var nameRow = sheet.GetRow(NameRow);
            if(nameRow == null) throw new Exception("GenerateMeta Error :no filed name row");
            int colCount = nameRow.LastCellNum + 1;
            DefMetaData defMetaData = new DefMetaData();
            defMetaData.Package = Package; 
            defMetaData.MgrClassName = $"{Package}.{sheet.SheetName}Mgr";
            defMetaData.ClassName = $"{Package}.{sheet.SheetName}";
            defMetaData.Fileds = ParseFileds(sheet, colCount);
            
            return null;
        }

        private List<FiledMetaData> ParseFileds(ISheet sheet, int colCount)
        {
            List<FiledMetaData> fileds = new List<FiledMetaData>();
            for (int i = 0; i < colCount; i++)
            {
                FiledMetaData filedMetaData = new FiledMetaData();
                ICell cell = sheet.GetRow(NameRow)?.GetCell(i);
                if(cell == null) 
                    throw new Exception("GenerateMeta Error :no filed name cell");
                filedMetaData.Name = cell.StringCellValue;
                cell = sheet.GetRow(TypeRow)?.GetCell(i);
                var firstData= sheet.GetRow(FirstDataRow)?.GetCell(i);
                if(cell == null) 
                    throw new Exception("GenerateMeta Error :no filed type cell");
                filedMetaData.Type = ParseFiledType(cell,firstData,out string typeClassName);
                filedMetaData.TypeClassName = typeClassName;
                cell = sheet.GetRow(FieldTagsRow)?.GetCell(i);
                
                if(cell == null)
                    throw new Exception("GenerateMeta Error :no filed tags cell");
                filedMetaData.Tags = ParseTags(cell,firstData);
                filedMetaData.IsNullable = true;
                if(filedMetaData.Tags.TryGetValue("NotNull", out var tag))
                   filedMetaData.IsNullable = !(bool)tag;
            }

            return fileds;
        }

        private Dictionary<string,object> ParseTags(ICell cell, ICell firstData)
        {
            var res = new Dictionary<string,object>();
            var str = cell.StringCellValue;
            if (str == null) return res;
            var arr = str.Split(';');
            foreach (var item in arr)
            {
                if(item.Length == 0) continue;
                var kv = item.Split(':');
                if(res.ContainsKey(kv[0])) throw new Exception("Duplicate tag: " + kv[0]);
                foreach (var parser in FieldTagParsers)
                {
                    if (parser.MatchKey(kv[0]))
                    {
                        object val = null;
                        if (kv.Length == 1)
                        {
                            val = parser.DefaultValue;
                            if(val == null)
                                throw new Exception("No default value for tag: " + kv[0]);
                        }
                        else
                            val = parser.ParseValue(kv[1]);
                        if(val == null)
                            throw new Exception("Parse value error for tag: " + kv[0]);
                        res.Add(kv[0],val);
                        break;
                    }
                }
            }
            return res;
        }

        private EFiledType ParseFiledType(ICell cell, ICell firstData,out string typeClassName)
        {
            
        }
    }
}