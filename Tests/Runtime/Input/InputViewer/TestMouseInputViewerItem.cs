using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
	/// <seealso cref="MouseInputViewerItem"/>
	/// </summary>
    public class TestMouseInputViewerItem
    {
        (InputViewer, MouseInputViewerItem) CreateMouseItem()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var mouse = inputViewer.gameObject.AddComponent<MouseInputViewerItem>();
            inputViewer.RefleshItems();
            return (inputViewer, mouse);
        }

        /// <summary>
		/// <seealso cref="MouseInputViewerItem.DoEnabled"/>
		/// <seealso cref="MouseInputViewerItem.Cursor"/>
		/// <seealso cref="MouseInputViewerItem.ButtonsText"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator DoEnabledPasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();

            inputViewer.UseInput.RecordedMousePresent = true;
            yield return null;
            Assert.IsTrue(mouse.DoEnabled);

            Assert.IsTrue(mouse.Cursor.gameObject.activeInHierarchy);
            Assert.IsTrue(mouse.ButtonsText.gameObject.activeInHierarchy);


            inputViewer.UseInput.RecordedMousePresent = false;
            yield return null;
            Assert.IsFalse(mouse.DoEnabled);

            Assert.IsFalse(mouse.Cursor.gameObject.activeInHierarchy);
            Assert.IsFalse(mouse.ButtonsText.gameObject.activeInHierarchy);
        }

        /// <summary>
		/// <seealso cref="MouseInputViewerItem.Cursor"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator CursorPasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();
            inputViewer.UseInput.RecordedMousePresent = true;

            inputViewer.UseInput.RecordedMousePos = Vector3.one * 10f;

            Assert.IsTrue(inputViewer.RootCanvas.transform.GetChildEnumerable().Any(_c => _c == mouse.Cursor));
            yield return null;
        }

        /// <summary>
		/// <seealso cref="MouseInputViewerItem.ButtonsText"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator ButtonPasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();
            inputViewer.UseInput.RecordedMousePresent = true;

            Assert.IsTrue(inputViewer.TextArea.transform.GetChildEnumerable().Any(_c => _c == mouse.ButtonsText));
            yield return null;
        }
    }
}
