using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    /// <summary>
    /// test case
    /// ## Constructor
    /// - Empty
    /// - string[]
    /// - IEnumerable{string}
    /// ## Add
    /// - single string
    /// - string[]
    /// - IEnumerable{string}
    /// - ignore !IsValid(label)
    /// ## Remove
    /// - single string
    /// - string[]
    /// - IEnumerable{string}
    /// - Clear
    /// ## Contain
    /// ## DoMatch
    /// - Complete
    /// - Included
    /// - Partial
    /// ## IsValid
    /// <seealso cref="Labels"/>
    /// </summary>
    public class TestLabels
    {
        const int ORDER_Contstrutor = 0;
        const int ORDER_Add = 0;
        const int ORDER_Remove = ORDER_Contstrutor + 100;
        const int ORDER_IEnumerable_Interface = 0;
        const int ORDER_Contain = ORDER_Contstrutor + 100;
        const int ORDER_DoMatch = ORDER_Contstrutor + 100;
        const int ORDER_IsValid = 0;

        #region Constructor
        [Test, Order(ORDER_Contstrutor), Description("Empty")]
        public void Constructor_Empty_Passes()
        {
            var labels = new Labels();
            Assert.AreEqual(0, labels.Count);
        }

        [Test, Order(ORDER_Contstrutor), Description("pass to string[]")]
        public void Constructor_StringArray_Passes()
        {
            var labels = new Labels("apple", "orange", "grape");
            AssertionUtils.AssertEnumerableByUnordered(
                new string[]
                {
                    "apple", "orange", "grape"
                }
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Contstrutor), Description("pass to IEnumerable<string>")]
        public void Constructor_StringEnumerable_Passes()
        {
            var strs = new List<string>() { "apple", "orange", "grape" };
            var labels = new Labels(strs);
            AssertionUtils.AssertEnumerableByUnordered(
                strs
                , labels
                , ""
            );
        }
        #endregion

        #region Add
        [Test, Order(ORDER_Add), Description("single string")]
        public void Add_Passes()
        {
            var labels = new Labels();
            labels.Add("apple");
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] {"apple"}
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Add), Description("pass to string[]")]
        public void AddRange_StringArray_Passes()
        {
            var labels = new Labels();
            labels.AddRange("apple", "orange", "grape");
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { "apple", "orange", "grape" }
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Add), Description("pass to IEnumerable<string>")]
        public void AddRange_StringEnumerable_Passes()
        {
            var strs = new List<string>() { "apple", "orange", "grape" };
            var labels = new Labels();
            labels.AddRange(strs);
            AssertionUtils.AssertEnumerableByUnordered(
                strs
                , labels
                , ""
            );
        }
        
        [Test, Order(ORDER_Add), Description("ignore !IsValid(label)")]
        public void Add_IgnoreInvalid_Passes()
        {
            var strs = new List<string>() { "apple", "orange", "grape", "", null, "    " };
            var labels = new Labels();
            labels.AddRange(strs);
            AssertionUtils.AssertEnumerableByUnordered(
                strs.Where(_s => Labels.IsValid(_s))
                , labels
                , ""
            );
        }
        #endregion

        #region Remove
        [Test, Order(ORDER_Remove), Description("single string")]
        public void Remove_Passes()
        {
            var labels = new Labels();
            var str = "str";
            labels.AddRange(str, "apple");

            labels.Remove(str);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { "apple" }
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Remove), Description("pass to string[]")]
        public void RemoveRange_StringArray_Passes()
        {
            var labels = new Labels();
            labels.AddRange("apple", "orange", "grape");

            labels.RemoveRange("apple", "grape");
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { "orange" }
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Remove), Description("pass to IEnumerable<string>")]
        public void RemoveRange_StringEnumerable_Passes()
        {
            var strs = new List<string>() { "apple", "orange", "grape" };
            var labels = new Labels();
            labels.AddRange(strs);

            labels.RemoveRange(new List<string> { "orange", "grape" });
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { "apple" }
                , labels
                , ""
            );
        }

        [Test, Order(ORDER_Remove), Description("Remove All")]
        public void Clear_Passes()
        {
            var labels = new Labels("a", "b", "c");
            labels.Clear();
            Assert.AreEqual(0, labels.Count);
        }
        #endregion

        #region Contiain
        [Test, Order(ORDER_Contain), Description("")]
        public void Contain_Passes()
        {
            var labels = new Labels("apple");
            Assert.IsTrue(labels.Contains("apple"));
            Assert.IsFalse(labels.Contains("grape"));
            Assert.IsFalse(labels.Contains("hoge"));
        }
        #endregion

        #region DoMatch
        class DoMatchTest
        {
            public const string LABEL1 = "1";
            public const string LABEL2 = "2";
            public const string LABEL3 = "3";

            public static void AssertAttributeLabels(Labels labels, Labels.MatchOp op, (bool isOK, string[] labels)[] testData)
            {
                foreach (var t in testData)
                {
                    Assert.AreEqual(t.isOK, labels.DoMatch(op, t.labels), $"labels => {labels.Aggregate((_s, _c) => _s + ", " + _c)}, got => {t.labels.Aggregate((_s, _c) => _s + ", " + _c)}");
                }
            }
        }


        /// <summary>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Complete Operation")]
        public void DoMatch_CompleteOp_Pass()
        {
            var op = Labels.MatchOp.Complete;
            {//test LABEL1
                var labels = new Labels(DoMatchTest.LABEL1);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchTest.LABEL1 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test LABEL2
                var labels = new Labels(DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test LABEL1, LABEL2
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test LABEL1, LABEL3
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL3);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }

        /// <summary>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Included Operation")]
        public void DoMatch_IncludedOp_Pass()
        {
            var type = typeof(DoMatchTest);
            var op = Labels.MatchOp.Included;
            {//test LABEL1
                var labels = new Labels(DoMatchTest.LABEL1);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL3, DoMatchTest.LABEL1 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test LABEL2
                var labels = new Labels(DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test LABEL1, LABEL2
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test LABEL1, LABEL3
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL3);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }

        /// <summary>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="Labels.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Partial Operation")]
        public void DoMatch_PartialOp_Pass()
        {
            var type = typeof(DoMatchTest);
            var op = Labels.MatchOp.Partial;
            {//test LABEL1
                var labels = new Labels(DoMatchTest.LABEL1);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (false, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test LABEL2
                var labels = new Labels(DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test LABEL1, LABEL2
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL2);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test LABEL1, LABEL3
                var labels = new Labels(DoMatchTest.LABEL1, DoMatchTest.LABEL3);
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchTest.LABEL1 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL1, DoMatchTest.LABEL3 }),
                    (false, new string[]{ DoMatchTest.LABEL2 }),
                    (true, new string[]{ DoMatchTest.LABEL2, DoMatchTest.LABEL3 }),
                    (true, new string[]{ DoMatchTest.LABEL3 }),
                };
                DoMatchTest.AssertAttributeLabels(labels, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }
        #endregion

        #region IsValid
        /// <summary>
        /// <seealso cref="Labels.IsValid(string)"/>
        /// </summary>
        [Test, Order(ORDER_IsValid), Description("")]
        public void IsValid_Passes()
        {
            Assert.IsTrue(Labels.IsValid("apple"));

            Assert.IsFalse(Labels.IsValid(""));
            Assert.IsFalse(Labels.IsValid(null));
            Assert.IsFalse(Labels.IsValid("   "));
            Assert.IsFalse(Labels.IsValid("   apple"));
            Assert.IsFalse(Labels.IsValid("apple  "));
            Assert.IsFalse(Labels.IsValid("a p p l e"));
        }
        #endregion
    }
}
