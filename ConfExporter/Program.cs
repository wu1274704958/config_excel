

using System;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ConfExporter
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (FileStream fs = new FileStream(args[0],FileMode.Open,FileAccess.Read))
            {
                IWorkbook book = new XSSFWorkbook(fs);
                foreach (var sheet in book)
                {
                    Console.WriteLine($"{sheet.SheetName}");
                    PrintSheet(sheet);   
                }
            }
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