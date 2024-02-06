using System.Collections.Concurrent;
using System.IO;

namespace core
{
    public interface ISerializer<in MD,in D>
    {
        bool Serialize(MD meta, D data, DirectoryInfo dir, ConcurrentDictionary<string, string> codeList);
    }
}