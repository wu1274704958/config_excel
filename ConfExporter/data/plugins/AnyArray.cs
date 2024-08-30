using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace conf.plugin
{

    public static class Serialize
    {
        public static T Deserialize<T>(string s)
        {
            var o = Deserialize(s, typeof(T));
            if (o == null) return default(T);
            return (T)o;
        }
        public static object Deserialize(string s, System.Type t)
        {
            if (t == typeof(string))
            {
                return s;
            }
            if (t == typeof(int))
            {
                if (int.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(char))
            {
                if (char.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(long))
            {
                if (long.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(float))
            {
                if (float.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(bool))
            {
                if (bool.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(double))
            {
                if (double.TryParse(s, out var v))
                    return v;
            }
            if (t == typeof(Vector2))
            {
                string[] arr = s.Split(',');
                if (arr.Length >= 2)
                {
                    Vector2 v = new Vector2(Deserialize<float>(arr[0]), Deserialize<float>(arr[1]));
                    return v;
                }
            }
            if (t == typeof(Vector3))
            {
                string[] arr = s.Split(',');
                if (arr.Length >= 3)
                {
                    Vector3 v = new Vector3(Deserialize<float>(arr[0]), Deserialize<float>(arr[1]), Deserialize<float>(arr[2]));
                    return v;
                }
            }
            if (t == typeof(Vector4))
            {
                string[] arr = s.Split(',');
                if (arr.Length >= 4)
                {
                    Vector4 v = new Vector4(Deserialize<float>(arr[0]), Deserialize<float>(arr[1]), Deserialize<float>(arr[2]), Deserialize<float>(arr[3]));
                    return v;
                }
            }
            if (t == typeof(AnyArray))
            {
                try
                {
                    AnyArray array = new AnyArray(s);
                    return array;
                }
                catch //(Exception e)
                { }
            }
            return null;
        }

    }
    public static class AnyArrayTyMap
    {
        public static readonly Dictionary<string, System.Type> TypeMap = new Dictionary<string, System.Type>(){
            { "b",typeof(bool)},
            { "i",typeof(int)},
            { "l",typeof(long)},
            { "f",typeof(float)},
            { "lf",typeof(double)},
            { "s",typeof(string)},
            { "vec2",typeof(Vector2)},
            { "vec3",typeof(Vector3)},
            { "vec4",typeof(Vector4)},
            { "ch",typeof(char)},
        };
    }

    [ProtoBuf.ProtoContract]
    public class AnyArray
    {
        [ProtoBuf.ProtoMember(1)]
        protected string originString;
        protected List<object> objs;
        protected List<System.Type> tyArr;
        public int Count => objs.Count;
        protected AnyArray() { }
        private static readonly char TypeDelimiter = '@';
        private static readonly char Delimiter = '|';

        public object this[int i]
        {
            get
            {
                if (good_idx(i))
                    return objs[i];
                return null;
            }
            set
            {
                if (good_idx(i))
                {
                    objs[i] = value;
                    tyArr[i] = value.GetType();
                }
            }
        }

        public AnyArray(string str, bool notParse = false)
        {
            if (notParse)
            {
                originString = str;
                return;
            }
            if (str.Length == 0)
            {
                this.objs = new List<object>();
                this.tyArr = new List<Type>();
            }
            else
            if (parse(str, out var objs, out var tys))
            {
                this.objs = objs;
                this.tyArr = tys;
            }
            else
            {
                throw new Exception("Parse AnyArray Failed!!!");
            }
        }

        public void Init()
        {
            if (originString != null && originString.Length > 0 && parse(originString, out var objs, out var tys))
            {
                this.objs = objs;
                this.tyArr = tys;
            }
            else
            {
                throw new Exception("Parse AnyArray Failed!!!");
            }
        }

        public AnyArray(object[] a)
        {
            objs = new List<object>();
            tyArr = new List<Type>();
            foreach (var t in a)
            {
                if (t == null) continue;
                tyArr.Add(t.GetType());
                objs.Add(t);
            }
        }

        public static bool TryParse(string str, out AnyArray arr)
        {
            arr = null;
            try
            {
                arr = new AnyArray(str);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Parse Any Array failed src = {str}");
                return false;
            }
        }

        public bool good_idx(int idx)
        {
            return idx >= 0 && idx < objs.Count;
        }
        public bool TypeEq<T>(int idx)
        {
            if (good_idx(idx))
            {
                var inTy = typeof(T);
                if (inTy == tyArr[idx])
                    return true;
                else
                {
                    var currTy = tyArr[idx];
                    while (currTy.BaseType != null)
                    {
                        if (currTy.BaseType == inTy)
                            return true;
                        currTy = currTy.BaseType;
                    }
                    return false;
                }
            }
            return false;
        }

        public T Get<T>(int idx)
        {
            if (TypeEq<T>(idx))
            {
                return (T)objs[idx];
            }
            return default(T);
        }

        public bool TryGet<T>(int idx, out T v, T def = default)
        {
            v = def;
            if (TypeEq<T>(idx))
            {
                v = (T)objs[idx];
                return true;
            }
            return false;
        }

        public object[] GetValues()
        {
            return objs.ToArray();
        }
        public List<object> GetValuesInside()
        {
            return objs;
        }

        public bool parse(string str, out List<object> objs, out List<System.Type> tys)
        {
            var cs = str.Trim().ToCharArray();
            int step = 0;
            System.Type currTy = null;
            objs = new List<object>();
            tys = new List<Type>();
            for (int i = 0; i < cs.Length; ++i)
            {
                var it = cs[i];
                if (step == 0 && it == '[')
                {
                    ++step; continue;
                }
                if (step == 1 && it == ']')
                {
                    if (i + 1 == cs.Length)
                    {
                        step = 100; break;
                    }
                }
                if (step == 1)
                {
                    if (GetType(cs, i, out var ty, out var new_it))
                    {
                        i = new_it; currTy = ty; step += 1;
                        continue;
                    }
                    else return false;
                }
                if (step == 2)
                {
                    if (GetVal(cs, i, currTy, out var obj, out var nit))
                    {
                        i = nit; step = 1;
                        objs.Add(obj);
                        tys.Add(currTy);
                        if (cs[i] == ']')
                        {
                            step = 100;
                            break;
                        }
                        else continue;
                    }
                    else return false;
                }
            }
            return step == 100;
        }

        private bool GetType(char[] arr, int it, out System.Type ty, out int new_it)
        {
            ty = null; new_it = it;
            var step = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = it; i < arr.Length; ++i)
            {
                var c = arr[i];
                if (step == 0)
                {
                    if (c != ' ')
                    {
                        sb.Append(c);
                        step += 1;
                    }
                    continue;
                }
                if (step == 1)
                {
                    if (c != TypeDelimiter)
                        sb.Append(c);
                    else
                    {
                        step += 1;
                        new_it = i;
                        ty = GetTypeByTag(sb.ToString().TrimEnd());
                        break;
                    }
                }
            }
            return step == 2 && ty != null;
        }

        private bool GetVal(char[] arr, int it, System.Type ty, out object val, out int new_it)
        {
            val = null; new_it = it;
            var step = 0;
            bool isEscapeCharacter = false;
            StringBuilder sb = new StringBuilder();
            for (int i = it; i < arr.Length; ++i)
            {
                var c = arr[i];
                if (c == '\\')
                {
                    isEscapeCharacter = true;
                    continue;
                }
                if (step == 0)
                {
                    if (isEscapeCharacter)
                    {
                        sb.Append(c);
                        step += 1;
                        isEscapeCharacter = false;
                        continue;
                    }
                    if (c != ' ')
                    {
                        sb.Append(c);
                        step += 1;
                    }
                    continue;
                }
                if (step == 1)
                {
                    if (isEscapeCharacter)
                    {
                        sb.Append(c);
                        isEscapeCharacter = false;
                        continue;
                    }
                    if (c != Delimiter && c != ']')
                        sb.Append(c);
                    else
                    {
                        step += 1;
                        new_it = i;
                        val = Serialize.Deserialize(sb.ToString().TrimEnd(), ty);
                        break;
                    }
                }
            }
            return step == 2 && val != null;
        }

        private System.Type GetTypeByTag(string res)
        {
            if (AnyArrayTyMap.TypeMap.TryGetValue(res, out var s))
            {
                return s;
            }
            return null;
        }

        public bool ToDict<K, V>(int begin, int end, out Dictionary<K, V> dict, out int step)
        {
            dict = null; step = 0;
            if ((end - begin) % 2 != 0)
                return false;
            dict = new Dictionary<K, V>();
            for (int i = begin; i < end; i += 2)
            {
                if (typeof(K) == tyArr[i] && typeof(V) == tyArr[i + 1])
                {
                    dict.Add((K)objs[i], (V)objs[i + 1]);
                }
                else
                {
                    return false;
                }
            }
            step = end - begin;
            return true;
        }

        public object[] GetRange(int start)
        {
            if (start >= Count) return null;
            return objs.GetRange(start, objs.Count - start).ToArray();
        }
        public object[] GetRangeByEOF(int start, out int step)
        {
            step = 0;
            if (start >= Count) return null;
            for (int i = start; i < Count; ++i)
            {
                ++step;
                if (tyArr[i] == typeof(char) && objs[i] is char ch && ch == '0')
                {
                    if (i - start <= 0) return null;
                    return objs.GetRange(start, i - start).ToArray();
                }
            }
            step = 0;
            return null;
        }
        public object[] GetRange(int start, int count)
        {
            if (start >= Count) return null;
            return objs.GetRange(start, count).ToArray();
        }
        public AnyArray GetRangeArr(int start)
        {
            if (start >= Count) return null;
            return new AnyArray
            {
                objs = objs.GetRange(start, objs.Count - start),
                tyArr = tyArr.GetRange(start, objs.Count - start),
            };
        }
        public AnyArray GetRangeArr(int start, int count)
        {
            if (start >= Count) return null;
            return new AnyArray
            {
                objs = objs.GetRange(start, count),
                tyArr = tyArr.GetRange(start, count),
            };
        }
        public void Add<T>(T v)
        {
            if (v == null) return;
            objs.Add(v);
            tyArr.Add(v.GetType());
        }
        public void PopBack()
        {
            if (Count <= 0) return;
            objs.RemoveAt(Count - 1);
            tyArr.RemoveAt(Count - 1);
        }
        public bool AllIs<T>()
        {
            if (tyArr == null || tyArr.Count == 0) return false;
            for (int i = 0; i < tyArr.Count; ++i)
            {
                if (!TypeEq<T>(i))
                    return false;
            }
            return true;
        }
        public List<T> GetAllAndIs<T>()
        {
            if (tyArr == null || tyArr.Count == 0) return null;
            List<T> arr = new List<T>();
            for (int i = 0; i < objs.Count; ++i)
            {
                if (objs[i] is T t)
                    arr.Add(t);
                else
                    return null;
            }
            return arr;
        }
    }
}
