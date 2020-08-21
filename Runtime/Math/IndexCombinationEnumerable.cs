using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
	/// 与えられた長さの配列に対する添字の全ての組み合わせを列挙していくEnumerable
	///
	/// ex) new AllEnumFlagCombinationEnumerable(3)
	///  => 0
	///     0, 1
	///     0, 2
	///     0, 1, 2
	///     1
	///     1, 2
	///     2
	/// </summary>
    public class IndexCombinationEnumerable : IEnumerable<IEnumerable<int>>, IEnumerable
    {
        int _length = 0;
        public IndexCombinationEnumerable(int length)
        {
            _length = length;
        }

        public IEnumerator<IEnumerable<int>> GetEnumerator()
        {
            int[] rowIndecies = Enumerable.Repeat(-1, _length).ToArray();
            while (Increment(rowIndecies))
            {
                yield return new IndexEnumerable(rowIndecies);
            }
        }

        bool Increment(int[] rowIndecies)
        {
            var index = 0;
            for(index = rowIndecies.Length-1; index >= 0; --index)
            {
                if (-1 != rowIndecies[index])
                    break;
            }

            if(index < 0)
            {
                rowIndecies[0] = 0;
            }
            else
            {
                if (index+1 < rowIndecies.Length && -1 == rowIndecies[index + 1]) index += 1;

                if(rowIndecies[index] == -1)
                {
                    rowIndecies[index] = rowIndecies[index-1] + 1;
                }
                else
                {
                    rowIndecies[index]++;
                }

                if (rowIndecies[index] >= rowIndecies.Length)
                {
                    rowIndecies[index] = -1;
                    for (var j = index-1; j >= 0; --j)
                    {
                        rowIndecies[j]++;
                        if (rowIndecies[j] < rowIndecies.Length)
                            break;
                        rowIndecies[j] = -1;
                    }
                }
            }

            return rowIndecies[0] != -1;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        class IndexEnumerable : IEnumerable<int>, IEnumerable
        {
            int[] _list;
            public IndexEnumerable(int[] list)
            {
                _list = list;
            }

            public IEnumerator<int> GetEnumerator()
            {
                for(var i=0; i<_list.Length; ++i)
                {
                    if (_list[i] != -1) yield return _list[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }


        public static IEnumerable<T> GetFlagEnumCombination<T>(params T[] flags)
            where T : System.Enum
        {
            return new FlagEnumCombinationEnumerable<T>(flags)
                .Select(_f => (T)(object)_f);
        }
        public static IEnumerable<T> GetFlagEnumCombination<T>(IEnumerable<T> flags)
            where T : System.Enum
            => GetFlagEnumCombination(flags.ToArray());

        class FlagEnumCombinationEnumerable<T> : IEnumerable<int>, IEnumerable
            where T : System.Enum
        {
            T[] _flags;
            public FlagEnumCombinationEnumerable(T[] flags)
            {
                Assert.IsNotNull(flags);
                Assert.IsTrue(typeof(T).GetCustomAttribute<System.FlagsAttribute>() != null, $"{typeof(T)} is not System.Flag...");

                _flags = flags;
            }

            public IEnumerator<int> GetEnumerator()
            {
                foreach(var indexPair in new IndexCombinationEnumerable(_flags.Length))
                {
                    yield return indexPair
                        .Where(_i => _i != -1)
                        .Select(_i => (int)(object)(_flags[_i]))
                        .Aggregate((_s, _c) => _s | _c);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        }
    }
}
