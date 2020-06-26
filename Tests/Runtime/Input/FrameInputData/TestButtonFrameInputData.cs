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
    /// <seealso cref="ButtonFrameInputData"/>
    /// </summary>
    public class TestButtonFrameInputData
    {
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
        /// <seealso cref="ButtonFrameInputData.UpdateInputDatas(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void UpdateInputDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            foreach(var name in buttonNames)
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
            data.UpdateInputDatas(replayInput);

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
                foreach(var buttonName in data.ObservedButtonNames)
                {
                    Assert.AreEqual(data.GetButton(buttonName), dest.GetButton(buttonName), $"Failed to serialize Button({buttonName})...");
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="ButtonFrameInputData.Update(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void UpdatePasses()
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
                "Fire2",
                "Jump",
            };
            data.AddObservedButtonNames(observedButtonNames);

            var frame = data.Update(replayInput);

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
            Debug.Log($"Success to Update Input Datas!");

            {
                var jsonSerializer = new JsonSerializer();
                var recoverInput = jsonSerializer.Deserialize<ButtonFrameInputData>(frame.InputText);
                AssertionUtils.AssertEnumerable(
                    data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                    , recoverInput.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                    , "Don't match Input Values..."
                );
            }
            Debug.Log($"Success to Create InputRecord.Frame!");
        }

        [Test]
        public void RecoverFramePasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

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

            var frameData = new InputRecord.Frame();
            var serializer = new JsonSerializer();
            frameData.InputText = serializer.Serialize(data);

            //データが正しく設定されるか確認
            var recoveredData = new ButtonFrameInputData();
            recoveredData.RecoverFrame(replayInput, frameData);

            AssertionUtils.AssertEnumerable(
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , recoveredData.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Failed to Recover Input Datas..."
            );
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

            data.UpdateInputDatas(replayInput);

            data.ResetDatas();
            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                data.ObservedButtonNames.Select(_n => (key: _n, value: (object)default(InputDefines.ButtonCondition)))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }
    }
}
