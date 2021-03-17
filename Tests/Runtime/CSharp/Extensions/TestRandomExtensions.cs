using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
	/// <seealso cref="RandomExtensions"/>
	/// </summary>
    public class TestRandomExtensions
    {
        class RangeDoubleABTestParam : IABTest
        {
            public double Min { get; set; }
            public double Max { get; set; }

            System.Random UseRandom { get; set; }
            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("Min", Min.ToString()),
                    ("Max", Max.ToString()),
                };
            }

            protected override void InitParams(System.Random rnd)
            {
                var tmp = rnd.NextDouble();
                var max = rnd.NextDouble();
                Min = System.Math.Min(tmp, max);
                Max = System.Math.Max(tmp, max);

                UseRandom = rnd;
            }

            protected override void TestMethod()
            {
                var value = UseRandom.Range(Min, Max);

                var errorMessage = $"Fail test... result={value}";
                Assert.IsTrue(Min <= value && value <= Max, errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="RandomExtensions.Range(System.Random, double, double)"/>
		/// </summary>
        [Test]
        public void ABTestRangeDouble()
        {
            var settings = TestSettings.CreateOrGet();
            var ABTest = new RangeDoubleABTestParam();
            ABTest.RunTest(settings);
        }

        class RangeIntABTestParam : IABTest
        {
            public int Min { get; set; }
            public int Max { get; set; }

            System.Random UseRandom { get; set; }
            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("Min", Min.ToString()),
                    ("Max", Max.ToString()),
                };
            }

            protected override void InitParams(System.Random rnd)
            {
                var tmp = rnd.Next();
                var max = rnd.Next();
                Min = System.Math.Min(tmp, max);
                Max = System.Math.Max(tmp, max);

                UseRandom = rnd;
            }

            protected override void TestMethod()
            {
                var value = UseRandom.Range(Min, Max);

                var errorMessage = $"Fail test... result={value}";
                Assert.IsTrue(Min <= value && value <= Max, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="RandomExtensions.Range(System.Random, int, int)"/>
        /// </summary>
        [Test]
        public void ABTestRangeInt()
        {
            var settings = TestSettings.CreateOrGet();
            var ABTest = new RangeDoubleABTestParam();
            ABTest.RunTest(settings);
        }

        class RangeFloatABTestParam : IABTest
        {
            public float Min { get; set; }
            public float Max { get; set; }

            System.Random UseRandom { get; set; }
            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("Min", Min.ToString()),
                    ("Max", Max.ToString()),
                };
            }

            protected override void InitParams(System.Random rnd)
            {
                var tmp = (float)rnd.NextDouble();
                var max = (float)rnd.NextDouble();
                Min = System.Math.Min(tmp, max);
                Max = System.Math.Max(tmp, max);

                UseRandom = rnd;
            }

            protected override void TestMethod()
            {
                var value = UseRandom.Range(Min, Max);

                var errorMessage = $"Fail test... result={value}";
                Assert.IsTrue(Min <= value && value <= Max, errorMessage);
            }
        }
        /// <summary>
        /// <seealso cref="RandomExtensions.Range(System.Random, float, float)"/>
        /// </summary>
        [Test]
        public void ABTestRangeFloat()
        {
            var settings = TestSettings.CreateOrGet();
            var ABTest = new RangeDoubleABTestParam();
            ABTest.RunTest(settings);
        }

        /// <summary>
        /// <seealso cref="RandomExtensions.RandomString(System.Random, int)"/>
        /// </summary>
        [Test]
        public void RandomString_Passes()
        {
            var rnd = new System.Random();
            for(var i=0; i<10000; ++i)
            {
                var length = rnd.Next() % 100;
                var str = rnd.RandomString(length);
                Assert.AreEqual(length, str.Length);
                Assert.IsTrue(str.All(_c => !(char.IsControl(_c) || (_c & 0x80) != 0)));
            }

            //output random string
            {
                var r = new System.Random();
                var strs = Enumerable.Range(0, 10).Select(_i => r.RandomString(r.Next() % 50))
                    .Aggregate("", (_s, _c) => _s + System.Environment.NewLine + _c);
                Debug.Log($"output random strings VV below VV: {System.Environment.NewLine}" + strs);
            }
        }

        enum RandomGetTestEnum
        {
            A, B, C, D, E, F, G, H, I, J,
        }
        [Test]
        public void Get_Passes()
        {
            var rnd = new System.Random();
            var array1 = System.Array.CreateInstance(typeof(int), 10);
            for (var i = 0; i < array1.Length; ++i) array1.SetValue(i, i);

            var list = Enumerable.Range(0, 10).ToList();
            var enumList = System.Enum.GetValues(typeof(RandomGetTestEnum));
            for (var i=0; i<10000; ++i)
            {
                var v = rnd.Get(array1);
                Assert.AreNotEqual(-1, System.Array.IndexOf(array1, v), $"Fail Get(System.Array)... get value={v}");

                var v2 = rnd.Get(list);
                Assert.AreNotEqual(-1, list.IndexOf(v2), $"Fail Get(List<T>)... get value={v}");

                var e = rnd.Get<RandomGetTestEnum>();
                Assert.AreNotEqual(-1, System.Array.IndexOf(enumList, e), $"Fail Get<Enum>()... get value={e}");
            }
        }
    }
}
