using System;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;

namespace conf
{
    [ProtoContract]
    public class Test2
    {
        [ProtoMember(1)] public Int32 Id;
        [ProtoMember(2)] public String Name;
    }

    [ProtoContract]
    public class Test2Mgr
    {
        [ProtoMember(1)] public Dictionary<Int32,Test2> Tests = new Dictionary<int, Test2>();
        public Test2 Get(int id) => Tests.TryGetValue(id, out var t) ? t : null;
        private static Test2Mgr _instance = null;
        public static Test2Mgr GetInstance() => _instance;

        private static void InitInstance(string path)
        {
            
        }
    }
}