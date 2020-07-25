using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Layouts;

namespace Hinode.Tests.Layouts
{
    /// <summary>
	/// <seealso cref="ILayout"/>
	/// <seealso cref="LayoutBase"/>
	/// </summary>
    public class TestILayout
    {
        class TestLayout : LayoutBase
        {
            public override bool DoChanged { get; }
            public override Vector3 UnitSize { get; }

            public override void UpdateUnitSize()
            { }

            public override void UpdateLayout()
            { }
        }

        /// <summary>
		/// <seealso cref="LayoutBase.OnDisposed(ILayout self)"/>
		/// </summary>
        [Test]
        public void OnDisposedPasses()
        {
            var layout = new TestLayout();
            var callCounter = 0;
            ILayout recievedSelf = null;
            layout.OnDisposed.Add((_self) => { callCounter++; recievedSelf = _self; });

            layout.Dispose();
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(layout, recievedSelf);

        }

        /// <summary>
        /// <seealso cref="LayoutBase.OnDisposed(ILayout self)"/>
        /// </summary>
        [Test]
        public void OnDisposedWhenAfterDisposePasses()
        {
            var layout = new TestLayout();
            var callCounter = 0;
            ILayout recievedSelf = null;
            layout.OnDisposed.Add((_self) => { callCounter++; recievedSelf = _self; });
            layout.Dispose();

            callCounter = 0;
            recievedSelf = null;
            layout.Dispose();
            Assert.AreEqual(0, callCounter);
            Assert.IsNull(recievedSelf);
        }

        /// <summary>
		/// <seealso cref="ILayout.Target"/>
        /// <seealso cref="ILayoutTarget.OnDisposed(ILayoutTarget self)"/>
        /// </summary>
        [Test]
        public void LayoutTargetOnDisposedPasses()
        {
            var layout = new TestLayout();
            var target = new LayoutTargetObject();

            layout.Target = target;

            target.Dispose();

            Assert.IsNull(layout.Target);
            Assert.AreEqual(0, target.OnDisposed.RegistedDelegateCount);
        }

        /// <summary>
        /// <seealso cref="ILayout.Target"/>
        /// <seealso cref="ILayoutTarget.OnDisposed(ILayoutTarget self)"/>
        /// </summary>
        [Test]
        public void LayoutTargetOnDisposedWhenReassignPasses()
        {
            var layout = new TestLayout();
            var prevTarget = new LayoutTargetObject();
            var curTarget = new LayoutTargetObject();

            layout.Target = prevTarget;

            {
                layout.Target = curTarget;
                Assert.AreEqual(0, prevTarget.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(1, curTarget.OnDisposed.RegistedDelegateCount);
            }
            Debug.Log($"Success to Assign override!");

            {
                prevTarget.Dispose();
                Assert.AreSame(curTarget, layout.Target);
                Assert.AreEqual(1, curTarget.OnDisposed.RegistedDelegateCount);
            }
            Debug.Log($"Success to Dispose prevTarget!");

            {
                curTarget.Dispose();
                Assert.IsNull(layout.Target);
                Assert.AreEqual(0, curTarget.OnDisposed.RegistedDelegateCount);
            }
            Debug.Log($"Success to Dispose curTarget");
        }

        /// <summary>
		/// <seealso cref="ILayout.Target"/>
		/// <seealso cref="ILayoutExtensions.ContainsTarget(this ILayout layout)"/>
		/// </summary>
        [Test]
        public void ContainsTargetPasses()
        {
            var layout = new TestLayout();
            Assert.IsFalse(layout.ContainsTarget());

            var target = new LayoutTargetObject();

            layout.Target = target;
            Assert.IsTrue(layout.ContainsTarget());

            layout.Target = null;
            Assert.IsFalse(layout.ContainsTarget());
        }
    }
}
