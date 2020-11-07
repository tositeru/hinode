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
        const int ORDER_BASIC_USAGE = 0;
        const int ORDER_DO_MATCH = ORDER_BASIC_USAGE + 100;
        const int ORDER_GET_LABEL_OBJECT = 0;

        /// <summary>
		/// <seealso cref="LabelObject.Add(string)"/>
		/// <seealso cref="LabelObject.Contains(string[])"/>
		/// <seealso cref="LabelObject.Remove(string)"/>
		/// <seealso cref="LabelObject.Count"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest, Order(ORDER_BASIC_USAGE), Description("")]
        public IEnumerator BasicUsage_Passes()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();

            var word = "word";
            label.Labels.Add(word);
            Assert.IsTrue(label.Contains(word));
            Assert.AreEqual(1, label.Count);

            label.Labels.Remove(word);
            Assert.IsFalse(label.Contains(word));
            Assert.AreEqual(0, label.Count);
            yield return null;
        }

        #region DoMatch
        /// <summary>
        /// <seealso cref="LabelObject.DoMatch{T}(out T, string[])"/>
        /// <seealso cref="LabelObject.DoMatch(out Component, System.Type, string[])"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest(), Order(ORDER_DO_MATCH), Description("")]
        public IEnumerator DoMatch_Passes()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();
            label.Labels.AddRange("Apple", "Orange");
            var com = label.gameObject.AddComponent<BoxCollider>();

            {
                Assert.IsTrue(label.DoMatch(out var getCom, typeof(BoxCollider), "Apple", "Orange"));
                Assert.AreSame(com, getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(Match ComponentType and Labels)!");

            {
                Assert.IsTrue(label.DoMatch(out var getCom, typeof(BoxCollider), "Apple"));
                Assert.AreSame(com, getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(Match ComponentType and Labels 2)!");

            {
                Assert.IsFalse(label.DoMatch(out var getCom, typeof(BoxCollider), "Hoge"));
                Assert.AreSame(com, getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(Match ComponentType and not Match Labels)!");

            {
                Assert.IsFalse(label.DoMatch(out var getCom, typeof(MeshFilter), "Apple"));
                Assert.IsNull(getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(not Match ComponentType and Match Labels)!");

            {
                Assert.IsFalse(label.DoMatch(out var getCom, typeof(MeshFilter), "Hoge"));
                Assert.IsNull(getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(not Match ComponentType and Labels)!");

            {
                var labels = new string[] { "Apple", "Orange" };
                Assert.AreEqual(label.Contains(labels), label.DoMatch(out var getCom, null, labels));
                Assert.IsNull(getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(Not ComponentType and Match Labels)!");

            {
                var labels = new string[] { "Apple", "hoge" };
                Assert.AreEqual(label.Contains(labels), label.DoMatch(out var getCom, null, labels));
                Assert.IsNull(getCom);
            }
            Logger.Log(Logger.Priority.High, () => "Success to DoMatch(Not ComponentType and not Match Labels)!");

            yield break;
        }
        #endregion

        #region GetLabelObject
        [UnityTest, Order(ORDER_GET_LABEL_OBJECT), Description("")]
        public IEnumerator GetLabelObject_Passes()
        {
            var gameObject = new GameObject("obj");
            var label = gameObject.AddComponent<LabelObject>();

            Assert.AreSame(label, LabelObject.GetLabelObject(gameObject));
            Assert.AreSame(label, LabelObject.GetLabelObject(gameObject.transform));
            Assert.AreSame(label, LabelObject.GetLabelObject(label));


            Assert.IsNull(LabelObject.GetLabelObject(100));
            Assert.IsNull(LabelObject.GetLabelObject(""));
            yield break;
        }
        #endregion
    }
}
