using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="AppendAxisButtonInputData"/>
    /// </summary>
    public class TestAppendAxisButtonInputData : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="AppendAxisButtonInputData.OnAttached(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnAttachedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendAxisButtonInputData>();
            var names = new string[] { "Horizontal", "Vertical" };
            inputObj.AddEnabledAxisButtons(names);
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<AxisButtonFrameInputData>());

            var axisButtonData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<AxisButtonFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , axisButtonData.ObservedButtonNames
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendAxisButtonInputData.Attach()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AttachPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendAxisButtonInputData>();
            yield return null;

            var names = new string[] { "Horizontal", "Vertical" };
            inputObj.AddEnabledAxisButtons(names);

            inputObj.Attach();
            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<AxisButtonFrameInputData>());

            var axisButtonData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<AxisButtonFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , axisButtonData.ObservedButtonNames
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendAxisButtonInputData.CreateInputData()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInputDataPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendAxisButtonInputData>();
            var names = new string[] { "Horizontal", "Vertical" };
            inputObj.AddEnabledAxisButtons(names);

            var axisButtonData = inputObj.CreateInputData() as AxisButtonFrameInputData;
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , axisButtonData.ObservedButtonNames
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendAxisButtonInputData.AddEnabledAxisButtons(IEnumerable{string})"/>
        /// <seealso cref="AppendAxisButtonInputData.AddEnabledAxisButtons(string[])"/>
        /// <seealso cref="AppendAxisButtonInputData.RemoveEnabledAxisButtons(IEnumerable{string})"/>
        /// <seealso cref="AppendAxisButtonInputData.RemoveEnabledAxisButtons(string[])"/>
        /// <seealso cref="AppendAxisButtonInputData.EnabledAxisButtons"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddRemoveEnabledAxisButtonsPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendAxisButtonInputData>();
            var names = new string[] { "Horizontal", "Vertical" };
            inputObj.AddEnabledAxisButtons(names);
            AssertionUtils.AssertEnumerableByUnordered(
                names
                , inputObj.EnabledAxisButtons
                , ""
            );
            Debug.Log($"Success to Add!");

            inputObj.RemoveEnabledAxisButtons(names[0]);
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { names[1] }
                , inputObj.EnabledAxisButtons
                , ""
            );
            Debug.Log($"Success to Remove!");
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendAxisButtonInputData.ClearEnabledAxisButton()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ClearEnabledAxisButtonsPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendAxisButtonInputData>();
            var names = new string[] { "Horizontal", "Vertical" };
            inputObj.AddEnabledAxisButtons(names);
            inputObj.ClearEnabledAxisButton();
            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { }
                , inputObj.EnabledAxisButtons
                , ""
            );
            yield return null;
        }

    }
}
