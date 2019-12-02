using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    public class SerializedFieldInfo
    {
        readonly object _value;
        readonly FieldInfo _info;
        public object Value { get => _value; }
        public FieldInfo FieldInfo { get => _info; }

        public SerializedFieldInfo(object value, FieldInfo info)
        {
            _value = value;
            _info = info;
        }
    }

    /// <summary>
    /// publicなFieldとSerializedFieldAttributeが指定されたFieldを探索するEnumerable
    /// </summary>
    public class SerializedFieldEnumerable : IEnumerable<SerializedFieldInfo>, IEnumerable
    {
        object _target;
        public SerializedFieldEnumerable(object target)
        {
            _target = target;
        }

        public IEnumerator<SerializedFieldInfo> GetEnumerator()
        {
            return new Enumerator(_target);
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        class Enumerator : IEnumerator<SerializedFieldInfo>, IEnumerator, System.IDisposable
        {
            object _target;
            IEnumerator<SerializedFieldInfo> _enumerator;

            public Enumerator(object target)
            {
                _target = target;
                Reset();
            }
            public SerializedFieldInfo Current => _enumerator.Current;
            object IEnumerator.Current => Current;
            public void Dispose() {}
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator = GetSerializedFieldEnumerator();

            IEnumerator<SerializedFieldInfo> GetSerializedFieldEnumerator()
            {
                if (_target == null) yield break;

                var serializableFields = _target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic)
                    .Where(_info => {
                        var fieldInfo = _info as FieldInfo;
                        if (fieldInfo.IsPublic) return true;
                        return fieldInfo.GetCustomAttribute<SerializeField>() != null;
                    });
                foreach(var f in serializableFields)
                {
                    yield return new SerializedFieldInfo(f.GetValue(_target), f);
                }
            }
        }
    }

    /// <summary>
    /// publicなFieldとSerializedFieldAttributeが指定されたFieldを探索するEnumerable
    /// targetにあるフィールドの中も探索します。
    /// <seealso cref="SerializedFieldEnumerable"/>
    /// </summary>
    class SerializedFieldHierarchyEnumerable : IEnumerable<SerializedFieldInfo>, IEnumerable
    {
        object _target;
        public SerializedFieldHierarchyEnumerable(object target)
        {
            _target = target;
        }

        public IEnumerator<SerializedFieldInfo> GetEnumerator()
        {
            return new Enumerator(_target);
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        class Enumerator : IEnumerator<SerializedFieldInfo>, IEnumerator, System.IDisposable
        {
            object _target;
            IEnumerator<SerializedFieldInfo> _enumerator;
            public Enumerator(object target)
            {
                _target = target;
                Reset();
            }
            public SerializedFieldInfo Current => _enumerator.Current;
            object IEnumerator.Current => Current;
            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator = GetSerializedFieldEnumerator();

            IEnumerator<SerializedFieldInfo> GetSerializedFieldEnumerator()
            {
                if (_target == null) yield break;

                var enumerator = _target.GetSerializedFieldEnumerable().GetEnumerator();

                var stack = new Stack<IEnumerator<SerializedFieldInfo>>();
                stack.Push(enumerator);
                while(stack.Count() > 0)
                {
                    var e = stack.Peek();
                    if(e.MoveNext())
                    {
                        yield return e.Current;
                        if(e.Current.GetType().IsClass || e.Current.GetType().IsSubclassOf(typeof(System.ValueType)))
                        {
                            enumerator = e.Current.GetSerializedFieldEnumerable().GetEnumerator();
                            stack.Push(enumerator);
                        }
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
            }
        }
    }

    public static class SerializedFieldEnumerableEx
    {
        public static IEnumerable<SerializedFieldInfo> GetSerializedFieldEnumerable(this object t)
        {
            return new SerializedFieldEnumerable(t);
        }

        public static IEnumerable<SerializedFieldInfo> GetHierarchySerializedFieldEnumerable(this object t)
        {
            return new SerializedFieldHierarchyEnumerable(t);
        }
    }
}
