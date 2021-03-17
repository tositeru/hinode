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
        [ContainsSerializationKeyTypeGetter(typeof(TestRecorder))]
        class TestRecorder : IFrameDataRecorder, ISerializable
        {
            public static string KeyValue = "value";

            [SerializeField] UpdateObserver<int> _value = new UpdateObserver<int>();

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
                return new FrameInputDataKeyValue[] { (KeyValue, _value) };
            }

            public void Record(ReplayableInput input)
            {
                //手ごろなプロパティに設定しているだけで、特に意味はない処理
                _value.Value = input.TouchCount > 0 ? input.GetTouch(0).fingerId : -1;
            }

            public void RecoverTo(ReplayableInput input)
            {
                input.ClearRecordedTouch();
                //手ごろなプロパティに設定しているだけで、特に意味はない処理
                input.SetRecordedTouch(0, new Touch() { fingerId = _value.Value });
            }

            public void RefleshUpdatedFlags()
            {
                _value.Reset();
            }

            public void ResetDatas()
            {
                _value.SetDefaultValue(true);
            }

            public TestRecorder(SerializationInfo info, StreamingContext context)
            {
                var e = info.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Name == KeyValue)
                    {
                        _value.Value = (int)e.Value;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (_value.DidUpdated) info.AddValue(KeyValue, _value.Value);
            }

            [SerializationKeyTypeGetter]
            public static System.Type GetKeyType(string key)
            {
                if (key == KeyValue) return typeof(int);
                return null;
            }
        }

        [ContainsSerializationKeyTypeGetter(typeof(TestRecorder2))]
        class TestRecorder2 : TestRecorder
        {
            public TestRecorder2() { }

            public TestRecorder2(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}

            [SerializationKeyTypeGetter]
            public static new System.Type GetKeyType(string key)
                => TestRecorder.GetKeyType(key);
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
        /// <seealso cref="FrameInputData.AddChildRecorder(IFrameDataRecorder)"/>
        /// <seealso cref="FrameInputData.ContainsChildRecorder(string)"/>
        /// <seealso cref="FrameInputData.RemoveChildRecorder(string)"/>
        /// </summary>
        [Test]
        public void ChildRecorderPasses()
        {
            var key = "test";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            var recorder = new FrameInputData();

            var child = new TestRecorder();
            recorder.AddChildRecorder(child);

            Assert.IsTrue(recorder.ContainsChildRecorder(key));

            recorder.RemoveChildRecorder(key);
            Assert.IsFalse(recorder.ContainsChildRecorder(key));
        }

        /// <summary>
        /// <seealso cref="FrameInputData.AddChildRecorder(IFrameDataRecorder)"/>
        /// </summary>
        [Test]
        public void AddChildRecorderFails()
        {
            var key = "test";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            var recorder = new FrameInputData();
            recorder
                .AddChildRecorder(new TestRecorder());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                recorder.AddChildRecorder(new TestRecorder());
            });
            Debug.Log($"Fail to AddChildFrameInput() The Type already add!");

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                recorder.AddChildRecorder(new TestRecorder2());
            });
            Debug.Log($"Fail to AddChildFrameInput() The Type don't Regist FrameInputData!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.ContainsChildRecorder(string)"/>
        /// <seealso cref="FrameInputData.ContainsChildRecorder(System.Type)"/>
        /// <seealso cref="FrameInputData.ContainsChildRecorder{T}"/>
        /// </summary>
        [Test]
        public void ContainsChildRecorderPasses()
        {
            var key = "test";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            var recorder = new FrameInputData();

            var child = new TestRecorder();
            recorder.AddChildRecorder(child);

            Assert.IsTrue(recorder.ContainsChildRecorder(key));
            Assert.IsTrue(recorder.ContainsChildRecorder(typeof(TestRecorder)));
            Assert.IsTrue(recorder.ContainsChildRecorder<TestRecorder>());
        }

        /// <summary>
        /// <seealso cref="FrameInputData.RemoveChildRecorder(string)"/>
        /// <seealso cref="FrameInputData.RemoveChildRecorder(System.Type)"/>
        /// <seealso cref="FrameInputData.RemoveChildRecorder{T}()"/>
        /// </summary>
        [Test]
        public void RemoveChildRecorderPasses()
        {
            var key = "test";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);

            var recorder = new FrameInputData();

            {
                var child = new TestRecorder();
                recorder.AddChildRecorder(child);
                recorder.RemoveChildRecorder(key);
                Assert.IsFalse(recorder.ContainsChildRecorder(key));
            }
            Debug.Log($"Success to RemoveChildRecorder(key)!");

            {
                var child = new TestRecorder();
                recorder.AddChildRecorder(child);
                recorder.RemoveChildRecorder(child.GetType());
                Assert.IsFalse(recorder.ContainsChildRecorder(key));
            }
            Debug.Log($"Success to RemoveChildRecorder(System.Type)!");

            {
                var child = new TestRecorder();
                recorder.AddChildRecorder(child);
                recorder.RemoveChildRecorder<TestRecorder>();
                Assert.IsFalse(recorder.ContainsChildRecorder(key));
            }
            Debug.Log($"Success to RemoveChildRecorder<T>()!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetChildRecorderEnumerable()"/>
        /// </summary>
        [Test]
        public void GetChildRecorderEnumerablePasses()
        {
            var key = "test";
            var key2= "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, IFrameDataRecorder)[] {
                    (key, childRecorder),
                    (key2, childRecorder2),
                }
                , recorder.GetChildRecorderEnumerable()
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test]
        public void GetValuesEnumerablePasses()
        {
            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            AssertionUtils.AssertEnumerableByUnordered(
                childRecorder.GetValuesEnumerable()
                    .Select(_t => ($"{key}.{_t.Key}", _t.Value.RawValue))
                .Concat(
                    childRecorder2.GetValuesEnumerable()
                        .Select(_t => ($"{key2}.{_t.Key}", _t.Value.RawValue)))
                , recorder.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="FrameInputData.GetKeyType(string)"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var testData = new (string key, System.Type type)[]
            {
                (key, typeof(TestRecorder)),
                (key2, typeof(TestRecorder2))
            };
            foreach(var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}, type={d.type}";
                Assert.AreEqual(d.type, FrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="FrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = new ReplayableInput();
            replayInput.IsReplaying = true;
            replayInput.SetRecordedTouch(0, new Touch() { fingerId = 22 });

            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            recorder.Record(replayInput);

            AssertionUtils.AssertEnumerableByUnordered(
                new (string, object)[] {
                    ($"{key}.{TestRecorder.KeyValue}", replayInput.GetTouch(0).fingerId),
                    ($"{key2}.{TestRecorder2.KeyValue}", replayInput.GetTouch(0).fingerId),
                }
                , recorder.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , ""
            );
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="FrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="FrameInputData.FrameInputData(SerializationInfo, StreamingContext)"/>
        /// <seealso cref="FrameInputData.GetObjectData(SerializationInfo, StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = new ReplayableInput();
            replayInput.IsReplaying = true;

            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            replayInput.SetRecordedTouch(0, new Touch() { fingerId = 111 });
            recorder.Record(replayInput);

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(recorder);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<FrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(recorder.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                AssertionUtils.AssertEnumerableByUnordered(
                    recorder.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                    , dest.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                    , ""
                );
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="FrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = new ReplayableInput();
            replayInput.IsReplaying = true;

            replayInput.SetRecordedTouch(0, new Touch() { fingerId = 100 });

            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            recorder.RecoverTo(replayInput); // <- Test is here

            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[] {
                    ($"{key}.{TestRecorder.KeyValue}", replayInput.GetTouch(0).fingerId),
                    ($"{key2}.{TestRecorder2.KeyValue}", replayInput.GetTouch(0).fingerId),
                }
                , recorder.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , ""
            );
        }

        /// <summary>
        /// <see cref="FrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            var replayInput = new ReplayableInput();
            replayInput.IsReplaying = true;

            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            recorder.Record(replayInput);

            recorder.ResetDatas(); // <- Test is here

            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[] {
                    ($"{key}.{TestRecorder.KeyValue}", default(int)),
                    ($"{key2}.{TestRecorder2.KeyValue}", default(int)),
                }
                , recorder.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="FrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleashUpdatedFlags()
        {
            var replayInput = new ReplayableInput();
            replayInput.IsReplaying = true;

            var key = "test";
            var key2 = "test2";
            FrameInputData.RegistChildFrameInputDataType<TestRecorder>(key);
            FrameInputData.RegistChildFrameInputDataType<TestRecorder2>(key2);

            var recorder = new FrameInputData();
            var childRecorder = new TestRecorder();
            var childRecorder2 = new TestRecorder2();
            recorder
                .AddChildRecorder(childRecorder)
                .AddChildRecorder(childRecorder2);

            recorder.Record(replayInput);

            recorder.ResetDatas(); // <- Test is here

            foreach (var t in recorder.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }
    }
}
