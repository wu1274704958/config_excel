using System;
using System.Collections.Generic;
using core;
using core.filed_type;
using impl.tags;
using NPOI.SS.UserModel;

namespace impl
{
    public struct FiledMetaData
    {
        public string Name { get; set; }
        public EFiledType TypeEnum { get; set; }
        public IFiledType Type { get; set; }
        // only for custom type and plugin type
        public string TypeClassName { get; set; }
        public bool IsNullable { get; set; }
        public Dictionary<string,object> Tags { get; set; }
        
        public bool Invalid => Type == null || TypeEnum == EFiledType.None || TypeClassName == null || Name == null;
    }
    public class DefMetaData
    {
        public string Package { get; set; }
        public string MgrClassName { get; set; }
        public string FullClassName => $"{Package}.{ClassName}";
        public string FullMgrClassName => $"{Package}.{MgrClassName}";
        public string ClassName { get; set; }
        public List<FiledMetaData> Fileds { get; set; }
        public FiledMetaData Key { get; set; }
        public int KeyIndex { get; set; }
        //public List<List<object>> Data { get; set; }
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
            new Reference(),
            new Others()
        };
        public static readonly List<ITagParser> TagParsers = new List<ITagParser>()
        {
            new Key(),
            new Sealed(),
            new Others()
        };
        public static readonly List<IFiledType> FieldTypes = new List<IFiledType>()
        {
            new BaseInternalFiledType<string>(),
            new BaseInternalFiledType<double>(),
            new BaseInternalFiledType<float>(),
            new BaseInternalFiledType<DateTime>(),
            new BaseInternalFiledType<bool>(),
            new BaseInternalFiledType<int>(),
            new BaseInternalFiledType<long>(),
            new BaseInternalFiledType<short>(),
            new BaseInternalFiledType<byte>(),
            
            new InternalArrayType<string>(),
            new InternalArrayType<double>(),
            new InternalArrayType<float>(),
            new InternalArrayType<DateTime>(),
            new InternalArrayType<bool>(),
            new InternalArrayType<int>(),
            new InternalArrayType<long>(),
            new InternalArrayType<short>(),
            new InternalArrayType<byte>(),
            
            new InternalDictType<int,string>(),
            new InternalDictType<int,int>(),
            new InternalDictType<int,long>(),
            new InternalDictType<int,float>(),
            new InternalDictType<int,double>(),
            new InternalDictType<int,bool>(),
            new InternalDictType<int,DateTime>(),
            
            new InternalDictType<string,string>(),
            new InternalDictType<string,int>(),
            new InternalDictType<string,long>(),
            new InternalDictType<string,float>(),
            new InternalDictType<string,double>(),
            new InternalDictType<string,bool>(),
            new InternalDictType<string,DateTime>(),
        };
        public DefMetaData GenerateMeta(ISheet sheet,string fileName)
        {
            if (sheet == null) return null;
            int rowCount = sheet.LastRowNum + 1;
            var nameRow = sheet.GetRow(NameRow);
            if(nameRow == null) throw new Exception("GenerateMeta Error :no filed name row");
            int colCount = nameRow.LastCellNum + 1;
            DefMetaData defMetaData = new DefMetaData();
            defMetaData.Package = $"{Package}.{fileName}"; 
            defMetaData.MgrClassName = $"{sheet.SheetName}Mgr";
            defMetaData.ClassName = $"{sheet.SheetName}";
            defMetaData.Fileds = ParseFileds(sheet, colCount);
            var tagsCell = sheet.GetRow(TagRow)?.GetCell(0);
            defMetaData.Tags = tagsCell == null ? new Dictionary<string, object>() : ParseTags(tagsCell,null,TagParsers);
            (defMetaData.Key,defMetaData.KeyIndex) = GetKey(defMetaData.Tags.TryGetValue(nameof(Key),out var key) ? key : null,defMetaData.Fileds);
            if(defMetaData.Key.Invalid)
                throw new Exception("GenerateMeta Error :no key filed");
            //defMetaData.Data = ParseData(sheet, defMetaData.Fileds, defMetaData.Key);
            return defMetaData;
        }

