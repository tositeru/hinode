using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="KeyboardInputViewerItem"/>
	/// </summary>
    public class TestKeyboardInputViewerItem : TestBase
    {
        (InputViewer, KeyboardInputViewerItem) CreateKeyboardItem()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var keyboard = inputViewer.gameObject.AddComponent<KeyboardInputViewerItem>();
            inputViewer.RefleshItems();
            return (inputViewer, keyboard);
        }

        /// <summary>
		/// <seealso cref="KeyboardInputViewerItem.AddObservedKey"/>
		/// <seealso cref="KeyboardInputViewerItem.ObservedKeyCodes"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator AddObservedKeyPasses()
        {
            var (inputViewer, keyboard) = CreateKeyboardItem();

            var keyCodes = new List<KeyCode>() { KeyCode.A, KeyCode.LeftArrow };
            keyboard.AddObservedKey(keyCodes);

            AssertionUtils.AssertEnumerableByUnordered(
                keyCodes
                , keyboard.ObservedKeyCodes
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="KeyboardInputViewerItem.RemoveObservedKey"/>
		/// <seealso cref="KeyboardInputViewerItem.ObservedKeyCodes"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RemoveObservedKeyPasses()
        {
            var (inputViewer, keyboard) = CreateKeyboardItem();

            var keyCodes = new List<KeyCode>() { KeyCode.A, KeyCode.LeftArrow };
            keyboard.AddObservedKey(keyCodes);
            keyboard.RemoveObservedKey(KeyCode.LeftArrow);

            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] { KeyCode.A }
                , keyboard.ObservedKeyCodes
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="KeyboardInputViewerItem.KeyCodeLimitPerText"/>
        /// <seealso cref="KeyboardInputViewerItem.KeyCodeTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator KeyCodeTextsPasses()
        {
            var (inputViewer, keyboard) = CreateKeyboardItem();

            keyboard.KeyCodeLimitPerText = 3;
            keyboard.AddObservedKey(KeyCodeDefines.AlphabetKeyCodes);
            yield return null; // <- Create and Update KeyCodeText in KeyboardInputViewerItem#UpdateItem()

            var count = keyboard.ObservedKeyCodes.Count / keyboard.KeyCodeLimitPerText
                + Mathf.Min(1, keyboard.ObservedKeyCodes.Count % keyboard.KeyCodeLimitPerText);
            Assert.AreEqual(count, keyboard.KeyCodeTexts.Count);
            AssertionUtils.AssertEnumerableByUnordered(
                keyboard.KeyCodeTexts.SelectMany(_t => _t.KeyCodes)
                , keyboard.ObservedKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardInputViewerItem.KeyCodeLimitPerText"/>
        /// <seealso cref="KeyboardInputViewerItem.KeyCodeTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator KeyCodeLimitPerTextChangedPasses()
        {
            var (inputViewer, keyboard) = CreateKeyboardItem();
            keyboard.AddObservedKey(KeyCodeDefines.AlphabetKeyCodes);

            var testData = new int[]
            {
                3,
                12,
                8,
                24,
            };

            foreach(var d in testData)
            {
                keyboard.KeyCodeLimitPerText = d;
                yield return null; // <- Create and Update KeyCodeTexts in KeyboardInputViewerItem#UpdateItem()

                var count = keyboard.ObservedKeyCodes.Count / keyboard.KeyCodeLimitPerText
                    + Mathf.Min(1, keyboard.ObservedKeyCodes.Count % keyboard.KeyCodeLimitPerText);
                Assert.AreEqual(count, keyboard.KeyCodeTexts.Count);
                AssertionUtils.AssertEnumerableByUnordered(
                    keyboard.KeyCodeTexts.SelectMany(_t => _t.KeyCodes)
                    , keyboard.ObservedKeyCodes
                    , ""
                );
            }
        }

        /// <summary>
		/// <seealso cref="KeyboardInputViewerItem.OnChangedStyle(InputViewerStyleInfo)"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator OnChangedStylePasses()
        {
            var (inputViewer, keyboard) = CreateKeyboardItem();
            inputViewer.UseInput.RecordedMousePresent = true;
            yield return null;

            inputViewer.StyleInfo.Font = new Font();
            inputViewer.StyleInfo.FontColor = Color.green;

            foreach(var keyCodeText in keyboard.KeyCodeTexts)
            {
                Assert.AreSame(inputViewer.StyleInfo.Font, keyCodeText.Text.font);
                Assert.AreEqual(inputViewer.StyleInfo.FontColor, keyCodeText.Text.color);
            }
        }
    }
}
