using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="ReplayableBaseInput"/>
    /// </summary>
    public class TestReplayableBaseInput
    {

        [UnityTest]
        public IEnumerator BasicReplayUsagePasses()
        {
            var inputObj = new GameObject("__input", typeof(ReplayableBaseInput));
            yield return null;

            var replayBaseInput = inputObj.GetComponent<ReplayableBaseInput>();

            replayBaseInput.recordedIMECompositionMode = IMECompositionMode.Auto;
            replayBaseInput.recordedCompositionString = "Test";
            replayBaseInput.recordedCompositionCursorPos = Vector2.zero;

            replayBaseInput.recordedMousePresent = true;
            replayBaseInput.recordedMousePosition = Vector2.one * 100f;
            replayBaseInput.recordedMouseScrollDelta = Vector2.one * 2f;
            replayBaseInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            replayBaseInput.SetRecordedMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            replayBaseInput.SetRecordedMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            replayBaseInput.recordedTouchSupported = true;
            replayBaseInput.recordedTouchCount = 2;
            replayBaseInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayBaseInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //入力データの上書きができているかの確認
            replayBaseInput.IsReplaying = true;
            Assert.AreEqual(replayBaseInput.recordedIMECompositionMode, replayBaseInput.imeCompositionMode);
            Assert.AreEqual(replayBaseInput.recordedCompositionString, replayBaseInput.recordedCompositionString);
            Assert.AreEqual(replayBaseInput.recordedCompositionCursorPos, replayBaseInput.recordedCompositionCursorPos);

            Assert.AreEqual(replayBaseInput.recordedMousePresent, replayBaseInput.recordedMousePresent);
            Assert.AreEqual(replayBaseInput.recordedMousePosition, replayBaseInput.recordedMousePosition);
            Assert.AreEqual(replayBaseInput.recordedMouseScrollDelta, replayBaseInput.recordedMouseScrollDelta);
            Assert.IsTrue(replayBaseInput.GetMouseButton((int)InputDefines.MouseButton.Left));
            Assert.IsTrue(replayBaseInput.GetMouseButtonUp((int)InputDefines.MouseButton.Middle));
            Assert.IsTrue(replayBaseInput.GetMouseButtonDown((int)InputDefines.MouseButton.Right));

            Assert.AreEqual(replayBaseInput.recordedTouchSupported, replayBaseInput.recordedTouchSupported);
            Assert.AreEqual(replayBaseInput.recordedTouchCount, replayBaseInput.recordedTouchCount);
            Assert.AreEqual(replayBaseInput.GetRecordedTouch(0), replayBaseInput.GetTouch(0));
            Assert.AreEqual(replayBaseInput.GetRecordedTouch(1), replayBaseInput.GetTouch(1));
        }
    }
}
