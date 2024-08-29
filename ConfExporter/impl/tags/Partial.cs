using System;
using core;

namespace impl.tags
{
    public class Partial : BaseTagParser<Partial>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return 1;
        }
        public override Type ValueType => typeof(int);
    }
}