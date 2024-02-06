using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using core;
using Microsoft.CSharp;

namespace impl
{
    public class DefSerializer : ISerializer<DefMetaData, TableData>
    {
        public bool Serialize(DefMetaData meta, TableData data, DirectoryInfo dir, ConcurrentDictionary<string, string> codeList)
        {
            Assembly assembly = CompileCode(codeList);
            var mgrTy = AppendData(assembly,meta,data);
            var appendMethod = mgrTy.GetMethod("Save",BindingFlags.Static | BindingFlags.Public,null,new Type[]{ typeof(FileInfo) },null);
            if (appendMethod == null) throw new Exception($"Serialize assembly not found method: {mgrTy.FullName}.Save(FileInfo)");
            var file = new FileInfo(dir.FullName + Path.DirectorySeparatorChar + meta.ClassName + ".dat");
            appendMethod.Invoke(null,new object[]{ file });
            return true;
        }

        private Type AppendData(Assembly assembly, DefMetaData meta, TableData data)
        {
            Type mgrTy = assembly.GetType(meta.FullMgrClassName);
            if(mgrTy == null) throw new Exception("Serialize assembly not found class: " + meta.FullMgrClassName);
            Type dataTy = assembly.GetType(meta.FullClassName);
            if(dataTy == null) throw new Exception("Serialize assembly not found class: " + meta.FullClassName);
            var appendMethod = mgrTy.GetMethod("AppendData",BindingFlags.Static | BindingFlags.Public,null,new Type[]{ meta.Key.Type.Type,dataTy},null);
            if (appendMethod == null) throw new Exception($"Serialize assembly not found method: {mgrTy.FullName}.AppendData({meta.Key.Type.Type.FullName},{dataTy.FullName})");
            for (int i = 0; i < data.Data.Count; i++)
            {
                var obj = Activator.CreateInstance(dataTy);
                var row = data.Data[i];
                var key = row[meta.KeyIndex];
                for (int j = 0; j < meta.Fileds.Count; j++)
                {
                    var f = meta.Fileds[j];
                    dataTy.GetProperty(f.Name).SetValue(obj, row[j]);
                }
                appendMethod.Invoke(null,new object[] { key,obj });
            }
            return mgrTy;
        }

        private Assembly CompileCode(ConcurrentDictionary<string,string> codeList)
        {
            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,  // 将程序集生成在内存中
                TreatWarningsAsErrors = false
            };

            // 添加需要引用的程序集（例如：System.dll）
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("protobuf-net.Core.dll");
            compilerParams.ReferencedAssemblies.Add("protobuf-net.dll");
            compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Memory.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");

            // 创建 C# 编译器
            CSharpCodeProvider codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
            // 编译代码并获取编译结果
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(compilerParams, codeList.Values.ToArray());

            if (compilerResults.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError e in compilerResults.Errors)
                {
                    sb.AppendLine(e.ToString());
                }
                // 处理编译错误
                throw new Exception($"Compile error: {sb}");
            }
            else
            {
                return compilerResults.CompiledAssembly;
            }
        }
    }
}