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

    public static class Array2DExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T[,] _array)
        {
            return new Array2DEnumerable<T>(_array);
        }

        public static IEnumerable<(T, int x, int y)> AsEnumerableWithIndex<T>(this T[,] _array)
        {
            return new Array2DWithIndexEnumerable<T>(_array);
        }

        class Array2DEnumerable<T> : IEnumerable<T>, IEnumerable
        {
            T[,] _target;
            public Array2DEnumerable(T[,] target)
            {
                _target = target;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var v in _target)
                {
                    yield return v;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        class Array2DWithIndexEnumerable<T> : IEnumerable<(T, int, int)>, IEnumerable
        {
            T[,] _target;
            public Array2DWithIndexEnumerable(T[,] target)
            {
                _target = target;
            }

            public IEnumerator<(T, int, int)> GetEnumerator()
            {
                for (var x = 0; x < _target.GetLength(0); ++x)
                {
                    for (var y = 0; y < _target.GetLength(1); ++y)
                    {
                        yield return (_target[x, y], y, x);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
