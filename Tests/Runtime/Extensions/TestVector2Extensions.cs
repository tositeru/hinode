using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Hinode.Tests.Extensions
{
    public class TestVector2Extensions : TestBase
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TryParsePasses()
        {
            var v = new Vector2(1.234f, 4.567f);
            var text = v.ToString("F4");
            Assert.IsTrue(Vector2Extensions.TryParse(text, out var result), $"パースに失敗しました. text={text}");
            AssertionUtils.AreNearlyEqual(v.x, result.x, float.Epsilon);
            AssertionUtils.AreNearlyEqual(v.y, result.y, float.Epsilon);

            text = "1.2345, 23";
            Assert.IsTrue(Vector2Extensions.TryParse(text, out result), $"パースに失敗しました. text={text}");
            AssertionUtils.AreNearlyEqual(1.2345f, result.x);
            AssertionUtils.AreNearlyEqual(23f, result.y);

            text = "(1.2345, 23";
            Assert.IsTrue(Vector2Extensions.TryParse(text, out result), $"パースに失敗しました. text={text}");
            AssertionUtils.AreNearlyEqual(1.2345f, result.x);
            AssertionUtils.AreNearlyEqual(23f, result.y);

            text = "hfohf39do";
            Assert.IsFalse(Vector2Extensions.TryParse(text, out var _), $"対応していないテキストのパースに成功しています. text={text}");
            text = "[1.2345, 23]";
            Assert.IsFalse(Vector2Extensions.TryParse(text, out var _), $"対応していないテキストのパースに成功しています. text={text}");
            text = "(1.2345, 23]";
            Assert.IsFalse(Vector2Extensions.TryParse(text, out var _), $"対応していないテキストのパースに成功しています. text={text}");
            text = "[1.2345, 23)";
            Assert.IsFalse(Vector2Extensions.TryParse(text, out var _), $"対応していないテキストのパースに成功しています. text={text}");
            text = "(1.2345 23)";
            Assert.IsFalse(Vector2Extensions.TryParse(text, out var _), $"対応していないテキストのパースに成功しています. text={text}");

        }
    }
}
