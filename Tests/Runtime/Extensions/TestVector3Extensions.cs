using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
	/// <seealso cref="Vector3Extensions"/>
	/// </summary>
    public class TestVector3Extensions
    {
        readonly float EPSILON = float.Epsilon;

        /// <summary>
		/// <seealso cref="Vector3Extensions.Mul(Vector3, Vector3)"/>
		/// </summary>
        [Test]
        public void MulPasses()
        {
            AssertionUtils.AreNearlyEqual(new Vector3(10, 20, 30), new Vector3(1, 2, 3).Mul(new Vector3(10, 10, 10)), EPSILON);
            AssertionUtils.AreNearlyEqual(new Vector3(100, -20, 0.5f), new Vector3(20, 5, 1f).Mul(new Vector3(5f, -4f, 0.5f)), EPSILON);
        }

        /// <summary>
		/// <seealso cref="Vector3Extensions.Abs(Vector3)"/>
		/// </summary>
        [Test]
        public void AbsPasses()
        {
            var testData = new (Vector3 correct, Vector3 value)[]
            {
                (new Vector3(10, 20, 30), new Vector3(-10, -20, -30)),
                (new Vector3(10, 20, 30), new Vector3(10, 20, 30)),
            };
            foreach(var data in testData)
            {
                var errorMessage = $"Fail test... {data}";
                AssertionUtils.AreNearlyEqual(data.correct, data.value.Abs(), EPSILON, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="Vector3Extensions.AreNearlyEqual(Vector3, Vector3, float)"/>
        /// </summary>
        [Test]
        public void AreNearlyEqualPasses()
        {
            var testData = new (bool correct, Vector3 a, Vector3 b, float epsilon)[]
            {
                (true, new Vector3(1, 2, 3), new Vector3(1, 2, 3), float.Epsilon),
                (true, new Vector3(1, 0.1100005f, 3), new Vector3(1, 0.110002f, 3), 0.01f),
                (false, new Vector3(1, 2, 3), new Vector3(1, 0.0002f, 3), float.Epsilon),
                (false, new Vector3(1, 2, 3), new Vector3(1, 0.02f, 0), float.Epsilon),
                (false, new Vector3(1, 2, 3), new Vector3(0, 0.02f, 3), float.Epsilon),
                (false, new Vector3(1, 2, 3), new Vector3(0, 2, 3), float.Epsilon),
                (false, new Vector3(1, 2, 3), new Vector3(0, 2, 0), float.Epsilon),
            };
            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... {data}";

                Assert.AreEqual(data.correct, data.a.AreNearlyEqual(data.b, data.epsilon), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="Vector3Extensions.TryParse(string, out Vector3)"/>
        /// </summary>
        [Test]
        public void TryParsePasses()
        {
            var v = new Vector3(1.234f, 4.567f, -9.876f);
            var validTexts = new (string text, Vector3 result)[]
            {
                (v.ToString("F4"), v),
                ("1.2345 23 -92", new Vector3(1.2345f, 23, -92)),
                ("1.2345, 23, -92", new Vector3(1.2345f, 23, -92)),
                ("(1.2, 0.4, -92)", new Vector3(1.2f, 0.4f, -92)),
                ("1.2, 0.4, -92)", new Vector3(1.2f, 0.4f, -92)),
                ("(1.2345, 23, -92", new Vector3(1.2345f, 23, -92)),
                ("[1.2345, 23, -92]", new Vector3(1.2345f, 23, -92)),
                ("1.2345, 23, -92]", new Vector3(1.2345f, 23, -92)),
                ("[1.2345, 23, -92", new Vector3(1.2345f, 23, -92)),
            };
            foreach(var data in validTexts)
            {
                Assert.IsTrue(Vector3Extensions.TryParse(data.text, out var result), $"パースに失敗しました. text={data.text}, result={result}");
                AssertionUtils.AreNearlyEqual(data.result, result, EPSILON);
            }

            var invalidTexts = new string[]
            {
                "hfohf39do",
                "ahfoei, 390jfwjf, d930qdjw"
            };
            foreach(var text in invalidTexts)
            {
                Assert.IsFalse(Vector3Extensions.TryParse(text, out var result), $"対応していないテキストのパースに成功しています. text={text}, result={result}");
            }
        }

    }
}
