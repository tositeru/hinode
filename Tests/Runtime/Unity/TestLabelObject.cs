using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Unity
{
    /// <summary>
	/// <seealso cref="LabelObject"/>
	/// </summary>
    public class TestLabelObject : TestBase
    {
        /// <summary>
		/// <seealso cref="LabelObject.Add(string)"/>
		/// <seealso cref="LabelObject.Contains(string[])"/>
		/// <seealso cref="LabelObject.Remove(string)"/>
		/// <seealso cref="LabelObject.Count"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();

            var word = "word";
            label.Add(word);
            Assert.IsTrue(label.Contains(word));
            Assert.AreEqual(1, label.Count);

            label.Remove(word);
            Assert.IsFalse(label.Contains(word));
            Assert.AreEqual(0, label.Count);
            yield return null;
        }

        /// <summary>
        /// <seealso cref="LabelObject.AddRange(string)"/>
        /// <seealso cref="LabelObject.Contains(string[])"/>
        /// <seealso cref="LabelObject.RemoveRange(string)"/>
		/// <seealso cref="LabelObject.Count"/>
		/// <seealso cref="LabelObject.Clear()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddAndRemoveRangePasses()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();

            var words = new string[] { "Apple", "Orange", "Grape" };
            label.AddRange(words);
            Assert.IsTrue(label.Contains(words));
            Assert.AreEqual(words.Length, label.Count);

            label.Remove("Apple");
            Assert.IsFalse(label.Contains(words));
            Assert.IsTrue(label.Contains("Orange", "Grape"));
            Assert.AreEqual(2, label.Count);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { "Orange", "Grape" }
                , label
                , ""
            );

            label.Clear();
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { }
                , label
                , ""
            );
            yield return null;
        }

        /// <summary>
		/// <seealso cref="LabelObject.Add"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator ValidLabelPasses()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();

            Assert.DoesNotThrow(() => {
                label.Add("apple");
                label.Add("1234");
                label.Add("APPLE");
                label.Add("____");
                label.Add("_Ap1");
                label.Add("1a_E3");
            });
            Debug.Log($"Success to Valid Label!");

            foreach (var l in new string[] {
                "!",
                "?",
                ".",
                "@a0kf",
                "|",
                "(",
                ")",
                "$",
            })
            {
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                    label.Add(l);
                });
            }
            Debug.Log($"Success to Invalid Label!");
            yield return null;
        }
    }
}
