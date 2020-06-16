using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Serialization;

namespace Hinode.Tests.Input
{
    public class TestEventSystemFrameInputData : TestBase
    {
        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new EventSystemFrameInputData();
            data.CompositionString = "test";
            data.CompositionCursorPos = new Vector2(1f, 2f);
            data.IMECompositionMode = IMECompositionMode.On;
            data.MousePresent = true;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.MousePosition = new Vector2(333f, 444f);
            data.MouseScrollDelta = new Vector2(0.1f, 0.2f);
            data.TouchSupported = true;
            data.TouchCount = 2;
            data.SetTouch(0, new Touch { fingerId = 0, deltaTime = 0.5f, phase = TouchPhase.Ended });
            data.SetTouch(1, new Touch { fingerId = 1, deltaTime = 0.25f, phase = TouchPhase.Moved });

            var refCache = new RefCache(data.GetType());

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<EventSystemFrameInputData>(json);

            Assert.AreEqual(13, data.GetUpdatedValueEnumerable().Count());
            Assert.AreEqual(data.GetUpdatedValueEnumerable().Count(), dest.GetUpdatedValueEnumerable().Count());

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_compositionString")));
            Assert.AreEqual(data.CompositionString, dest.CompositionString);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_compositionCursorPos")));
            Assert.AreEqual(data.CompositionCursorPos, dest.CompositionCursorPos);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_imeCompositionMode")));
            Assert.AreEqual(data.IMECompositionMode, dest.IMECompositionMode);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_mousePresent")));
            Assert.AreEqual(data.MousePresent, dest.MousePresent);

            var mouseButtons = (UpdateObserver<InputDefines.ButtonCondition>[])refCache.GetField(data, "_mouseButtons");
            foreach (var btnType in System.Enum.GetValues(typeof(InputDefines.MouseButton))
                .OfType<InputDefines.MouseButton>())
            {
                Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == mouseButtons[(int)btnType]));
                Assert.AreEqual(data.GetMouseButton(btnType), dest.GetMouseButton(btnType));
            }

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_mousePosition")));
            Assert.AreEqual(data.MousePosition, dest.MousePosition);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_mouseScrollDelta")));
            Assert.AreEqual(data.MouseScrollDelta, dest.MouseScrollDelta);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_touchSupported")));
            Assert.AreEqual(data.TouchSupported, dest.TouchSupported);

            Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == refCache.GetField(data, "_touchCount")));
            Assert.AreEqual(data.TouchCount, dest.TouchCount);

            var touches = (TouchUpdateObserver[])refCache.GetField(data, "_touches");
            for (var i = 0; i < data.TouchCount; ++i)
            {
                Assert.IsTrue(data.GetUpdatedValueEnumerable().Any(_v => _v == touches[i]));
                Assert.AreEqual(data.GetTouch(i), dest.GetTouch(i));
            }
        }

        [UnityTest]
        public IEnumerator UpdatePasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var inputObj = new GameObject("__input", typeof(ReplayableBaseInput));
            var replayBaseInput = inputObj.GetComponent<ReplayableBaseInput>();
            yield return null;

            replayBaseInput.IsReplaying = true;
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

            //データが正しく設定されるか確認
            var data = new EventSystemFrameInputData();
            var frame = data.Update(replayBaseInput);
            Assert.AreEqual(replayBaseInput.recordedIMECompositionMode, data.IMECompositionMode);
            Assert.AreEqual(replayBaseInput.recordedCompositionString, data.CompositionString);
            Assert.AreEqual(replayBaseInput.recordedCompositionCursorPos, data.CompositionCursorPos);
            Assert.AreEqual(replayBaseInput.recordedMousePresent, data.MousePresent);
            Assert.AreEqual(replayBaseInput.recordedMousePosition, data.MousePosition);
            Assert.AreEqual(replayBaseInput.recordedMouseScrollDelta, data.MouseScrollDelta);
            foreach(InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                Assert.AreEqual(replayBaseInput.GetRecordedMouseButton(btn), data.GetMouseButton(btn));
            }
            Assert.AreEqual(replayBaseInput.recordedTouchSupported, data.TouchSupported);
            Assert.AreEqual(replayBaseInput.recordedTouchCount, data.TouchCount);
            for(var i=0; i< replayBaseInput.recordedTouchCount; ++i)
            {
                Assert.AreEqual(replayBaseInput.GetRecordedTouch(i), (Touch)data.GetTouch(i));
            }
        }

        [UnityTest]
        public IEnumerator RecoverFramePasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var inputObj = new GameObject("__input", typeof(ReplayableBaseInput));
            var replayBaseInput = inputObj.GetComponent<ReplayableBaseInput>();
            var data = new EventSystemFrameInputData();

            yield return null;

            data.IMECompositionMode = IMECompositionMode.Auto;
            data.CompositionString = "Test";
            data.CompositionCursorPos = Vector2.zero;
            data.MousePresent = true;
            data.MousePosition = Vector2.one * 100f;
            data.MouseScrollDelta = Vector2.one * 2f;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);
            data.TouchSupported = true;
            data.TouchCount = 2;
            data.SetTouch(0, new Touch { fingerId = 11 });
            data.SetTouch(1, new Touch { fingerId = 22 });

            var frameData = new InputRecord.Frame();
            var serializer = new JsonSerializer();
            frameData.InputText = serializer.Serialize(data);

            //データが正しく設定されるか確認
            data.RecoverFrame(replayBaseInput, frameData);
            Assert.AreEqual(data.IMECompositionMode, replayBaseInput.recordedIMECompositionMode);
            Assert.AreEqual(data.CompositionString, replayBaseInput.recordedCompositionString);
            Assert.AreEqual(data.CompositionCursorPos, replayBaseInput.recordedCompositionCursorPos);
            Assert.AreEqual(data.MousePresent, replayBaseInput.recordedMousePresent);
            Assert.AreEqual(data.MousePosition, replayBaseInput.recordedMousePosition);
            Assert.AreEqual(data.MouseScrollDelta, replayBaseInput.recordedMouseScrollDelta);
            foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                Assert.AreEqual(data.GetMouseButton(btn), replayBaseInput.GetRecordedMouseButton(btn));
            }
            Assert.AreEqual(data.TouchSupported, replayBaseInput.recordedTouchSupported);
            Assert.AreEqual(data.TouchCount, replayBaseInput.recordedTouchCount);
            for (var i = 0; i < replayBaseInput.recordedTouchCount; ++i)
            {
                Assert.AreEqual((Touch)data.GetTouch(i), replayBaseInput.GetRecordedTouch(i), $"not equal index={i}...");
            }
        }

    }
}
