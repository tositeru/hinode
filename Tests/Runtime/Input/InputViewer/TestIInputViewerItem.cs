using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="IInputViewerItem"/>
	/// </summary>
    public class TestIInputViewerItem : TestBase
    {
        class DummyInputViewerItem : IInputViewerItem
        {
            public new InputViewer UseInputViewer { get; private set; }

            public override void OnInitItem(InputViewer inputViewer)
            {
                UseInputViewer = inputViewer;
            }

            public override void OnRemoveFromViewer(InputViewer inputViewer)
            {
                UseInputViewer = null;
            }

            public override void OnUpdateItem()
            {

            }

            public override void OnChangedStyle(InputViewerStyleInfo styleInfo)
            { }
        }

        /// <summary>
		/// <seealso cref="IInputViewerItem.Start()"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator AutoAddToInputViewerPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var dummyItem = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();

            Assert.IsFalse(inputViewer.ViewerItems.Contains(dummyItem), $"Auto Addition must add on IInputViewerItem#Start()...");
            yield return null; // <- Call IInputViewerItem#Start()

            Assert.AreSame(inputViewer, dummyItem.UseInputViewer);
            Assert.IsTrue(inputViewer.ViewerItems.Contains(dummyItem), $"IInputViewerItem must add automatic to InputViewer on IInputViewerItem#Start()...");
        }

        /// <summary>
		/// <seealso cref="IInputViewerItem.OnDestroy()"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator AutoRemoveFromInputViewerPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var dummyItem = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();
            yield return null; // <- Call IInputViewerItem#Start()

            Object.Destroy(dummyItem);
            yield return null; // <- Call IInputViewerItem#Destroy()

            Assert.IsNull(dummyItem.UseInputViewer);
            Assert.IsFalse(inputViewer.ViewerItems.Contains(dummyItem), $"IInputViewerItem must remove automatic from InputViewer on IInputViewerItem#Destroy()...");
        }
    }
}
