using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input.FrameInputDataRecorder
{
    /// <summary>
    /// <seealso cref="MouseFrameInputData"/>
    /// </summary>
    public class TestMouseFrameInputData
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.MousePresent"/>
        /// <seealso cref="MouseFrameInputData.MousePosition"/>
        /// <seealso cref="MouseFrameInputData.MouseScrollDelta"/>
        /// <seealso cref="MouseFrameInputData.SetMouseButton(InputDefines.MouseButton, InputDefines.ButtonCondition)"/>
        /// <seealso cref="MouseFrameInputData.GetMouseButton(InputDefines.MouseButton)"/>
        /// </summary>
        [Test]
        public void PropertiesPasses()
        {
            var data = new MouseFrameInputData();

            {//Mouse Present
                data.MousePresent = !data.MousePresent;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == MouseFrameInputData.KeyMousePresent)
                    .Value;
                Assert.AreEqual(data.MousePresent, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to MousePresent!");

            {//Mouse Position
                data.MousePosition = new Vector2(333f, 444f);
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == MouseFrameInputData.KeyMousePosition)
                    .Value;
                Assert.AreEqual(data.MousePosition, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to MousePosition!");

            {//Mouse Position
                data.MouseScrollDelta = new Vector2(0.1f, 0.2f);
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == MouseFrameInputData.KeyMouseScrollDelta)
                    .Value;
                Assert.AreEqual(data.MouseScrollDelta, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to MouseScrollDelta!");

            foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
            {
                data.SetMouseButton(btn, InputDefines.ButtonCondition.Push);
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == ((int)btn).ToString())
                    .Value;
                Assert.AreEqual(data.GetMouseButton(btn), observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
                Debug.Log($"Success to MouseButton({btn}!");
            }
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test]
        public void GetValuesEnumerablePasses()
        {
            var data = new MouseFrameInputData();
            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[]
                {
                    (MouseFrameInputData.KeyMousePresent, data.MousePresent),
                    (MouseFrameInputData.KeyMousePosition, data.MousePosition),
                    (MouseFrameInputData.KeyMouseScrollDelta, data.MouseScrollDelta),
                    (((int)InputDefines.MouseButton.Left).ToString(), data.GetMouseButton(InputDefines.MouseButton.Left)),
                    (((int)InputDefines.MouseButton.Right).ToString(), data.GetMouseButton(InputDefines.MouseButton.Right)),
                    (((int)InputDefines.MouseButton.Middle).ToString(), data.GetMouseButton(InputDefines.MouseButton.Middle)),
                },
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="MouseFrameInputData.GetKeyType()"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var data = new MouseFrameInputData();

            var testData = new (string key, System.Type type)[]
            {
                (MouseFrameInputData.KeyMousePresent, typeof(bool)),
                (MouseFrameInputData.KeyMousePosition, typeof(string)),
                (MouseFrameInputData.KeyMouseScrollDelta, typeof(string)),
            }.Concat(System.Enum.GetValues(typeof(InputDefines.MouseButton))
                .OfType<InputDefines.MouseButton>()
                .Select(_b => (key: ((int)_b).ToString(), type: typeof(InputDefines.ButtonCondition)))
            );
            foreach (var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}";
                Assert.AreEqual(d.type, MouseFrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Mouse
            replayInput.RecordedMousePresent = true;
            replayInput.RecordedMousePos = Vector2.one * 100f;
            replayInput.RecordedMouseScrollDelta = Vector2.one * 2f;
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            //データが正しく設定されるか確認
            var data = new MouseFrameInputData();
            data.Record(replayInput);

            Assert.AreEqual(replayInput.RecordedMousePresent, data.MousePresent);
            Assert.AreEqual(replayInput.RecordedMousePos, data.MousePosition);
            Assert.AreEqual(replayInput.RecordedMouseScrollDelta, data.MouseScrollDelta);
            foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                Assert.AreEqual(replayInput.GetRecordedMouseButton(btn), data.GetMouseButton(btn));
            }
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="MouseFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="MouseFrameInputData.MouseFrameInputData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// <seealso cref="MouseFrameInputData.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new MouseFrameInputData();
            //Mouse
            data.MousePresent = true;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.MousePosition = new Vector2(333f, 444f);
            data.MouseScrollDelta = new Vector2(0.1f, 0.2f);

            var refCache = new RefCache(data.GetType());

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<MouseFrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(data.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                Assert.AreEqual(data.MousePresent, dest.MousePresent);
                Assert.AreEqual(data.MousePosition, dest.MousePosition);
                Assert.AreEqual(data.MouseScrollDelta, dest.MouseScrollDelta);

                var mouseButtons = (UpdateObserver<InputDefines.ButtonCondition>[])refCache.GetField(data, "_mouseButtons");
                foreach (var btnType in System.Enum.GetValues(typeof(InputDefines.MouseButton))
                    .OfType<InputDefines.MouseButton>())
                {
                    Assert.AreEqual(data.GetMouseButton(btnType), dest.GetMouseButton(btnType), $"Failed to serialize Mouse Button({btnType})...");
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            var data = new MouseFrameInputData();
            //Mouse
            data.MousePresent = true;
            data.MousePosition = Vector2.one * 100f;
            data.MouseScrollDelta = Vector2.one * 2f;
            data.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            data.SetMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            data.SetMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            //データが正しく設定されるか確認
            data.RecoverTo(replayInput);

            {
                var errorMessage = "Failed to Set Input Datas to ReplayableInput...";
                Assert.AreEqual(data.MousePresent, replayInput.RecordedMousePresent, errorMessage);
                Assert.AreEqual(data.MousePosition, replayInput.RecordedMousePos, errorMessage);
                Assert.AreEqual(data.MouseScrollDelta, replayInput.RecordedMouseScrollDelta, errorMessage);
                foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
                {
                    Assert.AreEqual(data.GetMouseButton(btn), replayInput.GetRecordedMouseButton(btn), errorMessage);
                }
            }
            Debug.Log($"Success to Set Input Datas to ReplayableInput!");
        }

        /// <summary>
        /// <see cref="MouseFrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Mouse
            replayInput.RecordedMousePresent = true;
            replayInput.RecordedMousePos = Vector2.one * 100f;
            replayInput.RecordedMouseScrollDelta = Vector2.one * 2f;
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            //データが正しく設定されるか確認
            var data = new MouseFrameInputData();
            data.Record(replayInput);

            data.ResetDatas(); // <- Test run here

            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[]
                {
                    (MouseFrameInputData.KeyMousePresent, default(bool)),
                    (MouseFrameInputData.KeyMousePosition, default(Vector3)),
                    (MouseFrameInputData.KeyMouseScrollDelta, default(Vector2)),
                    (((int)InputDefines.MouseButton.Left).ToString(), default(InputDefines.ButtonCondition)),
                    (((int)InputDefines.MouseButton.Right).ToString(), default(InputDefines.ButtonCondition)),
                    (((int)InputDefines.MouseButton.Middle).ToString(), default(InputDefines.ButtonCondition)),
                },
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <see cref="MouseFrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleshUpdatedFlagsPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Mouse
            replayInput.RecordedMousePresent = true;
            replayInput.RecordedMousePos = Vector2.one * 100f;
            replayInput.RecordedMouseScrollDelta = Vector2.one * 2f;
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Middle, InputDefines.ButtonCondition.Up);
            replayInput.SetRecordedMouseButton(InputDefines.MouseButton.Right, InputDefines.ButtonCondition.Down);

            //データが正しく設定されるか確認
            var data = new MouseFrameInputData();
            data.Record(replayInput);

            data.RefleshUpdatedFlags(); // <- Test run here

            foreach (var t in data.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }

        /// <summary>
        /// <seealso cref="MouseFrameInputData.RegistTypeToFrameInputData()"/>
        /// </summary>
        [Test]
        public void RegistTypeToFrameInputDataPasses()
        {
            MouseFrameInputData.RegistTypeToFrameInputData();

            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(MouseFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType<MouseFrameInputData>());
            Assert.AreEqual(typeof(MouseFrameInputData), FrameInputData.GetChildFrameInputDataType(MouseFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.AreEqual(MouseFrameInputData.KEY_CHILD_INPUT_DATA_TYPE, FrameInputData.GetChildFrameInputDataKey<MouseFrameInputData>());

            Assert.DoesNotThrow(() => {
                MouseFrameInputData.RegistTypeToFrameInputData();
            });
        }
    }
}
