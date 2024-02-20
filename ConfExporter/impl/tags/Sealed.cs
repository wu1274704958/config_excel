using System;
using core;

namespace impl.tags
{
    public class Sealed : BaseTagParser<Sealed>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return 1;
        }
        public override Type ValueType => typeof(int);
    }
}