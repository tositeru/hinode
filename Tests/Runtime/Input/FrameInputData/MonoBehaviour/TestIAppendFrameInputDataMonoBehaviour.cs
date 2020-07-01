using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="IAppendFrameInputDataMonoBehaviour"/>
    /// </summary>
    public class TestIAppendFrameInputDataMonoBehaviour : TestBase
    {
        class TestRecorder : IFrameDataRecorder, ISerializable
        {
            public static string KEY_CHILD_FRAME_INPUT_DATA = "test";
            public static void RegistToFrameInputData()
            {
                FrameInputData.RegistChildFrameInputDataType<TestRecorder>(KEY_CHILD_FRAME_INPUT_DATA);
            }

            public static string KeyValue = "value";
            public UpdateObserver<int> Value { get; set; } = new UpdateObserver<int>();

            public TestRecorder() { }

            public void CopyUpdatedDatasTo(IFrameDataRecorder other)
            {
                var otherRecoder = other as TestRecorder;
                otherRecoder.Value.Value = Value.Value;
            }

            public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
            {
                return new FrameInputDataKeyValue[]
                {
                    (KeyValue, Value)
                };
            }

            public void Record(ReplayableInput input)
            {
                Value.Value = input.TouchCount;
            }

            public void RecoverTo(ReplayableInput input)
            {
                input.RecordedTouchCount = Value.Value;
            }

            public void RefleshUpdatedFlags()
            {
                Value.Reset();
            }

            public void ResetDatas()
            {
                Value.SetDefaultValue(true);
            }

            public TestRecorder(SerializationInfo info, StreamingContext context)
            {
                var e = info.GetEnumerator();
                while(e.MoveNext())
                {
                    if(e.Name == KeyValue)
                    {
                        Value.Value = (int)e.Value;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (Value.DidUpdated) info.AddValue(KeyValue, Value.Value);
            }
        }

        class TestAttachInputData : IAppendFrameInputDataMonoBehaviour
        {
            public override IFrameDataRecorder CreateInputData()
            {
                return new TestRecorder();
            }

            protected override void OnAttached(InputRecorder inputRecorder)
            {
                Assert.IsTrue(inputRecorder.FrameDataRecorder is FrameInputData);
                var frameInputData = inputRecorder.FrameDataRecorder as FrameInputData;

                TestRecorder.RegistToFrameInputData();
                frameInputData.AddChildRecorder(CreateInputData());
            }
        }

        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="IAppendFrameInputDataMonoBehaviour.OnAwake(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnStartedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<TestAttachInputData>();
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<TestRecorder>());
        }
    }
}
