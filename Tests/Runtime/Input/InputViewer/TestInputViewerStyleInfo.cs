using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="InputViewerStyleInfo"/>
	/// </summary>
    public class TestInputViewerStyleInfo
    {
        /// <summary>
		/// <seealso cref="InputViewerStyleInfo.OnChanged"/>
		/// <seealso cref="InputViewerStyleInfo.Font"/>
		/// </summary>
        [Test]
        public void OnChangedDelegateByFontPasses()
        {
            var styleInfo = new InputViewerStyleInfo();

            Font recievedFont = null;
            styleInfo.OnChanged.Add(info => {
                Assert.AreSame(styleInfo, info);
                recievedFont = info.Font;
            });

            var font = new Font();
            styleInfo.Font = new Font();
            Assert.AreSame(styleInfo.Font, recievedFont);
        }

        /// <summary>
        /// <seealso cref="InputViewerStyleInfo.OnChanged"/>
        /// <seealso cref="InputViewerStyleInfo.FontColor"/>
        /// </summary>
        [Test]
        public void OnChangedDelegateByFontColorPasses()
        {
            var styleInfo = new InputViewerStyleInfo();

            Color recievedFontColor = default(Color);
            styleInfo.OnChanged.Add(info => {
                Assert.AreSame(styleInfo, info);
                recievedFontColor = info.FontColor;
            });

            var font = new Font();
            styleInfo.FontColor = Color.green;
            Assert.AreEqual(styleInfo.FontColor, recievedFontColor);
        }

        /// <summary>
		/// <seealso cref="InputViewerStyleInfo.OnChanged"/>
		/// <seealso cref="InputViewerStyleInfo.ButtonColorAtFree"/>
		/// <seealso cref="InputViewerStyleInfo.ButtonColorAtDown"/>
		/// <seealso cref="InputViewerStyleInfo.ButtonColorAtPush"/>
		/// <seealso cref="InputViewerStyleInfo.ButtonColorAtUp"/>
		/// </summary>
        [Test]
        public void OnChangedDelegateByButtonColorPasses()
        {
            var styleInfo = new InputViewerStyleInfo();

            Color[] recievedColors = Enumerable.Range(0, 4).Select(_ => default(Color)).ToArray();
            int recievedCount = 0;
            styleInfo.OnChanged.Add(info => {
                Assert.AreSame(styleInfo, info);
                recievedCount++;
                recievedColors[0] = info.ButtonColorAtFree;
                recievedColors[1] = info.ButtonColorAtDown;
                recievedColors[2] = info.ButtonColorAtPush;
                recievedColors[3] = info.ButtonColorAtUp;
            });

            {
                styleInfo.ButtonColorAtFree = Color.red;
                Assert.AreEqual(styleInfo.ButtonColorAtFree, recievedColors[0]);
            }
            Debug.Log($"Success to Change ButtonColorAtFree!");

            {
                styleInfo.ButtonColorAtDown = Color.blue;
                Assert.AreEqual(styleInfo.ButtonColorAtDown, recievedColors[1]);
            }
            Debug.Log($"Success to Change ButtonColorAtDown!");

            {
                styleInfo.ButtonColorAtPush = Color.green;
                Assert.AreEqual(styleInfo.ButtonColorAtPush, recievedColors[2]);
            }
            Debug.Log($"Success to Change ButtonColorAtPush!");

            {
                styleInfo.ButtonColorAtUp = Color.yellow;
                Assert.AreEqual(styleInfo.ButtonColorAtUp, recievedColors[3]);
            }
            Debug.Log($"Success to Change ButtonColorAtUp!");

            Assert.AreEqual(4, recievedCount);
            Debug.Log($"Success to Call Count Of OnChangedStyleInfo!");
        }

        /// <summary>
		/// <seealso cref="InputViewerStyleInfo.GetButtonCondition(InputDefines.ButtonCondition)"/>
		/// </summary>
        [Test]
        public void GetButtonConditionPasses()
        {
            var styleInfo = new InputViewerStyleInfo();

            styleInfo.ButtonColorAtFree = Color.red;
            styleInfo.ButtonColorAtDown = Color.blue;
            styleInfo.ButtonColorAtPush = Color.green;
            styleInfo.ButtonColorAtUp = Color.yellow;

            Assert.AreEqual(styleInfo.ButtonColorAtFree, styleInfo.GetButtonCondition(InputDefines.ButtonCondition.Free));
            Assert.AreEqual(styleInfo.ButtonColorAtDown, styleInfo.GetButtonCondition(InputDefines.ButtonCondition.Down));
            Assert.AreEqual(styleInfo.ButtonColorAtPush, styleInfo.GetButtonCondition(InputDefines.ButtonCondition.Push));
            Assert.AreEqual(styleInfo.ButtonColorAtUp, styleInfo.GetButtonCondition(InputDefines.ButtonCondition.Up));
        }
    }
}
