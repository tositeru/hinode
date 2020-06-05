using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.MVC.Tests.ViewLayout
{
    /// <summary>
    /// <seealso cref="ViewLayoutValueDictionary"/>
    /// </summary>
    public class TestViewLayoutValueDictionary
    {
        [Test]
        public void BasicUsagePasses()
        {
            var layoutValueDict = new ViewLayoutValueDictionary();
            var key = "layoutName";
            var value = 100;
            layoutValueDict.AddValue(key, value);
            Assert.IsTrue(layoutValueDict.ContainsKey(key), $"Don't have key({key})...");
            Assert.AreEqual(value, layoutValueDict.GetValue(key), $"Don't equal value... key={key}, correct={value}, got={layoutValueDict.GetValue(key)}");
            Assert.AreEqual(1, layoutValueDict.Count);

            layoutValueDict.RemoveValue(key);
            Assert.IsFalse(layoutValueDict.ContainsKey(key), $"Already have key({key})...");
            Assert.AreEqual(0, layoutValueDict.Count);
        }

        [Test]
        public void AddSameKeywordValueFail()
        {
            var layoutValueDict = new ViewLayoutValueDictionary();
            var key = "layoutName";
            var value = 100;
            layoutValueDict.AddValue(key, value);


            Assert.Throws<System.ArgumentException>(() => {
                layoutValueDict.AddValue(key, 1.23f);
            }, $"Don't add same key...");
        }

        [Test]
        public void ClearPasses()
        {
            var layoutValueDict = new ViewLayoutValueDictionary();
            var key = "layoutName";
            var key2 = "layoutName2";
            var key3 = "layoutName3";
            layoutValueDict
                .AddValue(key, 100)
                .AddValue(key2, "apple")
                .AddValue(key3, 1.23f);

            layoutValueDict.Clear();

            Assert.AreEqual(0, layoutValueDict.Count);
            Assert.IsFalse(layoutValueDict.ContainsKey(key), $"Don't clear Key({key})");
            Assert.IsFalse(layoutValueDict.ContainsKey(key2), $"Don't clear Key({key2})");
            Assert.IsFalse(layoutValueDict.ContainsKey(key3), $"Don't clear Key({key3})");
        }

    }
}
