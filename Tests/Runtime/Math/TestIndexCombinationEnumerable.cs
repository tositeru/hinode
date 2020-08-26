using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Math
{
    /// <summary>
	/// <seealso cref="IndexCombinationEnumerable"/>
	/// </summary>
    public class TestIndexCombinationEnumerable
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            //foreach(var pair in new IndexCombinationEnumerable(4))
            //{
            //    var log = pair.Select(_i => _i.ToString()).Aggregate((_s, _c) => _s + "," + _c);
            //    Debug.Log(log);
            //}

            AssertionUtils.AssertEnumerable(
                new List<int[]>()
                {
                    new int[] { 0 },
                    new int[] { 0, 1 },
                    new int[] { 0, 1, 2 },
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 3 },
                    new int[] { 0, 2 },
                    new int[] { 0, 2, 3 },
                    new int[] { 0, 3 },
                    new int[] { 1 },
                    new int[] { 1, 2 },
                    new int[] { 1, 2, 3 },
                    new int[] { 1, 3 },
                    new int[] { 2 },
                    new int[] { 2, 3 },
                    new int[] { 3 },
                },
                new IndexCombinationEnumerable(4),
                "",
                (_correct, _got) => {
                    var correct = _correct.ToArray();
                    var got = _got.ToArray();
                    if (correct.Length != got.Length)
                    {
                        Debug.LogWarning($"Not Equal Length... corrent={correct.Length}, got={got.Length}");
                        Logger.LogWarning(Logger.Priority.High, () => {
                            var correctList = correct.Select(_e => _e.ToString()).Aggregate((_s, _c) => _s + "," + _c);
                            var gotList = got.Select(_e => _e.ToString()).Aggregate((_s, _c) => _s + "," + _c);
                            return $"correct list=> {correctList};{System.Environment.NewLine}got list=>{gotList}";
                        });
                        return false;
                    }
                    for (var i = 0; i < correct.Length; ++i)
                    {
                        if (correct[i] != got[i])
                        {
                            Debug.LogWarning($"Not Equal element[{i}]... corrent={correct[i]}, got={got[i]}");
                            Logger.LogWarning(Logger.Priority.High, () => {
                                var correctList = correct.Select(_e => _e.ToString()).Aggregate((_s, _c) => _s + "," + _c);
                                var gotList = got.Select(_e => _e.ToString()).Aggregate((_s, _c) => _s + "," + _c);
                                return $"correct list=> {correctList};{System.Environment.NewLine}got list=>{gotList}";
                            });
                            return false;
                        }
                    }
                    return true;
                }
            );
        }

        [System.Flags]
        enum TestFlags
        {
            A = 0x1,
            B = 0x1 << 1,
            C = 0x1 << 2,
            D = 0x1 << 3,
            E = 0x1 << 4,
        }

        /// <summary>
        /// <seealso cref="IndexCombinationEnumerable.GetFlagEnumCombination{T}(T[])"/>
        /// <seealso cref="IndexCombinationEnumerable.GetFlagEnumCombination{T}(IEnumerable{T})"/>
        /// </summary>
        [Test]
        public void GetFlagEnumCombinationPasses()
        {
            var flagsEnumerable = IndexCombinationEnumerable.GetFlagEnumCombination(
                TestFlags.A,
                TestFlags.D,
                TestFlags.E
                );
            //foreach(var f in flagsEnumerable)
            //{
            //    Debug.Log($"flags => {f}");
            //}
            AssertionUtils.AssertEnumerable(
                new TestFlags[] {
                    TestFlags.A,
                    TestFlags.A | TestFlags.D,
                    TestFlags.A | TestFlags.D | TestFlags.E,
                    TestFlags.A | TestFlags.E,
                    TestFlags.D,
                    TestFlags.D | TestFlags.E,
                    TestFlags.E,
                },
                flagsEnumerable,
                ""
            );
        }
    }
}
