#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;

namespace Hinode.Tests
{
    public static class AssertionUtils
    {
        public static void AssertEnumerable<T>(IEnumerable<T> corrects, IEnumerable<T> gots, string message, System.Func<T, T, bool> comparer = null)
        {
            if (gots == null && corrects == null) return;
            Assert.IsTrue(gots != null && corrects != null, $"{message}: 片方がnullになっています... correct=>{corrects != null} gots=>{gots !=null}");

            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            var index = 0;
            foreach (var (t, correct) in gots.Zip(corrects, (_t, _c) => (t: _t, c: _c)))
            {
                Assert.IsTrue(comparer(correct, t), $"{message}: don't be same... index=>{index}, correct={correct}, got={t}");
                index++;
            }
            Assert.AreEqual(corrects.Count(), index, $"{message}: Don't Equal count...");
        }

        public static void AssertEnumerableByUnordered<T>(IEnumerable<T> corrects, IEnumerable<T> gots, string message, System.Func<T, T, bool> comparer =null)
        {
            if (gots == null && corrects == null) return;
            Assert.IsTrue(gots != null && corrects != null, $"{message}: 片方がnullになっています... correct=>{corrects != null} gots=>{gots != null}");

            var correctList = corrects.ToList();
            Assert.AreEqual(corrects.Count(), gots.Count(), $"{message}: Don't Equal count...");

            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;
            foreach (var g in gots)
            {
                Assert.IsTrue(correctList.Any(_o => comparer(_o, g)), $"{message}: Don't exist {g}...");
                correctList.Remove(correctList.First(_o => comparer(_o, g)));
            }
            Assert.IsTrue(0 == correctList.Count(), $"{message}: Don't match elements...");
        }

        public static void AreNearlyEqual(float correct, float got, float epsilon, string message)
        {
            Assert.IsTrue(Utils.AreFloatsEqual(correct, got, epsilon), $"Not Nearly Equal... correct={correct}, got={got}, epslion={epsilon}. {message}");
        }
        public static void AreNearlyEqual(float correct, float got, float epsilon=float.Epsilon)
        {
            AreNearlyEqual(correct, got, epsilon, "");
        }

        public static void AreEqual<TKey, TValue>(Dictionary<TKey, TValue> correct, Dictionary<TKey, TValue> got, string message)
        {
            AreEqual(correct.AsEnumerable(), got.AsEnumerable(), message);
        }

        public static void AreEqual<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> correct, IEnumerable<KeyValuePair<TKey, TValue>> got, string message)
        {
            Assert.AreEqual(correct.Count(), got.Count(), $"Not Equal Count of Element... {message}");
            foreach (var t in got)
            {
                var cor = correct.FirstOrDefault(_t => _t.Key.Equals(t.Key));
                Assert.IsNotNull(cor, $"Not Exist Key({t.Key})... {message}");
                Assert.AreEqual(cor.Value, t.Value, $"Not Equal Value of key({t.Key})... {message}");
            }
        }

        public static void AreEqualComplexArray(IEnumerable<Complex> corrects, IEnumerable<Complex> gots, string message, float epsilon = 1e-4f)
        {
            Assert.AreEqual(corrects.Count(), gots.Count(), $"Not Equal Length... {message}");
            foreach (var ((correct, got), index) in corrects
                .Zip(gots, (_c, _o) => (correct: _c, got: _o))
                .Zip(Enumerable.Range(0, corrects.Count()), (_p, _i) => (pair: _p, index: _i)))
            {
                Assert.IsTrue(correct.NearlyEqual(got, epsilon), $"Not Equal Nearly by Index({index})... correct={correct}, got={got} {message}");
            }
        }
    }
}
#endif