using System;
using core;

namespace impl.tags
{
    public class NotNull : BaseTagParser<NotNull>
    {
        public override Type ValueType => typeof(bool);
        public override object DefaultValue => true;
    }
}