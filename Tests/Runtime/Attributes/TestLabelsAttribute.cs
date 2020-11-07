using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Attributes
{
    /// <summary>
    /// <seealso cref="LabelsAttribute"/>
    /// </summary>
    public class TestLabelsAttribute
    {
        const int ORDER_Contain = 0;
        const int ORDER_DoMatch = 0;

        #region Contain
        class ContainTest
        {
            public const string LABEL = "1";
            [Labels(Labels = new string[] { LABEL })]
            public int Field;
        }

        /// <summary>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_Contain), Description("")]
        public void Contain_Pass()
        {
            var type = typeof(ContainTest);
            var attr = type.GetField("Field").GetCustomAttribute<LabelsAttribute>();

            Assert.IsTrue(attr.Contains(ContainTest.LABEL));

            Assert.IsFalse(attr.Contains(""));
            Assert.IsFalse(attr.Contains(null));
            Assert.IsFalse(attr.Contains("Apple"));
            Assert.IsFalse(attr.Contains("Book"));
        }
        #endregion

        #region DoMatchLabels
        class DoMatchLabelsTest
        {
            public const string LABEL1 = "LABEL1";
            public const string LABEL2 = "LABEL2";
            public const string LABEL3 = "LABEL3";

            [Labels(Labels = new string[] { LABEL1 })]
            public void Func() => Debug.Log("A#Func");
            [Labels(Labels = new string[] { LABEL2 })]
            public void Func2() => Debug.Log("A#Func2");
            [Labels(Labels = new string[] { LABEL1, LABEL2 })]
            public void Func3() => Debug.Log("A#Func3");
            [Labels(Labels = new string[] { LABEL3, LABEL1 })]
            public void Func4() => Debug.Log("A#Func4");
        }

        void AssertAttributeLabels(LabelsAttribute attr, Labels.MatchOp op, (bool isOK, string[] labels)[] testData)
        {
            foreach (var t in testData)
            {
                Assert.AreEqual(t.isOK, attr.DoMatch(op, t.labels), $"labels => {attr.Labels.Aggregate((_s, _c) => _s + ", " + _c)}, got => {t.labels.Aggregate((_s, _c) => _s + ", " + _c)}");
            }
        }

        /// <summary>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Complete Operation")]
        public void DoMatchLabels_CompleteOp_Pass()
        {
            var type = typeof(DoMatchLabelsTest);
            var op = Labels.MatchOp.Complete;
            {//test Func()
                var funcInfo = type.GetMethod("Func");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test Func2()
                var funcInfo = type.GetMethod("Func2");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test Func3()
                var funcInfo = type.GetMethod("Func3");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test Func4()
                var funcInfo = type.GetMethod("Func4");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }

        /// <summary>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, string[])"/>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Included Operation")]
        public void DoMatchLabels_IncludedOp_Pass()
        {
            var type = typeof(DoMatchLabelsTest);
            var op = Labels.MatchOp.Included;
            {//test Func()
                var funcInfo = type.GetMethod("Func");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL3, DoMatchLabelsTest.LABEL1 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test Func2()
                var funcInfo = type.GetMethod("Func2");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test Func3()
                var funcInfo = type.GetMethod("Func3");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test Func4()
                var funcInfo = type.GetMethod("Func4");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }

        /// <summary>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, IEnumerable{string})"/>
        /// <seealso cref="LabelsAttribute.DoMatch(Labels.MatchOp, string[])"/>
        /// </summary>
        /// <returns></returns>
        [Test, Order(ORDER_DoMatch), Description("Partial Operation")]
        public void DoMatchLabels_PartialOp_Pass()
        {
            var type = typeof(DoMatchLabelsTest);
            var op = Labels.MatchOp.Partial;
            {//test Func()
                var funcInfo = type.GetMethod("Func");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func()!!");

            {//test Func2()
                var funcInfo = type.GetMethod("Func2");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (false, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func2()!!");

            {//test Func3()
                var funcInfo = type.GetMethod("Func3");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func3()!!");

            {//test Func4()
                var funcInfo = type.GetMethod("Func4");
                var attr = funcInfo.GetCustomAttribute<LabelsAttribute>();
                var testData = new (bool isOK, string[] labels)[]
                {
                    (true, new string[]{ DoMatchLabelsTest.LABEL1 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL1, DoMatchLabelsTest.LABEL3 }),
                    (false, new string[]{ DoMatchLabelsTest.LABEL2 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL2, DoMatchLabelsTest.LABEL3 }),
                    (true, new string[]{ DoMatchLabelsTest.LABEL3 }),
                };
                AssertAttributeLabels(attr, op, testData);
            }
            Logger.Log(Logger.Priority.High, () => "Success to Func4()!!");
        }
        #endregion
    }
}
