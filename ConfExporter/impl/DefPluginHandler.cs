using ConfExporter.core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace impl
{
    public class DefPluginHandler : IPluginHandler<DefMetaData>
    {
        public bool Handle(DefMetaData metadata, ref ConcurrentDictionary<string, string> codeDict)
        {
            foreach(var t in metadata.Fileds)
            {
                if(t.Type.PluginFiles != null && t.Type.PluginFiles.Length > 0)
                {
                    foreach(string s in t.Type.PluginFiles)
                    {
                        if (codeDict.ContainsKey(s))
                            continue;
                        var code = GetPlugin(s);
                        if(code != null)
                            codeDict.TryAdd(s, code);
                        else
                        {
                            Console.WriteLine("DefPluginHandler load plugin failed key = " + s);
                        }
                    }
                }
            }

            return !codeDict.IsEmpty;
        }

        private string GetPlugin(string s)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 读取嵌入资源
            using (Stream stream = assembly.GetManifestResourceStream($"{assembly.FullName.Split(',')[0]}.data.plugins.{s}"))
            {
                if (stream == null)
                    return null;

                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    return content;
                }
            }
        }
    }
}
