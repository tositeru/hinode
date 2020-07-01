using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="AppendMouseInputData"/>
    /// </summary>
    public class TestAppendMouseInputData : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="AppendMouseInputData.OnAttached(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnAttachedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendMouseInputData>();
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<MouseFrameInputData>());

            Assert.IsTrue(frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<MouseFrameInputData>()
                .Any());
        }

        /// <summary>
        /// <seealso cref="AppendMouseInputData.Attach()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AttachPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendMouseInputData>();
            yield return null;

            inputObj.Attach();
            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<MouseFrameInputData>());

            Assert.IsTrue(frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<MouseFrameInputData>()
                .Any());
        }

        /// <summary>
        /// <seealso cref="AppendMouseInputData.CreateInputData()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInputDataPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendMouseInputData>();
            Assert.IsTrue(inputObj.CreateInputData() is MouseFrameInputData);
            yield return null;
        }
    }
}
