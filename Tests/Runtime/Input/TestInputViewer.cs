using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <see cref="InputViewer"/>
    /// <seealso cref="ReplayableBaseInput"/>
    /// </summary>
    public class TestInputViewer
    {
        [UnityTest, Description("マウスとタッチ位置が正確に反映されているかのテスト")]
        public IEnumerator CursorPositionPasses()
        {
            var inputViewer = InputViewer.Instance;
            var replayableInput = inputViewer.gameObject.AddComponent<ReplayableBaseInput>();
            inputViewer.Target = replayableInput;

            yield return null;

            replayableInput.IsReplaying = true;
            replayableInput.recordedMousePresent = true;
            replayableInput.recordedTouchSupported = true;
            var from = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var to = new Vector2(Screen.width, Screen.height);
            var frameCount = 10f;
            var t = 0f;
            while(t <= 1f)
            {
                var pos = Vector2.Lerp(from, to, t);
                t += 1f / frameCount;
                replayableInput.recordedMousePosition = pos;
                replayableInput.recordedTouchCount = 2;
                replayableInput.SetRecordedTouch(0, new Touch()
                {
                    fingerId = 0,
                    position = pos + Vector2.up * 20,
                });
                replayableInput.SetRecordedTouch(0, new Touch()
                {
                    fingerId = 1,
                    position = pos + Vector2.down * 20,
                });
                yield return null;

                Assert.AreEqual(replayableInput.recordedMousePosition, inputViewer.MouseCursor.rectTransform.anchoredPosition);
                Assert.IsTrue(inputViewer.MouseCursor.gameObject.activeInHierarchy);

                Assert.AreEqual(replayableInput.GetRecordedTouch(0).position, inputViewer.GetTouchCursor(0).rectTransform.anchoredPosition);
                Assert.AreEqual(replayableInput.GetRecordedTouch(1).position, inputViewer.GetTouchCursor(1).rectTransform.anchoredPosition);
                Assert.IsTrue(inputViewer.GetTouchCursor(0).gameObject.activeInHierarchy);
                Assert.IsTrue(inputViewer.GetTouchCursor(1).gameObject.activeInHierarchy);
            }

            //Touch系は表示されない状態もあるのでそれも確認している
            replayableInput.recordedTouchCount = 0;
            yield return null;
            Assert.IsFalse(inputViewer.GetTouchCursor(0).gameObject.activeInHierarchy);
            Assert.IsFalse(inputViewer.GetTouchCursor(1).gameObject.activeInHierarchy);
        }

        [Ignore("yet not implement..."), UnityTest, Description("ボタンの入力情報のテスト")]
        public IEnumerator ButtonInfoPasses()
        {
            yield return null;
            throw new System.NotImplementedException("yet not implement...");
        }
    }
}
