using System.Collections.Concurrent;

namespace core
{
    public interface IGenCode<in MD,out C>
    {
        C GenerateCode(MD meta,ref ConcurrentDictionary<string,string> codeList);
    }
}