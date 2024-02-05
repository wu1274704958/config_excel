using System;
using ProtoBuf;
using System.Collections.Concurrent;

namespace conf
{
    [ProtoContract]
    public class Test
    {
        [ProtoMember(1)] public Int32 Id;
        [ProtoMember(2)] public String Name;
    }

    [ProtoContract]
    public class TestMgr
    {
        
    }
}