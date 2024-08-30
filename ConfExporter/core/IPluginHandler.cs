using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfExporter.core
{
    public interface IPluginHandler<in MD>
    {
        bool Handle(MD metadata,ref ConcurrentDictionary<string, string> codeDict);
    }
}
