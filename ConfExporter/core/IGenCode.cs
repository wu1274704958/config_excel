using System.Collections.Generic;
using NPOI.XSSF.Streaming.Values;

namespace core
{
    public interface IGenCode<in MD,out C>
    {
        C GenerateCode(MD meta,out List<KeyValuePair<string,string>> codeList);
    }
}