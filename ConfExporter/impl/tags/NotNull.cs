using System;
using core;

namespace impl.tags
{
    public class NotNull : BaseTagParser<NotNull>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return true;
        }

        public override Type ValueType => typeof(bool);
    }
}