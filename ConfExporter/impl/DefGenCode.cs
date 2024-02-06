using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using core;
using impl.tags;

namespace impl
{
    public class GenCodeResult
    {
        
    }
    public class DefGenCode : IGenCode<DefMetaData, GenCodeResult>
    {
        public GenCodeResult GenerateCode(DefMetaData meta, ref ConcurrentDictionary<string,string> codeList)
        {
            var sb = new StringBuilder();
            var res = new GenCodeResult();
            AppendImport(sb);
            AppendNameSpaceHead(sb, meta.Package);
            AppendLeftCurlyBraces(sb);
            //data class
            AppendClassHead(sb, meta.ClassName);
            AppendLeftCurlyBraces(sb);
            AppendClassBody(sb, meta);
            AppendRightCurlyBraces(sb);
            //mgr class
            AppendClassHead(sb, meta.MgrClassName);
            AppendLeftCurlyBraces(sb);
            AppendMgrClassBody(sb, meta);
            AppendRightCurlyBraces(sb);
            
            AppendRightCurlyBraces(sb);
            
            if(!codeList.TryAdd(meta.MgrClassName, sb.ToString()))
                throw new Exception("Same code exists: "+meta.MgrClassName);
            return res;
        }

        private void AppendMgrClassBody(StringBuilder sb, DefMetaData meta)
        {
            var keyTy = meta.Key.TypeClassName;
            var mgrName = meta.MgrClassName;
            sb.Append($@"
[ProtoMember(1)] private Dictionary<{keyTy},{meta.ClassName}> _dict = new Dictionary<{keyTy}, {meta.ClassName}>();
public {meta.ClassName} Get({keyTy} id) => _dict.TryGetValue(id, out var t) ? t : null;
private static {mgrName} _instance = null;
public static {mgrName} GetInstance()=> _instance;
private static FileInfo _lastReadFile = null;
protected {mgrName}() {{ }}
public static void InitInstance(FileInfo file)
{{
    _instance = null;
    using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read))
    {{
        _instance = Serializer.Deserialize<{mgrName}>(fs);
        Debug.Assert(_instance != null,""Load Config {meta.ClassName} failed at ""+file.FullName);
        _lastReadFile = file;
    }}
}}

public static void Reload()
{{
    if(_lastReadFile == null) return;
    InitInstance(_lastReadFile);
}}
public static void Save(FileInfo file)
{{
    using (FileStream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Write))
    {{
        Serializer.Serialize(fs, _instance);
    }}
}}
public static void AppendData(Int32 id,{meta.ClassName} d)
{{
    if (_instance == null)
        _instance = new {mgrName}();
    Debug.Assert(_instance._dict.ContainsKey(id) == false,""Append Same {meta.ClassName} id = "" + id);
    _instance._dict.Add(id,d);
}}
            ");
        }

        private void AppendClassBody(StringBuilder sb, DefMetaData meta)
        {
            int i = 1;
            foreach (var f in meta.Fileds)
            {
                sb.AppendLine($"[ProtoMember({i})] public {f.TypeClassName} {f.Name} {{ get; private set; }}");
                if (f.Tags.TryGetValue(nameof(Reference), out var v))
                    sb.AppendLine($"public {v} {f.Name}Ref => {v}Mgr.GetInstance().Get({f.Name});");
                ++i;
            }
        }

        private void AppendClassHead(StringBuilder sb, string className)
        {
            sb.Append($@"
[ProtoContract]
public class {className}
            ");
        }

        private void AppendLeftCurlyBraces(StringBuilder sb)
        {
            sb.AppendLine("{");
        }
        private void AppendRightCurlyBraces(StringBuilder sb)
        {
            sb.AppendLine("}");
        }

        private void AppendNameSpaceHead(StringBuilder sb, string package)
        {
            sb.Append($@"
namespace {package}
            ");
        }

        private void AppendImport(StringBuilder sb)
        {
            sb.Append(@"
using System;
using ProtoBuf;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
            ");
        }
        
        
    }
}


