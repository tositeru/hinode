using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="AppendTouchInputData"/>
    /// </summary>
    public class TestAppendTouchInputData : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="AppendTouchInputData.OnAttached(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnAttachedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendTouchInputData>();
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<TouchFrameInputData>());

            Assert.IsTrue(frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<TouchFrameInputData>()
                .Any());
        }

        /// <summary>
        /// <seealso cref="AppendTouchInputData.Attach()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AttachPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendTouchInputData>();
            yield return null;

            inputObj.Attach();
            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<TouchFrameInputData>());

            Assert.IsTrue(frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<TouchFrameInputData>()
                .Any());
        }

        /// <summary>
        /// <seealso cref="AppendTouchInputData.CreateInputData()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInputDataPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendTouchInputData>();
            Assert.IsTrue(inputObj.CreateInputData() is TouchFrameInputData);
            yield return null;
        }
    }
}
