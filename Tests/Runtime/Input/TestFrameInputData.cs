using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input.FrameInputData
{
    /// <summary>
    /// <seealso cref="FrameInputData"/>
    /// </summary>
    public class TestFrameInputData
    {
        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new Hinode.FrameInputData();
            //Mouse
            data.MousePresent = true;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.MousePosition = new Vector2(333f, 444f);
            data.MouseScrollDelta = new Vector2(0.1f, 0.2f);
            //Touch
            data.TouchSupported = true;
            data.TouchCount = 2;
            data.SetTouch(0, new Touch { fingerId = 0, deltaTime = 0.5f, phase = TouchPhase.Ended });
            data.SetTouch(1, new Touch { fingerId = 1, deltaTime = 0.25f, phase = TouchPhase.Moved });

            var refCache = new RefCache(data.GetType());

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<Hinode.FrameInputData>(json);

            Assert.AreEqual(10, data.GetUpdatedValueEnumerable().Count());
            Assert.AreEqual(data.GetUpdatedValueEnumerable().Count(), dest.GetUpdatedValueEnumerable().Count());

            //Mouse
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

            //Touch
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
            var replayInput = ReplayableInput.Instance;
            yield return null;

            replayInput.IsReplaying = true;
            //Mouse
            replayInput.RecordedMousePresent = true;
            replayInput.RecordedMousePos = Vector2.one * 100f;
            replayInput.RecordedMouseScrollDelta = Vector2.one * 2f;
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);
            //Touch
            replayInput.RecordedTouchSupported = true;
            replayInput.RecordedTouchCount = 2;
            replayInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //データが正しく設定されるか確認
            var data = new Hinode.FrameInputData();
            var frame = data.Update(replayInput);
            //Mouse
            Assert.AreEqual(replayInput.RecordedMousePresent, data.MousePresent);
            Assert.AreEqual(replayInput.RecordedMousePos, data.MousePosition);
            Assert.AreEqual(replayInput.RecordedMouseScrollDelta, data.MouseScrollDelta);
            foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                Assert.AreEqual(replayInput.GetRecordedMouseButton(btn), data.GetMouseButton(btn));
            }
            //Touch
            Assert.AreEqual(replayInput.RecordedTouchSupported, data.TouchSupported);
            Assert.AreEqual(replayInput.RecordedTouchCount, data.TouchCount);
            for (var i = 0; i < replayInput.RecordedTouchCount; ++i)
            {
                Assert.AreEqual(replayInput.GetRecordedTouch(i), (Touch)data.GetTouch(i));
            }
        }

        [UnityTest]
        public IEnumerator RecoverFramePasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            var data = new Hinode.FrameInputData();

            yield return null;

            //Mouse
            data.MousePresent = true;
            data.MousePosition = Vector2.one * 100f;
            data.MouseScrollDelta = Vector2.one * 2f;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            //Touch
            data.TouchSupported = true;
            data.TouchCount = 2;
            data.SetTouch(0, new Touch { fingerId = 11 });
            data.SetTouch(1, new Touch { fingerId = 22 });

            var frameData = new InputRecord.Frame();
            var serializer = new JsonSerializer();
            frameData.InputText = serializer.Serialize(data);

            //データが正しく設定されるか確認
            data.RecoverFrame(replayInput, frameData);
            //Mouse
            Assert.AreEqual(data.MousePresent, replayInput.RecordedMousePresent);
            Assert.AreEqual(data.MousePosition, replayInput.RecordedMousePos);
            Assert.AreEqual(data.MouseScrollDelta, replayInput.RecordedMouseScrollDelta);
            foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                Assert.AreEqual(data.GetMouseButton(btn), replayInput.GetRecordedMouseButton(btn));
            }

            //Touch
            Assert.AreEqual(data.TouchSupported, replayInput.RecordedTouchSupported);
            Assert.AreEqual(data.TouchCount, replayInput.RecordedTouchCount);
            for (var i = 0; i < replayInput.RecordedTouchCount; ++i)
            {
                Assert.AreEqual((Touch)data.GetTouch(i), replayInput.GetRecordedTouch(i), $"not equal index={i}...");
            }
        }

        [Test]
        public void SerializationKeyButtonPasses()
        {
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator UpdateKeyButtonPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator RecoverFrameKeyButtonPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [Test]
        public void SerializationButtonPasses()
        {
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator UpdateButtonPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator RecoverFrameButtonPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [Test]
        public void SerializationAxisPasses()
        {
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator UpdateAxisPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator RecoverFrameAxisPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

    }
}
