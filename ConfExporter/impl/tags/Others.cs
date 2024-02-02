using System;
using core;

namespace impl.tags
{
    public class Others : BaseTagParser<Others>
    {
        public override Type ValueType => typeof(string);
        public override object DefaultValue => null;
        public override bool MatchKey(string key)
        {
            return true;
        }
    }
}