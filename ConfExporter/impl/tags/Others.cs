﻿using System;
using core;

namespace impl.tags
{
    public class Others : BaseTagParser<Others>
    {
        public override object GetDefaultValue(IFiledType type)
        {
            return null;
        }
        public override Type ValueType => typeof(string);
        public override bool MatchKey(string key)
        {
            return true;
        }
    }
}