using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

namespace ConfExporter.unit_test
{
    public interface ITest
    {
        int test(int a,int b);
    }
    public class TestDynLoadCode
    {
        

        public static void Test()
        {
            var code = @"
public class TestImpl
{
    public int test(int a,int b)
    {
        return a + b;
    }
}
            ";
            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,  // 将程序集生成在内存中
                TreatWarningsAsErrors = false
            };

            // 添加需要引用的程序集（例如：System.dll）
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("protobuf-net.Core.dll");
            compilerParams.ReferencedAssemblies.Add("protobuf-net.dll");
            // 创建 C# 编译器
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            // 编译代码并获取编译结果
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(compilerParams, code);

            if (compilerResults.Errors.HasErrors)
            {
                // 处理编译错误
                foreach (CompilerError error in compilerResults.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorText}");
                }
            }
            else
            {
                // 获取编译后的程序集
                Assembly assembly = compilerResults.CompiledAssembly;

                // 创建动态类的实例并执行方法
                Type dynamicClassType = assembly.GetType("TestImpl");
                object dynamicClassInstance = Activator.CreateInstance(dynamicClassType);
                MethodInfo executeMethod = dynamicClassType.GetMethod("test", new Type[] { typeof(int), typeof(int) });
                var res = executeMethod?.Invoke(dynamicClassInstance, new object[] { 1, 2 });
                Console.WriteLine($"val = {res}");
            }
        }
    }
}