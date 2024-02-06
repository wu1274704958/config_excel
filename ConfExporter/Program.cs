

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using conf;
using impl;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ConfExporter
{
    internal class Program
    {
        class Options
        {
            public string CodeOutDir { get; set; }
            public string DataOutDir { get; set; }
        }
        public static void Main(string[] args)
        {
            
            conf.Sheet1Mgr.InitInstance(new FileInfo("D:\\code\\ConfExporter\\ConfExporter\\bin\\Debug\\data\\Sheet1.dat"));
            var d = conf.Sheet1Mgr.GetInstance().Get(1);
            Console.WriteLine("{0} {1} {2} {3}",d.Id,d.Name,d.Age,d.Birthday);
            
        }

        private static void LoadTable(string[] args)
        {
            DirectoryInfo dataOutDir = new DirectoryInfo("data");
            DirectoryInfo codeOutDir = new DirectoryInfo("conf");
            if(dataOutDir.Exists == false) dataOutDir.Create();
            if(codeOutDir.Exists == false) codeOutDir.Create();
            using (FileStream fs = new FileStream(args[0],FileMode.Open,FileAccess.Read))
            {
                IWorkbook book = new XSSFWorkbook(fs);
                foreach (var sheet in book)
                {
                    var d = new DefGenMeta().GenerateMeta(sheet);
                    var dict =  new ConcurrentDictionary<string, string>();
                    var code = new DefGenCode().GenerateCode(d, ref dict);
                    foreach (var f in dict)
                    {
                        WriteFile(f.Key, f.Value, ".cs", codeOutDir.FullName);
                    }
                    var data = new DefLoader().Load(sheet, d);
                    new DefSerializer().Serialize(d,data,dataOutDir,dict);
                    Console.WriteLine("223");
                }
            }
        }

        private static void WriteFile(string name, string cnt, string suffix, string dir)
        {
            var path = Path.Combine(dir, name + suffix);
            File.WriteAllText(path, cnt);
        }

        private static void PrintSheet(ISheet sheet)
        {
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                for (int j = row.FirstCellNum; j < row.LastCellNum; j++)
                {
                    ICell cell = row.GetCell(j);
                    if (cell == null) continue;
                    switch (cell.CellType)
                    {
                        case CellType.Unknown:
                            Console.WriteLine("unknown");
                            break;
                        case CellType.Numeric:
                            if(cell.CellStyle.DataFormat == 14)
                                Console.WriteLine(cell.DateCellValue);
                            else
                                Console.WriteLine(cell.NumericCellValue);
                            break;
                        case CellType.String:
                            Console.WriteLine(cell.StringCellValue);
                            break;
                        case CellType.Formula:
                            Console.WriteLine(cell.NumericCellValue);
                            break;
                        case CellType.Blank:
                            Console.WriteLine("nil");
                            break;
                        case CellType.Boolean:
                            Console.WriteLine(cell.BooleanCellValue);
                            break;
                        case CellType.Error:
                            Console.WriteLine(cell.ErrorCellValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        
    }
}