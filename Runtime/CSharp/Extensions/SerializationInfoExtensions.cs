using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    public static class SerializationInfoExtensions
    {
        public static IEnumerable<SerializationEntry> GetEnumerable(this SerializationInfo t)
        {
            return new SerializationEntryEnumerable(t);
        }


        class SerializationEntryEnumerable : IEnumerable<SerializationEntry>, IEnumerable
        {
            SerializationInfo _target;
            public SerializationEntryEnumerable(SerializationInfo target)
            {
                _target = target;
            }

            public IEnumerator<SerializationEntry> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<SerializationEntry>, IEnumerator, System.IDisposable
            {
                SerializationInfo _target;
                SerializationInfoEnumerator _enumerator;
                public Enumerator(SerializationInfo target)
                {
                    _target = target;
                    Reset();
                }
                public SerializationEntry Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = _target.GetEnumerator();
            }
        }

    }
}
