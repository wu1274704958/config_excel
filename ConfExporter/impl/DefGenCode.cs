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
            //custom enum
            if(meta.Tags.TryGetValue(nameof(CustomEnum), out var customEnumDict))
                AppendCustomEnum(sb, meta, customEnumDict as Dictionary<string,List<(string,string)>>);
            //data class
            var sealedVal = meta.Tags.TryGetValue(nameof(Sealed), out var v) ? (int)v : 0;
            AppendClassHead(sb, meta.ClassName,(sealedVal & 1) == 1);
            AppendLeftCurlyBraces(sb);
            AppendClassBody(sb, meta);
            AppendRightCurlyBraces(sb);
            //mgr class
            AppendClassHead(sb, meta.MgrClassName,(sealedVal & 2) == 2);
            AppendLeftCurlyBraces(sb);
            AppendMgrClassBody(sb, meta);
            AppendRightCurlyBraces(sb);
            //end
            AppendRightCurlyBraces(sb);
            var dir = meta.Package;
            var dotIdx = -1;
            if ((dotIdx = dir.IndexOf('.')) > 0)
                dir = dir.Substring(dotIdx + 1);
            
            if(!codeList.TryAdd($"{dir}/{meta.MgrClassName}", sb.ToString()))
                throw new Exception("Same code exists: "+meta.MgrClassName);
            return res;
        }

        private void AppendCustomEnum(StringBuilder sb, DefMetaData meta, Dictionary<string,List<(string,string)>> dictionary)
        {
            foreach (var it in dictionary)
            {
                AppendCustomEnum(sb, meta, it.Key, it.Value);
            }
        }

        private void AppendCustomEnum(StringBuilder sb, DefMetaData meta, string name, List<(string, string)> kv)
        {
            sb.Append($@"public enum {name} {{");
            foreach (var it in kv)
            {
                if(it.Item2 == null)
                    sb.Append($"{it.Item1},");
                else
                    sb.Append($"{it.Item1} = {it.Item2},");
            }
            sb.Append("}");
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
        _instance = Serializer.DeserializeWithLengthPrefix<{mgrName}>(fs, PrefixStyle.Fixed32);
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
        Serializer.SerializeWithLengthPrefix(fs, _instance, PrefixStyle.Fixed32);
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
                if (f.Tags.TryGetValue(nameof(EnumRef), out var eValue))
                    sb.AppendLine($"public {eValue} {f.Name}_e => ({eValue}){f.Name};");
                ++i;
            }
        }

        private void AppendClassHead(StringBuilder sb, string className,bool @sealed = false)
        {
            var preClass = @sealed ? "sealed" : "";
            sb.Append($@"
[ProtoContract]
public {preClass} class {className}
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


