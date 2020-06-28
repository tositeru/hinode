using System.Collections;
using System.Collections.Generic;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="IFrameDataRecorder"/>
    /// </summary>
    public class TestIFrameDataRecorder
    {
        class TestFrameDataRecorder : IFrameDataRecorder
        {
            UpdateObserver<int> _touchCount = new UpdateObserver<int>();
            public int TouchCount { get => _touchCount.Value; }

            public void CopyUpdatedDatasTo(IFrameDataRecorder other)
            {
                var r = other as TestFrameDataRecorder;
                r._touchCount.Value = TouchCount;
            }

            public void RecoverTo(ReplayableInput input)
            {
                input.RecordedTouchCount = TouchCount;
            }

            public void ResetDatas()
            {
                _touchCount.SetDefaultValue(true);
            }

            public void Record(ReplayableInput input)
            {
                _touchCount.Value = input.TouchCount;
            }

            public void RefleshUpdatedFlags()
            {
                _touchCount.Reset();
            }

            public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
            {
                return new FrameInputDataKeyValue[]
                {
                    new FrameInputDataKeyValue("touchCount", _touchCount),
                };
            }
        }

        /// <summary>
        /// <seealso cref="IFrameDataRecorderExtensions.WriteToFrame(IFrameDataRecorder, Serialization.ISerializer)"/>
        /// </summary>
        [Test]
        public void WriteToFramePasses()
        {
            var recorder = new MouseFrameInputData();
            recorder.SetMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);

            var serializer = new JsonSerializer();
            var frame = recorder.WriteToFrame(serializer);

            Assert.AreEqual(serializer.Serialize(recorder), frame.InputText);            
        }

        /// <summary>
        /// <seealso cref="IFrameDataRecorderExtensions.RecoverFromFrame()"/>
        /// </summary>
        [Test]
        public void RecoverFromFramePasses()
        {
            var recorder = new MouseFrameInputData();
            var btn = InputDefines.MouseButton.Left;
            recorder.SetMouseButton(btn, InputDefines.ButtonCondition.Push);

            var serializer = new JsonSerializer();
            var frame = recorder.WriteToFrame(serializer);

            var otherRecoder = new MouseFrameInputData();
            otherRecoder.RecoverFromFrame(frame, serializer);

            Assert.AreEqual(recorder.GetMouseButton(btn), otherRecoder.GetMouseButton(btn));
        }
    }
}
