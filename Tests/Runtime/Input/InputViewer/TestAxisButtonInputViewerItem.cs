using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="AxisButtonInputViewerItem"/>
	/// </summary>
    public class TestAxisButtonInputViewerItem : TestBase
    {
        (InputViewer, AxisButtonInputViewerItem) CreateAxisItem()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var Axis = inputViewer.gameObject.AddComponent<AxisButtonInputViewerItem>();
            inputViewer.RefleshItems();
            return (inputViewer, Axis);
        }

        /// <summary>
		/// <seealso cref="AxisButtonInputViewerItem.AddObservedAxis"/>
		/// <seealso cref="AxisButtonInputViewerItem.ObservedAxises"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator AddObservedAxisPasses()
        {
            var (inputViewer, Axis) = CreateAxisItem();

            var Axises = new List<string>() { "Horizontal", "Vertical" };
            Axis.AddObservedAxis(Axises);

            AssertionUtils.AssertEnumerableByUnordered(
                Axises
                , Axis.ObservedAxises
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AxisButtonInputViewerItem.RemoveObservedAxis"/>
		/// <seealso cref="AxisButtonInputViewerItem.ObservedAxises"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RemoveObservedAxisPasses()
        {
            var (inputViewer, Axis) = CreateAxisItem();

            var Axises = new List<string>() { "Horizontal", "Vertical" };
            Axis.AddObservedAxis(Axises);
            Axis.RemoveObservedAxis(Axises[1]);

            AssertionUtils.AssertEnumerableByUnordered(
                new string[] { Axises[0] }
                , Axis.ObservedAxises
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="AxisButtonInputViewerItem.AxisLimitPerText"/>
        /// <seealso cref="AxisButtonInputViewerItem.AxisTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AxisTextsPasses()
        {
            var (inputViewer, Axis) = CreateAxisItem();

            Axis.AxisLimitPerText = 3;
            Axis.AddObservedAxis(Enumerable.Range(0, 30).Select(_i => $"Axis{_i}"));
            yield return null; // <- Create and Update AxisText in AxisButtonInputViewerItem#UpdateItem()

            var count = Axis.ObservedAxises.Count / Axis.AxisLimitPerText
                + Mathf.Min(1, Axis.ObservedAxises.Count % Axis.AxisLimitPerText);
            Assert.AreEqual(count, Axis.AxisTexts.Count);
            AssertionUtils.AssertEnumerableByUnordered(
                Axis.AxisTexts.SelectMany(_t => _t.Axises)
                , Axis.ObservedAxises
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="AxisButtonInputViewerItem.AxisLimitPerText"/>
        /// <seealso cref="AxisButtonInputViewerItem.AxisTexts"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AxisLimitPerTextChangedPasses()
        {
            var (inputViewer, Axis) = CreateAxisItem();
            Axis
                .AddObservedAxis(Enumerable.Range(0, 30).Select(_i => $"Fire{_i}"));

            var testData = new int[]
            {
                3,
                12,
                8,
                24,
            };

            foreach(var d in testData)
            {
                Axis.AxisLimitPerText = d;
                yield return null; // <- Create and Update AxisTexts in AxisButtonInputViewerItem#UpdateItem()

                var count = Axis.ObservedAxises.Count / Axis.AxisLimitPerText
                    + Mathf.Min(1, Axis.ObservedAxises.Count % Axis.AxisLimitPerText);
                Assert.AreEqual(count, Axis.AxisTexts.Count);
                AssertionUtils.AssertEnumerableByUnordered(
                    Axis.AxisTexts.SelectMany(_t => _t.Axises)
                    , Axis.ObservedAxises
                    , ""
                );
            }
        }

        /// <summary>
		/// <seealso cref="AxisButtonInputViewerItem.OnChangedStyle(InputViewerStyleInfo)"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator OnChangedStylePasses()
        {
            var (inputViewer, Axis) = CreateAxisItem();
            inputViewer.UseInput.RecordedMousePresent = true;
            yield return null;

            inputViewer.StyleInfo.Font = new Font();
            inputViewer.StyleInfo.FontColor = Color.green;

            foreach(var keyCodeText in Axis.AxisTexts)
            {
                Assert.AreSame(inputViewer.StyleInfo.Font, keyCodeText.Text.font);
                Assert.AreEqual(inputViewer.StyleInfo.FontColor, keyCodeText.Text.color);
            }
        }
    }
}
