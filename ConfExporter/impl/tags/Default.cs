using System;
using core;

namespace impl.tags
{
    public class Default : BaseTagParser<Default>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            throw new Exception("Default can not get default value");
        }

        public override Type ValueType => typeof(string);
    }
}