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
    /// <seealso cref="AxisButtonFrameInputData"/>
    /// </summary>
    public class TestAxisButtonFrameInputData
    {
        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.AddObservedButtonNames(string[])"/>
        /// <seealso cref="AxisButtonFrameInputData.SetAxis(string, float)"/>
        /// <seealso cref="AxisButtonFrameInputData.GetAxis(string)"/>
        /// </summary>
        [Test]
        public void AxisBasicUsagePasses()
        {
            var data = new AxisButtonFrameInputData();
            var buttonName = "Horizontal";
            data.AddObservedButtonNames(buttonName);
            Assert.IsTrue(data.ContainsButton(buttonName));

            var axis = 1f;
            data.SetAxis(buttonName, axis);
            Assert.AreEqual(axis, data.GetAxis(buttonName));

        }

        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test]
        public void GetValuesEnumerablePasses()
        {
            var data = new AxisButtonFrameInputData();
            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            data.AddObservedButtonNames(buttonNames);

            AssertionUtils.AssertEnumerableByUnordered(
                buttonNames.Select(_n => (key: _n, (object)data.GetAxis(_n)))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="AxisButtonFrameInputData.GetKeyType(string)"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var testData = new (string key, System.Type type)[]
            {
                ("Horizontal", typeof(float)),
                ("Vertical", typeof(float)),
            };
            foreach (var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}";
                Assert.AreEqual(d.type, AxisButtonFrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedButton(name, InputDefines.ButtonCondition.Down);
            }

            //データが正しく設定されるか確認
            var data = new AxisButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            data.AddObservedButtonNames(observedButtonNames);

            //Update only observed Button
            data.Record(replayInput);

            foreach (var name in buttonNames)
            {
                var errorMessage = $"Fail... ButtonName={name}";
                if (observedButtonNames.Contains(name))
                {
                    Assert.AreEqual(replayInput.GetAxis(name), data.GetAxis(name), errorMessage);
                }
                else
                {
                    Assert.AreEqual(0f, data.GetAxis(name), errorMessage);
                }
            }
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="AxisButtonFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="AxisButtonFrameInputData.AxisButtonFrameInputData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// <seealso cref="AxisButtonFrameInputData.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new AxisButtonFrameInputData();

            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            foreach (var name in buttonNames)
            {
                data.SetAxis(name, 0.5f);
            }

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<AxisButtonFrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(data.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                foreach (var buttonName in data.ObservedButtonNames)
                {
                    Assert.AreEqual(data.GetAxis(buttonName), dest.GetAxis(buttonName), $"Failed to serialize Button({buttonName})...");
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            replayInput.IsReplaying = true;

            var data = new AxisButtonFrameInputData();
            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            foreach (var name in buttonNames)
            {
                data.SetAxis(name, -1f);
            }

            var frameData = new InputRecord.Frame();
            var serializer = new JsonSerializer();
            frameData.InputText = serializer.Serialize(data);

            data.RecoverTo(replayInput); // <- Test is here

            //データが正しく設定されるか確認
            {
                foreach (var buttonName in data.ObservedButtonNames)
                {
                    Assert.AreEqual(data.GetAxis(buttonName), replayInput.GetAxis(buttonName), $"Failed to serialize Button({buttonName})...");
                }
            }
            Debug.Log($"Success to Set Input Datas to ReplayableInput!");
        }

        /// <summary>
        /// <see cref="AxisButtonFrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedAxis(name, -12f);
            }

            //データが正しく設定されるか確認
            var data = new AxisButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            data.AddObservedButtonNames(observedButtonNames);
            data.Record(replayInput);

            data.ResetDatas(); // <- Test run Here

            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                data.ObservedButtonNames.Select(_n => (key: _n, value: (object)default(float)))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <seealso cref="AxisButtonFrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleashUpdatedFlags()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var buttonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedAxis(name, -12f);
            }

            //データが正しく設定されるか確認
            var data = new AxisButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Horizontal",
                "Vertical",
            };
            data.AddObservedButtonNames(observedButtonNames);
            data.Record(replayInput);

            data.RefleshUpdatedFlags(); // <- Test run here.

            foreach(var t in data.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }
    }
}