        private List<List<object>> ParseData(ISheet sheet, List<FiledMetaData> fileds, FiledMetaData key)
        {
            List<List<object>> datas = new List<List<object>>();
            for (int r = FirstDataRow; r < sheet.LastRowNum + 1; r++)
            {
                var row = new List<object>();
                for (int c = 0;c < fileds.Count; c++)
                {
                    var f = fileds[c];
                    object value = null;
                    var cell = sheet.GetRow(r)?.GetCell(c);
                    if(cell == null && !f.IsNullable)
                        throw new Exception($"ParseData Error :no cell r={r},c={c}");
                    value = cell == null ? f.Type.DefaultValue : f.Type.ParseValue(cell);
                    row.Add(value);
                }
                datas.Add(row);
            }
            return datas;
        }

        private (FiledMetaData,int) GetKey(object value, List<FiledMetaData> fileds, string id = "Id")
        {
            if(!string.IsNullOrEmpty(value as string))
                id = (string)value;
            for (int i = 0; i < fileds.Count; i++)
            {
                var f = fileds[i];
                if (f.Name == id)
                    return (f,i);
            }
            return (default(FiledMetaData),-1);
        }

        private List<FiledMetaData> ParseFileds(ISheet sheet, int colCount)
        {
            List<FiledMetaData> fileds = new List<FiledMetaData>();
            for (int i = 0; i < colCount; i++)
            {
                FiledMetaData filedMetaData = new FiledMetaData();
                ICell cell = sheet.GetRow(NameRow)?.GetCell(i);
                if(cell == null || cell.CellType == CellType.Blank) 
                    return fileds;
                filedMetaData.Name = cell.StringCellValue;
                cell = sheet.GetRow(TypeRow)?.GetCell(i);
                var firstData= sheet.GetRow(FirstDataRow)?.GetCell(i);
                filedMetaData.TypeEnum = ParseFiledType(cell,firstData,out string typeClassName,out var filedType);
                filedMetaData.Type = filedType;
                if (filedMetaData.TypeEnum == EFiledType.None || filedMetaData.Type == null)
                    throw new Exception("GenerateMeta Error :parse filed type failed");
                filedMetaData.TypeClassName = typeClassName;
                cell = sheet.GetRow(FieldTagsRow)?.GetCell(i);
                filedMetaData.Tags = ParseTags(cell,filedMetaData.Type,FieldTagParsers);
                filedMetaData.IsNullable = true;
                if(filedMetaData.Tags.TryGetValue(nameof(NotNull), out var tag))
                   filedMetaData.IsNullable = !(bool)tag;
                fileds.Add(filedMetaData);
            }
            return fileds;
        }

        private void PreParseTags(ICell cell, Action<string, string> f)
        {
            string str = null;
            if (cell == null || cell.CellType != CellType.String || (str = cell.StringCellValue) == null) return;
            var arr = str.Split(';');
            foreach (var item in arr)
            {
                if(item.Length == 0) continue;
                var kv = item.Split(':');
                f(kv[0],kv.Length == 1 ? null : kv[1]);
            }
        }

        private Dictionary<string,object> ParseTags(ICell cell, IFiledType filedType,List<ITagParser> tagParsers)
        {
            var res = new Dictionary<string,object>();
            PreParseTags(cell, (k, v) =>
            {
                if (res.ContainsKey(k)) throw new Exception("Duplicate tag: " + k);
                foreach (var parser in tagParsers)
                {
                    if (parser.MatchKey(k))
                    {
                        object val = null;
                        if (v == null)
                        {
                            val = parser.GetDefaultValue(filedType);
                            if (val == null)
                                throw new Exception("No default value for tag: " + k);
                        }
                        else
                            val = parser.ParseValue(v);

                        if (val == null)
                            throw new Exception("Parse value error for tag: " + k);
                        res.Add(k, val);
                        break;
                    }
                }
            });
            return res;
        }
        
        private IFiledType MatchOtherType(string specifiedTy)
        {
            foreach (var ty in FieldTypes)
            {
                if (ty.IsMatch(specifiedTy))
                {
                    return ty;
                }
            }
            return null;
        }
        
        private EFiledType ParseFiledType(ICell cell, ICell firstData,out string typeClassName,out IFiledType type)
        {
            string specifiedTy = null;
            if (cell != null && cell.CellType == CellType.String && cell.StringCellValue?.Length > 0)
                specifiedTy = cell.StringCellValue;
            foreach (var ty in FieldTypes)
            {
                var f = specifiedTy == null ? ty.TryDeduceType(firstData) : ty.IsMatch(specifiedTy,MatchOtherType);
                if (f)
                {
                    type = ty;
                    typeClassName = ty.FullTypeName;
                    return ty.TypeEnum;
                }
            }
            type = null;
            typeClassName = null;
            return EFiledType.None;
        }
    }
}