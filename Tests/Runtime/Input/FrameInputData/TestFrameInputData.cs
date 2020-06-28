using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input.FrameInputDataRecorder
{
    /// <summary>
    /// <seealso cref="FrameInputData"/>
    /// </summary>
    public class TestFrameInputData
    {
        class TestRecorder : IFrameDataRecorder, ISerializable
        {
            UpdateObserver<int> _value;

            public int Value { get => _value.Value; }

            public TestRecorder() { }

            public void CopyUpdatedDatasTo(IFrameDataRecorder other)
            {
                Assert.IsTrue(other is TestRecorder);

                var otherRecorder = other as TestRecorder;
                if (_value.DidUpdated) otherRecorder._value.Value = _value.Value;
            }

            public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
            {
                return new FrameInputDataKeyValue[] { ("value", _value) };
            }

            public void Record(ReplayableInput input)
            {
                _value.Value = input.TouchCount;
            }

            public void RecoverTo(ReplayableInput input)
            {
                input.RecordedTouchCount = _value.Value;
            }

            public void RefleshUpdatedFlags()
            {
                _value.Reset();
            }

            public void ResetDatas()
            {
                _value.SetDefaultValue(true);
            }

            readonly string Key = "Value";
            public TestRecorder(SerializationInfo info, StreamingContext context)
            {
                var e = info.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Name == Key)
                    {
                        _value.Value = (int)e.Value;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (_value.DidUpdated) info.AddValue(Key, _value.Value);
            }
        }

        class TestRecorder2 : TestRecorder
        {
            public TestRecorder2(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="FrameInputData.ContainsChildFrameInputDataType(string)"/>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType{T}(string)"/>
        /// <seealso cref="FrameInputData.UnregistChildFrameInputDataType(string)"/>
        /// </summary>
        [Test]
        public void BasicUsageChildFrameInputDataTypePasses()
        {
            var key = "TestKey";
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key));

            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(key));

            Debug.Log($"Success to RegistChildFrameInputDataType!");

            FrameInputData.UnregistChildFrameInputDataType(key);
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key));

            Debug.Log($"Success to UnregistChildFrameInputDataType!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.ContainsChildFrameInputDataType(string)"/>
        /// <seealso cref="FrameInputData.ContainsChildFrameInputDataType(System.Type)"/>
        /// <seealso cref="FrameInputData.ContainsChildFrameInputDataType{T}()"/>
        /// </summary>
        [Test]
        public void ContainsChildFrameInputDataTypePasses()
        {
            var key = "TestKey";
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key));
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(typeof(TestRecorder)));
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType<TestRecorder>());

            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(key));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(typeof(TestRecorder)));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType<TestRecorder>());
        }

        [Test]
        public void RegistChildFrameInputDataTypePasses()
        {
            var key = "TestKey";
            var key2 = "TestKey2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(key));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(key2));
        }

        /// <summary>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType{T}(string)"/>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType(string, System.Type)"/>
        /// </summary>
        [Test]
        public void RegistChildFrameInputDataTypeFail()
        {
            var key = "TestKey";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                FrameInputData.RegistChildFrameInputDataType<TestRecorder>("newKey");
            });
            Debug.Log($"Fail to test Registed Same Key!");

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key);
            });
            Debug.Log($"Fail to test Registed Same Type!");

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                FrameInputData.RegistChildFrameInputDataType("NewKey", typeof(string));
            });
            Debug.Log($"Fail to test the type is not IFrameInputDataRecorder and ISerializable!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.UnregistChildFrameInputDataType(string)"/>
        /// </summary>
        [Test]
        public void UnregistChildFrameInputDataTypePasses()
        {
            var key = "TestKey";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            FrameInputData.UnregistChildFrameInputDataType(key);
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key));
        }

        /// <summary>
        /// <seealso cref="FrameInputData.UnregistChildFrameInputDataType(string)"/>
        /// </summary>
        [Test]
        public void UnregistChildFrameInputDataTypeFail()
        {
            var key = "TestKey";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            FrameInputData.UnregistChildFrameInputDataType(key);

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                FrameInputData.UnregistChildFrameInputDataType(key);
            });
            Debug.Log($"Fail to Test The key don't regist!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.ClearChildFrameInputDataType()"/>
        /// </summary>
        [Test]
        public void ClearChildFrameInputDataTypePasses()
        {
            var key = "TestKey";
            var key2 = "TestKey2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            FrameInputData.ClearChildFrameInputDataType();

            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key));
            Assert.IsFalse(FrameInputData.ContainsChildFrameInputDataType(key2));
        }

        /// <summary>
        /// <seealso cref="FrameInputData.RegistedChildFrameInputDataTypes"/>
        /// </summary>
        [Test]
        public void RegistedChildFrameInputDataTypesPasses()
        {
            var testData = new (string key, System.Type type)[]
            {
                ("TestKey", typeof(TestRecorder)),
                ("TestKey2", typeof(TestRecorder2)),
            };
            foreach(var d in testData)
            {
                FrameInputData.RegistChildFrameInputDataType(d.key, d.type);
            }

            AssertionUtils.AssertEnumerableByUnordered(
                testData
                , FrameInputData.RegistedChildFrameInputDataTypes.Select(_t => (_t.Key, _t.Value))
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetChildFrameInputDataKey{T}"/>
        /// <seealso cref="FrameInputData.GetChildFrameInputDataKey(System.Type)"/>
        /// </summary>
        [Test]
        public void GetChildFrameInputDataKeyPasses()
        {
            var testData = new (string key, System.Type type)[]
            {
                ("TestKey", typeof(TestRecorder)),
                ("TestKey2", typeof(TestRecorder2)),
            };
            foreach (var d in testData)
            {
                FrameInputData.RegistChildFrameInputDataType(d.key, d.type);
            }

            foreach (var d in testData)
            {
                Assert.AreEqual(d.key, FrameInputData.GetChildFrameInputDataKey(d.type), $"Fail... type={d.type}");
            }

            Assert.AreEqual(testData[0].key, FrameInputData.GetChildFrameInputDataKey<TestRecorder>());
            Assert.AreEqual(testData[1].key, FrameInputData.GetChildFrameInputDataKey<TestRecorder2>());
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetChildFrameInputDataType(string)"/>
        /// </summary>
        [Test]
        public void GetChildFrameInputDataTypePasses()
        {
            var testData = new (string key, System.Type type)[]
            {
                ("TestKey", typeof(TestRecorder)),
                ("TestKey2", typeof(TestRecorder2)),
            };
            foreach (var d in testData)
            {
                FrameInputData.RegistChildFrameInputDataType(d.key, d.type);
            }

            foreach (var d in testData)
            {
                Assert.AreEqual(d.type, FrameInputData.GetChildFrameInputDataType(d.key), $"Fail... key={d.key}");
            }
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetChildFrameInputDataEnumerable()"/>
        /// </summary>
        [Test]
        public void GetChildFrameInputDataEnumerablePasses()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            throw new System.NotImplementedException();

            var data = new Hinode.FrameInputData();

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<Hinode.FrameInputData>(json);

            Assert.AreEqual(10, data.GetValuesEnumerable().Count());
            Assert.AreEqual(data.GetValuesEnumerable().Count(), dest.GetValuesEnumerable().Count());
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
            var serializer = new JsonSerializer();
            data.Record(replayInput);
            var frame = data.WriteToFrame(serializer);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <seealso cref="FrameInputData."/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void RecoverToPasses()
        {
            throw new System.NotImplementedException();

            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            replayInput.IsReplaying = true;
            var data = new Hinode.FrameInputData();

            //データが正しく設定されるか確認
            data.RecoverTo(replayInput);


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
