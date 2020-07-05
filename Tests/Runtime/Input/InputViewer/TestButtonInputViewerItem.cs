using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="ButtonInputViewerItem"/>
	/// </summary>
    public class TestButtonInputViewerItem : TestBase
    {
        (InputViewer, ButtonInputViewerItem) CreateButtonItem()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var button = inputViewer.gameObject.AddComponent<ButtonInputViewerItem>();
            inputViewer.RefleshItems();
            return (inputViewer, button);
        }

        /// <summary>
		/// <seealso cref="ButtonInputViewerItem.AddObservedButton"/>
		/// <seealso cref="ButtonInputViewerItem.ObservedButtons"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator AddObservedButtonPasses()
        {
            var (inputViewer, button) = CreateButtonItem();

            var buttons = new List<string>() { "Jump", "Fire1" };
            button.AddObservedButton(buttons);

            AssertionUtils.AssertEnumerableByUnordered(
                buttons
                , button.ObservedButtons
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="ButtonInputViewerItem.RemoveObservedButton"/>
		/// <seealso cref="ButtonInputViewerItem.ObservedButtons"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RemoveObservedButtonPasses()
        {
            var (inputViewer, button) = CreateButtonItem();

            var buttons = new List<string>() { "Jump", "Fire1" };
            button.AddObservedButton(buttons);
            button.RemoveObservedButton(buttons[1]);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { buttons[0] }
                , button.ObservedButtons
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="ButtonInputViewerItem.ButtonLimitPerText"/>
        /// <seealso cref="ButtonInputViewerItem.ButtonTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ButtonTextsPasses()
        {
            var (inputViewer, button) = CreateButtonItem();

            button.ButtonLimitPerText = 3;
            button.AddObservedButton(Enumerable.Range(0, 30).Select(_i => $"Fire{_i}"));
            yield return null; // <- Create and Update ButtonText in ButtonInputViewerItem#UpdateItem()

            var count = button.ObservedButtons.Count / button.ButtonLimitPerText
                + Mathf.Min(1, button.ObservedButtons.Count % button.ButtonLimitPerText);
            Assert.AreEqual(count, button.ButtonTexts.Count);
            AssertionUtils.AssertEnumerableByUnordered(
                button.ButtonTexts.SelectMany(_t => _t.Buttons)
                , button.ObservedButtons
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="ButtonInputViewerItem.ButtonLimitPerText"/>
        /// <seealso cref="ButtonInputViewerItem.ButtonTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ButtonLimitPerTextChangedPasses()
        {
            var (inputViewer, button) = CreateButtonItem();
            button
                .AddObservedButton(Enumerable.Range(0, 30).Select(_i => $"Fire{_i}"));

            var testData = new int[]
            {
                3,
                12,
                8,
                24,
            };

            foreach(var d in testData)
            {
                button.ButtonLimitPerText = d;
                yield return null; // <- Create and Update ButtonTexts in ButtonInputViewerItem#UpdateItem()

                var count = button.ObservedButtons.Count / button.ButtonLimitPerText
                    + Mathf.Min(1, button.ObservedButtons.Count % button.ButtonLimitPerText);
                Assert.AreEqual(count, button.ButtonTexts.Count);
                AssertionUtils.AssertEnumerableByUnordered(
                    button.ButtonTexts.SelectMany(_t => _t.Buttons)
                    , button.ObservedButtons
                    , ""
                );
            }
        }

        /// <summary>
		/// <seealso cref="ButtonInputViewerItem.OnChangedStyle(InputViewerStyleInfo)"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator OnChangedStylePasses()
        {
            var (inputViewer, button) = CreateButtonItem();
            inputViewer.UseInput.RecordedMousePresent = true;
            yield return null;

            inputViewer.StyleInfo.Font = new Font();
            inputViewer.StyleInfo.FontColor = Color.green;

            foreach(var keyCodeText in button.ButtonTexts)
            {
                Assert.AreSame(inputViewer.StyleInfo.Font, keyCodeText.Text.font);
                Assert.AreEqual(inputViewer.StyleInfo.FontColor, keyCodeText.Text.color);
            }
        }
    }
}
