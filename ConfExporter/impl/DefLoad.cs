using System;
using System.Collections.Generic;
using core;
using NPOI.SS.UserModel;

namespace impl
{
    public class TableData
    {
        public List<List<object>> Data { get; set; }
    }
    public class DefLoader : IDataLoader<TableData, DefMetaData>
    {
        public TableData Load(ISheet sheet, DefMetaData meta)
        {
            return new TableData() { Data = ParseData(sheet, meta.Fileds, meta.Key) };
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

        public static readonly int FirstDataRow = 4;
    }
}