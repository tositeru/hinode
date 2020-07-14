using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Layouts;

namespace Hinode.Tests.Layouts
{
    /// <summary>
	/// <seealso cref="ILayoutTarget"/>
	/// <seealso cref="LayoutTargetBase"/>
	/// </summary>
    public class TestLayoutTarget
    {
        class LayoutTargetTest : LayoutTargetBase
        {
            public override ILayoutTarget Parent { get; }
            public override IEnumerable<ILayoutTarget> Childrens { get; }

            public override Vector3 Pos { get; set; }
            public override Vector3 Size { get; set; }

            public override Vector3 AnchorMin { get; set; }
            public override Vector3 AnchorMax { get; set; }

            public override Vector3 AnchorOffsetMin { get; set; }
            public override Vector3 AnchorOffsetMax { get; set; }
        }

        /// <summary>
		/// <seealso cref="LayoutTargetBase.OnDisposed(ILayoutTarget self)"/>
		/// </summary>
        [Test]
        public void OnDisposedPasses()
        {
            var layout = new LayoutTargetTest();
            var callCounter = 0;
            ILayoutTarget recievedSelf = null;
            layout.OnDisposed.Add((_self) => { callCounter++; recievedSelf = _self; });

            layout.Dispose();
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(layout, recievedSelf);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetBase.OnDisposed(ILayoutTarget self)"/>
        /// </summary>
        [Test]
        public void OnDisposedWhenAfterDisposePasses()
        {
            var layout = new LayoutTargetTest();
            var callCounter = 0;
            ILayoutTarget recievedSelf = null;
            layout.OnDisposed.Add((_self) => { callCounter++; recievedSelf = _self; });
            layout.Dispose();

            callCounter = 0;
            recievedSelf = null;
            layout.Dispose();
            Assert.AreEqual(0, callCounter);
            Assert.IsNull(recievedSelf);
        }
    }
}
