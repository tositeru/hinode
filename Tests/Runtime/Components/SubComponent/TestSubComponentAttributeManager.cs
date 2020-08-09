using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Components.SubComponent
{
    /// <summary>
    /// <seealso cref="SubComponentAttributeManager"/>
    /// </summary>
    public class TestSubComponentAttributeManager
    {
        enum Timing
        {
            Init,
            Destroy,
            UpdateUI
        }
        List<(Timing, System.Type)> _reservedRemoveMethods = new List<(Timing, System.Type)>();

        void ReserveRemoveMethod<T>(Timing timing)
            where T : ISubComponentAttribute
            => ReserveRemoveMethod(timing, typeof(T));
        void ReserveRemoveMethod(Timing timing, System.Type type)
        {
            _reservedRemoveMethods.Add((timing, type));
        }

        [TearDown]
        public void RemoveInitMethods()
        {
            foreach(var (timing, type) in _reservedRemoveMethods)
            {
                switch (timing)
                {
                    case Timing.Init:
                        SubComponentAttributeManager.RemoveInitMethod(type);
                        break;
                    case Timing.Destroy:
                        SubComponentAttributeManager.RemoveDestroyMethod(type);
                        break;
                    case Timing.UpdateUI:
                        SubComponentAttributeManager.RemoveUpdateUIMethod(type);
                        break;
                    default:
                        throw new System.NotImplementedException($"Not implement... timing={timing}");
                }

            }
        }

        class AttributeTest : ISubComponentAttribute
        {
            public static object RecievedInstance { get; set; }
            public static int CallCounter { get; set; }

            public static void Method(object inst)
            {
                RecievedInstance = inst;
                CallCounter++;
            }
        }

        class SubCom : ISubComponent<MonoBehaviour>
        {
            public MonoBehaviour RootComponent { get; set; }
            public void Destroy() {}
            public void Init() {}
            public void UpdateUI() { }
        }

        #region Init Methods
        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddInitMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Init);

            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());

            SubComponentAttributeManager.AddInitMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddInitMethodFailWhenNull()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Init);

            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddInitMethod<AttributeTest>(null)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddInitMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddInitMethodFailWhenAlreadyContains()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Init);
            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());

            SubComponentAttributeManager.AddInitMethod<AttributeTest>(AttributeTest.Method);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddInitMethod<AttributeTest>(AttributeTest.Method)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveInitMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Init);

            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());
            SubComponentAttributeManager.AddInitMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());

            SubComponentAttributeManager.RemoveInitMethod<AttributeTest>();
            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveInitMethodFailWhenNotContains()
        {
            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());

            Assert.DoesNotThrow(() =>
                SubComponentAttributeManager.RemoveInitMethod<AttributeTest>()
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RunInitMethods{T}(ISubComponent{T})"/>
        /// </summary>
        [Test]
        public void RunInitMethodsPasses()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Init);

            Assert.IsFalse(SubComponentAttributeManager.ContainsInitMethod<AttributeTest>());
            SubComponentAttributeManager.AddInitMethod<AttributeTest>(AttributeTest.Method);

            AttributeTest.CallCounter = 0;
            AttributeTest.RecievedInstance = null;

            var inst = new SubCom();
            SubComponentAttributeManager.RunInitMethods(inst);

            Assert.AreSame(inst, AttributeTest.RecievedInstance);
            Assert.AreEqual(1, AttributeTest.CallCounter);
        }
        #endregion

        #region Destroy Methods
        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddDestroyMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Destroy);

            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());

            SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddDestroyMethodFailWhenNull()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Destroy);

            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(null)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddDestroyMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddDestroyMethodFailWhenAlreadyContains()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Destroy);
            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());

            SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(AttributeTest.Method);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(AttributeTest.Method)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveDestroyMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Destroy);

            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());
            SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());

            SubComponentAttributeManager.RemoveDestroyMethod<AttributeTest>();
            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveDestroyMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveDestroyMethodFailWhenNotContains()
        {
            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());

            Assert.DoesNotThrow(() =>
                SubComponentAttributeManager.RemoveDestroyMethod<AttributeTest>()
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RunDestroyMethods{T}(ISubComponent{T})"/>
        /// </summary>
        [Test]
        public void RunDestroyMethodsPasses()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.Destroy);

            Assert.IsFalse(SubComponentAttributeManager.ContainsDestroyMethod<AttributeTest>());
            SubComponentAttributeManager.AddDestroyMethod<AttributeTest>(AttributeTest.Method);

            AttributeTest.CallCounter = 0;
            AttributeTest.RecievedInstance = null;
            var inst = new SubCom();
            SubComponentAttributeManager.RunDestroyMethods(inst);

            Assert.AreSame(inst, AttributeTest.RecievedInstance);
            Assert.AreEqual(1, AttributeTest.CallCounter);
        }
        #endregion

        #region UpdateUI Methods
        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddUpdateUIMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.UpdateUI);

            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());

            SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddUpdateUIMethodFailWhenNull()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.UpdateUI);

            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(null)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod(System.Type, SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// <seealso cref="SubComponentAttributeManager.AddUpdateUIMethod{T}(SubComponentAttributeManager.AttributeMethodDelegate)"/>
        /// </summary>
        [Test]
        public void AddUpdateUIMethodFailWhenAlreadyContains()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.UpdateUI);
            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());

            SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(AttributeTest.Method);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(AttributeTest.Method)
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveUpdateUIMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveUpdateUIMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveUpdateUIMethodPass()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.UpdateUI);

            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());
            SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(AttributeTest.Method);
            Assert.IsTrue(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());

            SubComponentAttributeManager.RemoveUpdateUIMethod<AttributeTest>();
            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RemoveUpdateUIMethod(System.Type)"/>
        /// <seealso cref="SubComponentAttributeManager.RemoveUpdateUIMethod{T}"/>
        /// </summary>
        [Test]
        public void RemoveUpdateUIMethodFailWhenNotContains()
        {
            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());

            Assert.DoesNotThrow(() =>
                SubComponentAttributeManager.RemoveUpdateUIMethod<AttributeTest>()
            );
        }

        /// <summary>
        /// <seealso cref="SubComponentAttributeManager.RunUpdateUIMethods{T}(ISubComponent{T})"/>
        /// </summary>
        [Test]
        public void RunUpdateUIMethodsPasses()
        {
            ReserveRemoveMethod<AttributeTest>(Timing.UpdateUI);

            Assert.IsFalse(SubComponentAttributeManager.ContainsUpdateUIMethod<AttributeTest>());
            SubComponentAttributeManager.AddUpdateUIMethod<AttributeTest>(AttributeTest.Method);

            AttributeTest.CallCounter = 0;
            AttributeTest.RecievedInstance = null;

            var inst = new SubCom();
            SubComponentAttributeManager.RunUpdateUIMethods(inst);

            Assert.AreSame(inst, AttributeTest.RecievedInstance);
            Assert.AreEqual(1, AttributeTest.CallCounter);
        }
        #endregion

    }
}
