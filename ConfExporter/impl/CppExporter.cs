using System;
using System.Collections.Concurrent;
using System.IO;
using core;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace impl
{
    /// <summary>
    /// C++ backend: emits protobuf schemas and protobuf-net-compatible binary
    /// data. The engine's vendored protoc creates C++ message classes at build time.
    /// </summary>
    public sealed class CppExporter
    {
        public int Export(FileInfo[] inputFiles, DirectoryInfo codeOutputDirectory, DirectoryInfo dataOutputDirectory, ExportOption exportOption)
        {
            if (!codeOutputDirectory.Exists)
                codeOutputDirectory.Create();
            if (!dataOutputDirectory.Exists)
                dataOutputDirectory.Create();

            var exported = 0;
            foreach (var inputFile in inputFiles)
            {
                try
                {
                    using (var stream = inputFile.Open(FileMode.Open, FileAccess.Read))
                    {
                        IWorkbook book = new XSSFWorkbook(stream);
                        Context.Instance.CurrentBook = book;

                        foreach (var sheet in book)
                        {
                            if (sheet.SheetName.Length == 0 || sheet.SheetName[0] == '_')
                                continue;
                            if (exportOption.ChooseSheet != null && !exportOption.ChooseSheet.Contains(sheet.SheetName))
                                continue;

                            ExportSheet(sheet, Path.GetFileNameWithoutExtension(inputFile.Name), codeOutputDirectory, dataOutputDirectory);
                            ++exported;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("C++ protobuf export failed for {0}: {1}: {2}", inputFile.FullName, e.GetType().FullName, e.Message);
                    return -1;
                }
            }

            return exported;
        }

        private static void ExportSheet(ISheet sheet, string fileName, DirectoryInfo codeOutputDirectory, DirectoryInfo dataOutputDirectory)
        {
            Context.Instance.CurrentSheet = sheet;
            var meta = new DefGenMeta().GenerateMeta(sheet, fileName);
            File.WriteAllText(Path.Combine(codeOutputDirectory.FullName, meta.ClassName + ".proto"), new CppGenCode().Generate(meta));

            // Reuse the existing C# protobuf-net contract and serializer so the
            // data framing and field semantics remain compatible with --backend csharp.
            var generatedCSharp = new ConcurrentDictionary<string, string>();
            new DefGenCode().GenerateCode(meta, ref generatedCSharp);
            new DefSerializer().Serialize(meta, new DefLoader().Load(sheet, meta), dataOutputDirectory, generatedCSharp);
        }
    }
}
