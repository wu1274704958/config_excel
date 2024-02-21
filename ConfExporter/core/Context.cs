using System;
using NPOI.SS.UserModel;

namespace core
{
    //single instance
    public class Context 
    {
        private static readonly Lazy<Context> _instance = new Lazy<Context>(() => new Context());
        public static Context Instance => _instance.Value;

        private Context()
        {
        }
        public IWorkbook CurrentBook { get; set; }
        public ISheet CurrentSheet { get; set; }
    }
}