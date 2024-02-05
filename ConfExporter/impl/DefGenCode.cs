using System.Collections.Generic;
using System.Text;
using core;

namespace impl
{
    public class GenCodeResult
    {
        
    }
    public class DefGenCode : IGenCode<DefMetaData, GenCodeResult>
    {
        public GenCodeResult GenerateCode(DefMetaData meta, out List<KeyValuePair<string, string>> codeList)
        {
            codeList = new List<KeyValuePair<string, string>>();
            var sb = new StringBuilder();
            AppendImport(sb);
            return null;
        }

        private void AppendImport(StringBuilder sb)
        {
            sb.AppendLine("using System.Collections.Concurrent;");
            sb.AppendLine("using ProtoBuf;");
            sb.AppendLine("using System;");
        }
    }
}


