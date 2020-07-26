using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="Vector2Extensions"/>
    /// </summary>
    public class TestVector2Extensions : TestBase
    {
        readonly float EPSILON = float.Epsilon;

        /// <summary>
        /// <seealso cref="Vector2Extensions.Mul(Vector2, Vector2)"/>
        /// </summary>
        [Test]
        public void MulPasses()
        {
            AssertionUtils.AreNearlyEqual(new Vector2(10, 20), new Vector2(1, 2).Mul(new Vector2(10, 10)), EPSILON);
            AssertionUtils.AreNearlyEqual(new Vector2(100, -20), new Vector2(20, 5).Mul(new Vector2(5f, -4f)), EPSILON);
        }

        /// <summary>
        /// <seealso cref="Vector2Extensions.Abs(Vector3)"/>
        /// </summary>
        [Test]
        public void AbsPasses()
        {
            var testData = new (Vector2 correct, Vector2 value)[]
            {
                (new Vector2(10, 20), new Vector3(-10, -20)),
                (new Vector2(10, 20), new Vector3(10, 20)),
            };
            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... {data}";
                AssertionUtils.AreNearlyEqual(data.correct, data.value.Abs(), EPSILON, errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="Vector2Extensions.AreNearlyEqual(Vector2, Vector2, float)"/>
		/// </summary>
        [Test]
        public void AreNearlyEqualPasses()
        {
            var testData = new (bool correct, Vector2 a, Vector2 b, float epsilon)[]
            {
                (true, new Vector2(1, 2), new Vector2(1, 2), float.Epsilon),
                (true, new Vector2(1, 0.1100005f), new Vector2(1, 0.110002f), 0.01f),
                (false, new Vector2(1, 2), new Vector2(1, 0.0002f), float.Epsilon),
                (false, new Vector2(1, 2), new Vector2(1, 0.02f), float.Epsilon),
                (false, new Vector2(1, 2), new Vector2(0, 0.02f), float.Epsilon),
                (false, new Vector2(1, 2), new Vector2(0, 2f), float.Epsilon),
            };
            foreach(var data in testData)
            {
                var errorMessage = $"Fail test... {data}";

                Assert.AreEqual(data.correct, data.a.AreNearlyEqual(data.b, data.epsilon), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="Vector2Extensions.TryParse(string, out Vector2)"/>
        /// </summary>
        [Test]
        public void TryParsePasses()
        {
            var v = new Vector2(1.234f, 4.567f);
            var validTexts = new (string text, Vector2 result)[]
            {
                (v.ToString("F4"), v),
                ("1.2345 23", new Vector2(1.2345f, 23)),
                ("1.2345, 23", new Vector2(1.2345f, 23)),
                ("(1.2, 0.4)", new Vector2(1.2f, 0.4f)),
                ("1.2, 0.4)", new Vector2(1.2f, 0.4f)),
                ("(1.2345, 23", new Vector2(1.2345f, 23)),
                ("[1.2345, 23]", new Vector2(1.2345f, 23)),
                ("1.2345, 23]", new Vector2(1.2345f, 23)),
                ("[1.2345, 23", new Vector2(1.2345f, 23)),
            };
            foreach (var data in validTexts)
            {
                Assert.IsTrue(Vector2Extensions.TryParse(data.text, out var result), $"パースに失敗しました. text={data.text}, result={result}");
                AssertionUtils.AreNearlyEqual(data.result, result, EPSILON);
            }

            var invalidTexts = new string[]
            {
                "hfohf39do",
                "ahfoei, 390jfwjf"
            };
            foreach (var text in invalidTexts)
            {
                Assert.IsFalse(Vector2Extensions.TryParse(text, out var result), $"対応していないテキストのパースに成功しています. text={text}, result={result}");
            }
        }
    }
}
