using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public static class TypeExtensions
    {
        public static FieldInfo GetFieldInHierarchy(this System.Type t, string name, BindingFlags flags = BindingFlags.Default)
        {
            while(t != null)
            {
                var info = t.GetField(name, flags);
                if (info != null)
                    return info;
                t = t.BaseType;
            }
            return null;
        }

        public static bool IsArrayOrList(this System.Type t)
        {
            if(t.IsArray) return true;
            //IList, IList<>から派生しているか？
            return t.GetInterfaces().Any(_i => _i.Equals(typeof(IList)) || _i.Equals(typeof(IList<>)));
        }

        public static System.Type GetArrayElementType(this System.Type t)
        {
            Assert.IsTrue(t.IsArrayOrList());
            if (t.IsArray)
            {
                return t.GetElementType();
            }

            var elementType = t.GetInterfaces()
                .Where(_i => _i.IsGenericType)
                .Where(_i => _i.GetGenericTypeDefinition().Equals(typeof(IList<>)))
                .Select(_i => _i.GetGenericArguments()[0])
                .FirstOrDefault();
            return elementType ?? typeof(object);
        }

        public static IEnumerable<System.Type> GetClassHierarchyEnumerable(this System.Type t)
        {
            return new ClassHierarchyEnumerable(t);
        }

        class ClassHierarchyEnumerable : IEnumerable<System.Type>, IEnumerable
        {
            System.Type _target;
            public ClassHierarchyEnumerable(System.Type target)
            {
                _target = target;
            }

            public IEnumerator<System.Type> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<System.Type>, IEnumerator, System.IDisposable
            {
                System.Type _target;
                IEnumerator<System.Type> _enumerator;
                public Enumerator(System.Type target)
                {
                    _target = target;
                    Reset();
                }
                public System.Type Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = GetEnumerator();

                IEnumerator<System.Type> GetEnumerator()
                {
                    var t = _target;
                    while(t != null)
                    {
                        yield return t;
                        t = t.BaseType;
                    }
                }
            }
        }

    }
}
