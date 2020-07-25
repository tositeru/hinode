using System.Collections;
using System.Collections.Generic;
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
    }
}
