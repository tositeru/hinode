using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="AppendKeyboardInputData"/>
    /// </summary>
    public class TestAppendKeyboardInputData : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <seealso cref="AppendKeyboardInputData.OnAttached(InputRecorder)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator OnAttachedPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendKeyboardInputData>();
            var keycodes = new KeyCode[] { KeyCode.A, KeyCode.F1 };
            inputObj.AddEnabledKeyCodes(keycodes);
            yield return null;

            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<KeyboardFrameInputData>());

            var inputData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<KeyboardFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                keycodes
                , inputData.ObservedKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendKeyboardInputData.Attach()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AttachPasses()
        {
            var recorder = new GameObject().AddComponent<InputRecorderMonoBehaviour>();
            var inputObj = recorder.gameObject.AddComponent<AppendKeyboardInputData>();
            yield return null;

            var keycodes = new KeyCode[] { KeyCode.A, KeyCode.F1 };
            inputObj.AddEnabledKeyCodes(keycodes);

            inputObj.Attach();
            var frameInputData = recorder.UseRecorder.FrameDataRecorder as FrameInputData;
            Assert.IsTrue(frameInputData.ContainsChildRecorder<KeyboardFrameInputData>());

            var inputData = frameInputData.GetChildRecorderEnumerable()
                .Select(_t => _t.child)
                .OfType<KeyboardFrameInputData>()
                .FirstOrDefault();
            AssertionUtils.AssertEnumerableByUnordered(
                keycodes
                , inputData.ObservedKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AppendKeyboardInputData.CreateInputData()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CreateInputDataPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendKeyboardInputData>();
            var keycodes = new KeyCode[] { KeyCode.A, KeyCode.F1 };
            inputObj.AddEnabledKeyCodes(keycodes);

            var inputData = inputObj.CreateInputData() as KeyboardFrameInputData;
            AssertionUtils.AssertEnumerableByUnordered(
                keycodes
                , inputData.ObservedKeyCodes
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendKeyboardInputData.AddEnabledKeyCodes(IEnumerable{string})"/>
        /// <seealso cref="AppendKeyboardInputData.AddEnabledKeyCodes(string[])"/>
        /// <seealso cref="AppendKeyboardInputData.RemoveEnabledKeyCodes(IEnumerable{string})"/>
        /// <seealso cref="AppendKeyboardInputData.RemoveEnabledKeyCodes(string[])"/>
        /// <seealso cref="AppendKeyboardInputData.EnabledKeyCodes"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AddRemoveEnabledKeyCodesPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendKeyboardInputData>();
            var keycodes = new KeyCode[] { KeyCode.A, KeyCode.F1 };
            inputObj.AddEnabledKeyCodes(keycodes);
            AssertionUtils.AssertEnumerableByUnordered(
                keycodes
                , inputObj.EnabledKeyCodes
                , ""
            );
            Debug.Log($"Success to Add!");

            inputObj.RemoveEnabledKeyCodes(keycodes[0]);
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] { keycodes[1] }
                , inputObj.EnabledKeyCodes
                , ""
            );
            Debug.Log($"Success to Remove!");
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AppendKeyboardInputData.ClearEnabledKeyCodes()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ClearEnabledKeyCodesPasses()
        {
            var inputObj = new GameObject().AddComponent<AppendKeyboardInputData>();
            var keycodes = new KeyCode[] { KeyCode.A, KeyCode.F1 };
            inputObj.AddEnabledKeyCodes(keycodes);
            inputObj.ClearEnabledKeyCodes();
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] { }
                , inputObj.EnabledKeyCodes
                , ""
            );
            yield return null;
        }
    }
}
