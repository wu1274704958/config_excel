using System;
using core;

namespace impl.tags
{
    public class EnumRef : BaseTagParser<EnumRef>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            throw new Exception("EnumRef can not get default value");
        }

        public override Type ValueType => typeof(string);
    }
}