﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
    /// <see cref="InputViewer"/>
    /// <seealso cref="ReplayableBaseInput"/>
    /// </summary>
    public class TestInputViewer : TestBase
    {
        /// <summary>
		/// <seealso cref="InputViewer.CreateInstance()"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInstancePasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            var scene = SceneManager.GetActiveScene();
            Assert.IsTrue(scene.GetRootGameObjects().Any(_o => _o.TryGetComponent<InputViewer>(out var _)));

            yield return null;
        }

        /// <summary>
		/// <seealso cref="InputViewer.RootCanvas"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator RootCanvasPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            yield return null;
            Assert.IsNotNull(inputViewer.RootCanvas);
            Assert.IsNull(inputViewer.RootCanvas.transform.parent);
        }

        /// <summary>
        /// <seealso cref="InputViewer.TextArea"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TextAreaPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            yield return null;
            Assert.IsNotNull(inputViewer.TextArea);
            Assert.AreSame(inputViewer.RootCanvas.transform, inputViewer.TextArea.transform.parent);
        }

        class DummyInputViewerItem : IInputViewerItem
        {
            public new InputViewer UseInputViewer { get; private set; }
            public int OnChangedStyleCallCounter { get; set; } = 0;

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
            {
                OnChangedStyleCallCounter++;
            }

        }

        /// <summary>
        /// <seealso cref="InputViewer.ViewerItems"/>
        /// <seealso cref="InputViewer.AddItem()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddItemPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var item = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();

            inputViewer.AddItem(item);
            Assert.IsTrue(inputViewer.ViewerItems.Contains(item));
            yield return null;
        }

        /// <summary>
        /// <seealso cref="InputViewer.ViewerItems"/>
        /// <seealso cref="InputViewer.AddItem()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddItemWhenAlreadyAddedPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var item = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();
            inputViewer.AddItem(item);

            inputViewer.AddItem(item); // <- Test point. InputViewer#AddItem will do nothing.

            Assert.AreEqual(1, inputViewer.ViewerItems.Where(_i => _i == item).Count());
            yield return null;
        }

        /// <summary>
        /// <seealso cref="InputViewer.ViewerItems"/>
        /// <seealso cref="InputViewer.RemoveItem(IInputViewerItem)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RemoveItemPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var item = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();
            inputViewer.AddItem(item);

            inputViewer.RemoveItem(item); // <- Test point.
            Assert.IsFalse(inputViewer.ViewerItems.Contains(item));
            yield return null;
        }

        /// <summary>
        /// <seealso cref="InputViewer.ViewerItems"/>
        /// <seealso cref="InputViewer.RemoveItem(IInputViewerItem)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RemoveItemWhenNotExistPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var item = inputViewer.gameObject.AddComponent<DummyInputViewerItem>();

            inputViewer.RemoveItem(item); // <- Test point.
            Assert.IsFalse(inputViewer.ViewerItems.Contains(item));
            yield return null;
        }

        /// <summary>
		/// <seealso cref="InputViewer.ViewerItems"/>
		/// <seealso cref="InputViewer.RefleshItems()"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator RefleshItemsPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var items = new IInputViewerItem[]
            {
                inputViewer.gameObject.AddComponent<DummyInputViewerItem>(),
                inputViewer.gameObject.AddComponent<DummyInputViewerItem>(),
            };

            inputViewer.RefleshItems();
            AssertionUtils.AssertEnumerableByUnordered(
                items
                , inputViewer.ViewerItems
                , ""
            );

            Assert.IsTrue(inputViewer.ViewerItems.OfType<DummyInputViewerItem>().All(_d => _d.UseInputViewer == inputViewer));
            yield return null;
        }

        [UnityTest]
        public IEnumerator OnChangedStyleInfoPasses()
        {
            var inputViewer = InputViewer.CreateInstance();
            Assert.IsFalse(inputViewer.ViewerItems.Any());

            var items = new IInputViewerItem[]
            {
                inputViewer.gameObject.AddComponent<DummyInputViewerItem>(),
                inputViewer.gameObject.AddComponent<DummyInputViewerItem>(),
            };

            yield return null;

            foreach (var child in inputViewer.ViewerItems.OfType<DummyInputViewerItem>())
            {
                Assert.AreEqual(1, child.OnChangedStyleCallCounter);
            }
            Debug.Log($"Success to IInputViewerItem#InitItem()!");

            inputViewer.StyleInfo.Font = new Font();
            foreach(var child in inputViewer.ViewerItems.OfType<DummyInputViewerItem>())
            {
                Assert.AreEqual(2, child.OnChangedStyleCallCounter);
            }
            Debug.Log($"Success to Change InputViewerStyleInfo's property!");
        }

        // will delete below

        //[UnityTest, Description("マウスとタッチ位置が正確に反映されているかのテスト")]
        //public IEnumerator CursorPositionPasses()
        //{
        //    var inputViewer = InputViewer.Instance;
        //    var replayableInput = inputViewer.gameObject.AddComponent<ReplayableBaseInput>();
        //    inputViewer.Target = replayableInput;

        //    yield return null;

        //    replayableInput.IsReplaying = true;
        //    replayableInput.recordedMousePresent = true;
        //    replayableInput.recordedTouchSupported = true;
        //    var from = new Vector2(Screen.width / 2f, Screen.height / 2f);
        //    var to = new Vector2(Screen.width, Screen.height);
        //    var frameCount = 10f;
        //    var t = 0f;
        //    while(t <= 1f)
        //    {
        //        var pos = Vector2.Lerp(from, to, t);
        //        t += 1f / frameCount;
        //        replayableInput.recordedMousePosition = pos;
        //        replayableInput.recordedTouchCount = 2;
        //        replayableInput.SetRecordedTouch(0, new Touch()
        //        {
        //            fingerId = 0,
        //            position = pos + Vector2.up * 20,
        //        });
        //        replayableInput.SetRecordedTouch(0, new Touch()
        //        {
        //            fingerId = 1,
        //            position = pos + Vector2.down * 20,
        //        });
        //        yield return null;

        //        Assert.AreEqual(replayableInput.recordedMousePosition, inputViewer.MouseCursor.rectTransform.anchoredPosition);
        //        Assert.IsTrue(inputViewer.MouseCursor.gameObject.activeInHierarchy);

        //        Assert.AreEqual(replayableInput.GetRecordedTouch(0).position, inputViewer.GetTouchCursor(0).rectTransform.anchoredPosition);
        //        Assert.AreEqual(replayableInput.GetRecordedTouch(1).position, inputViewer.GetTouchCursor(1).rectTransform.anchoredPosition);
        //        Assert.IsTrue(inputViewer.GetTouchCursor(0).gameObject.activeInHierarchy);
        //        Assert.IsTrue(inputViewer.GetTouchCursor(1).gameObject.activeInHierarchy);
        //    }

        //    //Touch系は表示されない状態もあるのでそれも確認している
        //    replayableInput.recordedTouchCount = 0;
        //    yield return null;
        //    Assert.IsFalse(inputViewer.GetTouchCursor(0).gameObject.activeInHierarchy);
        //    Assert.IsFalse(inputViewer.GetTouchCursor(1).gameObject.activeInHierarchy);
        //}

        //[Ignore("yet not implement..."), UnityTest, Description("ボタンの入力情報のテスト")]
        //public IEnumerator ButtonInfoPasses()
        //{
        //    yield return null;
        //    throw new System.NotImplementedException("yet not implement...");
        //}
    }
}
