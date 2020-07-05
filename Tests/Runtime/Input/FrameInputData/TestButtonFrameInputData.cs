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
    /// <seealso cref="ButtonFrameInputData"/>
    /// </summary>
    public class TestButtonFrameInputData
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ButtonBasicUsagePasses()
        {
            var data = new ButtonFrameInputData();
            var buttonName = "Fire1";
            data.AddObservedButtonNames(buttonName);
            Assert.IsTrue(data.ContainsButton(buttonName));

            data.SetButton(buttonName, InputDefines.ButtonCondition.Down);
            Assert.AreEqual(InputDefines.ButtonCondition.Down, data.GetButton(buttonName));

        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test]
        public void GetValuesEnumerablePasses()
        {
            var data = new ButtonFrameInputData();
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            data.AddObservedButtonNames(buttonNames);

            AssertionUtils.AssertEnumerableByUnordered(
                buttonNames.Select(_n => (key: _n, (object)data.GetButton(_n)))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="ButtonFrameInputData.GetKeyType(string)"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var testData = new (string key, System.Type type)[]
            {
                ("Fire1", typeof(int)),
                ("Fire2", typeof(int)),
                ("Jump", typeof(int)),
            };
            foreach (var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}";
                Assert.AreEqual(d.type, ButtonFrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedButton(name, InputDefines.ButtonCondition.Down);
            }

            //データが正しく設定されるか確認
            var data = new ButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Fire1",
                "Jump",
            };
            data.AddObservedButtonNames(observedButtonNames);

            //Update only observed Button
            data.Record(replayInput);

            foreach (var name in buttonNames)
            {
                var errorMessage = $"Fail... ButtonName={name}";
                if (observedButtonNames.Contains(name))
                {
                    Assert.AreEqual(replayInput.GetButtonCondition(name), data.GetButton(name), errorMessage);
                }
                else
                {
                    Assert.AreEqual(InputDefines.ButtonCondition.Free, data.GetButton(name), errorMessage);
                }
            }
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="ButtonFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="ButtonFrameInputData.ButtonFrameInputData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// <seealso cref="ButtonFrameInputData.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new ButtonFrameInputData();

            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach (var name in buttonNames)
            {
                data.SetButton(name, InputDefines.ButtonCondition.Down);
            }

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<ButtonFrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(data.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                foreach (var buttonName in data.ObservedButtonNames)
                {
                    Assert.AreEqual(data.GetButton(buttonName), dest.GetButton(buttonName), $"Failed to serialize Button({buttonName})...");
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            replayInput.IsReplaying = true;

            var data = new ButtonFrameInputData();
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach (var name in buttonNames)
            {
                data.SetButton(name, InputDefines.ButtonCondition.Down);
            }

            //データが正しく設定されるか確認
            data.RecoverTo(replayInput);

            Debug.Log($"Success to Recover Input Datas!");

            {
                foreach (var buttonName in data.ObservedButtonNames)
                {
                    Assert.AreEqual(data.GetButton(buttonName), replayInput.GetButton(buttonName), $"Failed to serialize Button({buttonName})...");
                }
            }
            Debug.Log($"Success to Set Input Datas to ReplayableInput!");
        }

        /// <summary>
        /// <see cref="ButtonFrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedButton(name, InputDefines.ButtonCondition.Down);
            }

            //データが正しく設定されるか確認
            var data = new ButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Fire1",
                "Jump",
            };
            data.AddObservedButtonNames(observedButtonNames);
            data.Record(replayInput);

            data.ResetDatas(); // <- Test run here

            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                data.ObservedButtonNames.Select(_n => (key: _n, value: (object)default(InputDefines.ButtonCondition)))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <see cref="ButtonFrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleshUpdatedFlagsPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach (var name in buttonNames)
            {
                replayInput.SetRecordedButton(name, InputDefines.ButtonCondition.Down);
            }

            //データが正しく設定されるか確認
            var data = new ButtonFrameInputData();
            var observedButtonNames = new string[] {
                "Fire1",
                "Jump",
            };
            data.AddObservedButtonNames(observedButtonNames);
            data.Record(replayInput);

            data.RefleshUpdatedFlags(); // <- Test run here

            foreach(var t in data.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.RegistTypeToFrameInputData()"/>
        /// </summary>
        [Test]
        public void RegistTypeToFrameInputDataPasses()
        {
            ButtonFrameInputData.RegistTypeToFrameInputData();

            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(ButtonFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType<ButtonFrameInputData>());
            Assert.AreEqual(typeof(ButtonFrameInputData), FrameInputData.GetChildFrameInputDataType(ButtonFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.AreEqual(ButtonFrameInputData.KEY_CHILD_INPUT_DATA_TYPE, FrameInputData.GetChildFrameInputDataKey<ButtonFrameInputData>());

            Assert.DoesNotThrow(() => {
                ButtonFrameInputData.RegistTypeToFrameInputData();
            });
        }
    }
}
