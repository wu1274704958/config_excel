using System;
using ProtoBuf;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace conf
{
    [ProtoContract]
    public class Test
    {
        [ProtoMember(1)] public Int32 Id { get; private set; }
        [ProtoMember(2)] public String Name;
        public conf.Test2 Ref => conf.Test2Mgr.GetInstance().Get(Id);
    }

    [ProtoContract]
    public class TestMgr
    {
        [ProtoMember(1)] private Dictionary<Int32,Test> _dict = new Dictionary<int, Test>();
        public Test Get(int id) => _dict.TryGetValue(id, out var t) ? t : null;
        private static TestMgr _instance = null;
        public static TestMgr GetInstance()=> _instance;
        private static FileInfo _lastReadFile = null;
        protected TestMgr() { }

        public static void InitInstance(FileInfo file)
        {
            _instance = null;
            using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read))
            {
                _instance = Serializer.Deserialize<TestMgr>(fs);
                Debug.Assert(_instance != null,"Load Config Test failed at "+file.FullName);
                _lastReadFile = file;
            }
        }

        public static void Reload()
        {
            if(_lastReadFile == null) return;
            InitInstance(_lastReadFile);
        }
        public static void Save(FileInfo file)
        {
            using (FileStream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Write))
            {
                Serializer.Serialize(fs, _instance);
            }
        }
        public static void AppendData(Int32 id,Test d)
        {
            if (_instance == null)
                _instance = new TestMgr();
            Debug.Assert(_instance._dict.ContainsKey(id) == false,"Append Same Test id = " + id);
            _instance._dict.Add(id,d);
        }
    }
}