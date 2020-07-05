using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="AppendButtonInputData"/>
    /// </summary>
    public class TestAppendButtonInputData : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="AppendButtonInputData.OnAttached(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnAttachedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendButtonInputData>();
            var names = new string[] { "Jump", "Fire1" };
            inputObj.AddEnabledButtons(names);
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<ButtonFrameInputData>());

            var axisButtonData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<ButtonFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , axisButtonData.ObservedButtonNames
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendButtonInputData.Attach()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AttachPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendButtonInputData>();
            yield return null;

            var names = new string[] { "Jump", "Fire1" };
            inputObj.AddEnabledButtons(names);

            inputObj.Attach();
            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<ButtonFrameInputData>());

            var axisButtonData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<ButtonFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , axisButtonData.ObservedButtonNames
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendButtonInputData.CreateInputData()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInputDataPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendButtonInputData>();
            var names = new string[] { "Jump", "Fire1" };
            inputObj.AddEnabledButtons(names);

            var buttonData = inputObj.CreateInputData() as ButtonFrameInputData;
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , buttonData.ObservedButtonNames
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendButtonInputData.AddEnabledButtons(IEnumerable{string})"/>
        /// <seealso cref="AppendButtonInputData.AddEnabledButtons(string[])"/>
        /// <seealso cref="AppendButtonInputData.RemoveEnabledButtons(IEnumerable{string})"/>
        /// <seealso cref="AppendButtonInputData.RemoveEnabledButtons(string[])"/>
        /// <seealso cref="AppendButtonInputData.EnabledButtons"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddRemoveEnabledButtonsPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendButtonInputData>();
            var names = new string[] { "Jump", "Fire1" };
            inputObj.AddEnabledButtons(names);
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , inputObj.EnabledButtons
                , ""
            );
            Debug.Log($"Success to Add!");

            inputObj.RemoveEnabledButtons(names[0]);
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { names[1] }
                , inputObj.EnabledButtons
                , ""
            );
            Debug.Log($"Success to Remove!");
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendButtonInputData.ClearEnabledButton()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ClearEnabledButtonsPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendButtonInputData>();
            var names = new string[] { "Jump", "Fire1" };
            inputObj.AddEnabledButtons(names);
            inputObj.ClearEnabledButton();
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { }
                , inputObj.EnabledButtons
                , ""
            );
            yield return null;
        }
    }
}
