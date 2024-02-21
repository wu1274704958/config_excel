using System;
using System.Collections.Concurrent;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace core
{
    public class IExporter<GM,GC,LD,S,MD,C,D>
    where GM : IGenMeta<MD>,new()
    where GC : IGenCode<MD,C>,new()
    where LD : IDataLoader<D,MD>,new()
    where S : ISerializer<MD,D>,new()
    {
        public virtual int Export(FileInfo[] @in, DirectoryInfo outCodeDir, DirectoryInfo outDataDir, bool onlyGenData)
        {
            int ret = 0;
            foreach (var f in @in)
            {
                var fileName = f.Name;
                var extIdx = fileName.IndexOf('.');
                if(extIdx > 0)
                    fileName = fileName.Substring(0,extIdx);
                using (FileStream fs = f.Open( FileMode.Open, FileAccess.Read))
                {
                    IWorkbook book = new XSSFWorkbook(fs);
                    Context.Instance.CurrentBook = book;
                    foreach (var sheet in book)
                    {
                        if(sheet.SheetName[0] == '_')
                            continue;
                        ret += HandleSheet(sheet, outCodeDir, outDataDir, onlyGenData,fileName);
                    }
                }
            }
            return ret;
        }
        
        protected virtual int HandleSheet(ISheet sheet, DirectoryInfo outCodeDir, DirectoryInfo outDataDir, bool onlyGenData,string fileName)
        {
            try
            {
                Context.Instance.CurrentSheet = sheet;
                var d = new GM().GenerateMeta(sheet,fileName);
                var dict = new ConcurrentDictionary<string, string>();
                var code = new GC().GenerateCode(d, ref dict);
                if (!onlyGenData)
                {
                    foreach (var f in dict)
                    {
                        WriteFile(f.Key, f.Value, ".cs", outCodeDir.FullName);
                    }
                }
                var data = new LD().Load(sheet, d);
                new S().Serialize(d, data, outDataDir, dict);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
            return 1;
        }
        
        protected static void WriteFile(string name, string cnt, string suffix, string dir)
        {
            var delimiter = name.IndexOf('/');
            if (delimiter > 0)
            {
                dir = Path.Combine(dir, name.Substring(0, delimiter));
                name = name.Substring(delimiter + 1);
            }
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, name + suffix);
            File.WriteAllText(path, cnt);
        }
    }
}