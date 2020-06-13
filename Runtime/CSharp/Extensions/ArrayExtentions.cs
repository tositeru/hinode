using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// System.Arrayの拡張メソッド
    /// 
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestArrayExtensions"/>
    /// </summary>
    public static class ArrayExtentions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestArrayExtensions.GetEnumerablePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<object> GetEnumerable(this System.Array t)
        {
            return new Enumerable(t);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestArrayExtensions.GetEnumerableWithTypePasses()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumerable<T>(this System.Array t)
            => t.GetEnumerable().OfType<T>();

        class Enumerable : IEnumerable<object>, IEnumerable
        {
            System.Array _target;
            public Enumerable(System.Array target)
            {
                _target = target;
            }

            public IEnumerator<object> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<object>, IEnumerator, System.IDisposable
            {
                System.Array _target;
                IEnumerator _enumerator;
                public Enumerator(System.Array target)
                {
                    _target = target;
                    Reset();
                }
                public object Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = _target.GetEnumerator();
            }
        }

    }
}
