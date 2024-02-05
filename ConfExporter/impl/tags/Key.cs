using System;
using core;

namespace impl.tags
{
    public class Key : BaseTagParser<Key>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return "Id";
        }
        public override Type ValueType => typeof(string);
    }
}