using System;
using core;

namespace impl.tags
{
    public class Reference : BaseTagParser<Reference>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return null;
        }
        public override Type ValueType  => typeof(string);
    }
}