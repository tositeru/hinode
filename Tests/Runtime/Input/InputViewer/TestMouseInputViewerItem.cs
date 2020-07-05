using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="MouseInputViewerItem"/>
	/// </summary>
    public class TestMouseInputViewerItem : TestBase
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
            yield return null;

            inputViewer.UseInput.RecordedMousePos = Vector3.one * 10f;
            Assert.IsTrue(inputViewer.RootCanvas.transform.GetChildEnumerable().Any(_c => _c == mouse.Cursor.transform));
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
            yield return null;

            Assert.IsTrue(inputViewer.TextArea.transform.GetChildEnumerable().Any(_c => _c == mouse.ButtonsText.transform));
        }

        /// <summary>
		/// <seealso cref="MouseInputViewerItem.OnChangedStyle(InputViewerStyleInfo)"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator OnChangedStylePasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();
            inputViewer.UseInput.RecordedMousePresent = true;
            yield return null;

            inputViewer.StyleInfo.Font = new Font();
            inputViewer.StyleInfo.FontColor = Color.green;

            Assert.AreSame(inputViewer.StyleInfo.Font, mouse.ButtonsText.font);
            Assert.AreEqual(inputViewer.StyleInfo.FontColor, mouse.ButtonsText.color);
        }

        [UnityTest]
        public IEnumerator CursorRadiusPasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();
            inputViewer.UseInput.RecordedMousePresent = true;

            mouse.CursorRadius = 20;

            var mouseR = mouse.Cursor.transform as RectTransform;
            Assert.AreEqual(mouse.CursorRadius, mouseR.rect.width);
            Assert.AreEqual(mouse.CursorRadius, mouseR.rect.height);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CursorColorPasses()
        {
            var (inputViewer, mouse) = CreateMouseItem();
            inputViewer.UseInput.RecordedMousePresent = true;

            inputViewer.StyleInfo.ButtonColorAtFree = Color.red;
            inputViewer.StyleInfo.ButtonColorAtDown = Color.blue;
            inputViewer.StyleInfo.ButtonColorAtPush = Color.green;
            inputViewer.StyleInfo.ButtonColorAtUp = Color.yellow;

            inputViewer.UseInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
            yield return null;
            Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtFree, mouse.Cursor.color);
            Debug.Log($"Success to Set Cursor Color at Free!");

            inputViewer.UseInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
            yield return null;
            Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtDown, mouse.Cursor.color);
            Debug.Log($"Success to Set Cursor Color at Down!");

            inputViewer.UseInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            yield return null;
            Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtPush, mouse.Cursor.color);
            Debug.Log($"Success to Set Cursor Color at Push!");

            inputViewer.UseInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Up);
            yield return null;
            Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtUp, mouse.Cursor.color);
            Debug.Log($"Success to Set Cursor Color at Up!");

        }

    }
}
