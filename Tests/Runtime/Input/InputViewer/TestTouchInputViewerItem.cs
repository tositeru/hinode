using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input.InputViewers
{
    /// <summary>
	/// <seealso cref="TouchInputViewerItem"/>
	/// </summary>
    public class TestTouchInputViewerItem : TestBase
    {
        (InputViewer, TouchInputViewerItem) CreateTouchItem()
        {
            var inputViewer = InputViewer.CreateInstance();
            inputViewer.UseInput = new ReplayableInput() { IsReplaying = true };
            var touch = inputViewer.gameObject.AddComponent<TouchInputViewerItem>();
            inputViewer.RefleshItems();
            return (inputViewer, touch);
        }

        /// <summary>
		/// <seealso cref="TouchInputViewerItem.DoEnabled"/>
		/// <seealso cref="TouchInputViewerItem.Pointers"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator DoEnabledPasses()
        {
            var (inputViewer, touch) = CreateTouchItem();

            inputViewer.UseInput.RecordedTouchSupported = true;
            inputViewer.UseInput.RecordedTouchCount = 2;
            yield return null;
            Assert.IsTrue(touch.DoEnabled);
            Assert.IsTrue(touch.Pointers.All(_p => _p.gameObject.activeInHierarchy));


            inputViewer.UseInput.RecordedTouchSupported = false;
            yield return null;
            Assert.IsFalse(touch.DoEnabled);
            Assert.IsFalse(touch.Pointers.All(_p => _p.gameObject.activeInHierarchy));
        }

        /// <summary>
		/// <seealso cref="TouchInputViewerItem.PointerCount"/>
		/// <seealso cref="TouchInputViewerItem.Pointers"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator PointerCountPasses()
        {
            var (inputViewer, touch) = CreateTouchItem();
            inputViewer.UseInput.RecordedTouchSupported = true;

            var testData = new int[]
            {
                3, 1, 6
            };

            foreach(var d in testData)
            {
                inputViewer.UseInput.RecordedTouchCount = d;
                yield return null;
                Assert.AreEqual(inputViewer.UseInput.TouchCount, touch.PointerCount);
                Assert.AreEqual(inputViewer.UseInput.TouchCount, touch.Pointers.Count());
            }
        }

        /// <summary>
		/// <seealso cref="TouchInputViewerItem.OnChangedStyle(InputViewerStyleInfo)"/>
		/// </summary>
		/// <returns></returns>
        //[UnityTest]
        public IEnumerator OnChangedStylePasses()
        {
            var (inputViewer, touch) = CreateTouchItem();
            inputViewer.UseInput.RecordedTouchSupported = true;
            inputViewer.UseInput.RecordedTouchCount = 3;
            yield return null;

            inputViewer.StyleInfo.Font = new Font();
            inputViewer.StyleInfo.FontColor = Color.green;
            foreach(var pointer in touch.Pointers)
            {
                Assert.AreEqual(inputViewer.StyleInfo.Font, pointer.IDText.font);
                Assert.AreEqual(inputViewer.StyleInfo.FontColor, pointer.IDText.color);
            }
        }

        /// <summary>
		/// <seealso cref="TouchInputViewerItem.PointerRadius"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator PointerRadiusPasses()
        {
            var (inputViewer, touch) = CreateTouchItem();
            inputViewer.UseInput.RecordedTouchSupported = true;
            inputViewer.UseInput.RecordedTouchCount = 2;
            touch.PointerRadius = 20;
            yield return null;

            foreach(var p in touch.Pointers)
            {
                var touchR = p.transform as RectTransform;
                Assert.AreEqual(touch.PointerRadius, touchR.rect.width);
                Assert.AreEqual(touch.PointerRadius, touchR.rect.height);
            }
        }

        /// <summary>
		/// <seealso cref=""/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator PointerColorPasses()
        {
            var (inputViewer, touch) = CreateTouchItem();
            inputViewer.UseInput.RecordedTouchSupported = true;
            inputViewer.UseInput.RecordedTouchCount = 2;

            inputViewer.StyleInfo.ButtonColorAtFree = Color.red;
            inputViewer.StyleInfo.ButtonColorAtDown = Color.blue;
            inputViewer.StyleInfo.ButtonColorAtPush = Color.green;
            inputViewer.StyleInfo.ButtonColorAtUp = Color.yellow;


            System.Action<TouchPhase> setTouchs = (phase) => {
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    inputViewer.UseInput.SetRecordedTouch(i, new Touch() { fingerId = i, phase = phase });
                }
            };

            {
                setTouchs(TouchPhase.Began);
                yield return null;
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtDown, touch.Pointers[i].PointerImage.color);
                }
            }
            Debug.Log($"Success to Set Cursor Color at TouchPhase.Began!");

            {
                setTouchs(TouchPhase.Moved);
                yield return null;
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtPush, touch.Pointers[i].PointerImage.color);
                }
            }
            Debug.Log($"Success to Set Cursor Color at TouchPhase.Moved!");

            {
                setTouchs(TouchPhase.Stationary);
                yield return null;
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtPush, touch.Pointers[i].PointerImage.color);
                }
            }
            Debug.Log($"Success to Set Cursor Color at TouchPhase.Stationary!");

            {
                setTouchs(TouchPhase.Ended);
                yield return null;
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtUp, touch.Pointers[i].PointerImage.color);
                }
            }
            Debug.Log($"Success to Set Cursor Color at TouchPhase.Ended!");

            {
                setTouchs(TouchPhase.Canceled);
                yield return null;
                for (var i = 0; i < inputViewer.UseInput.TouchCount; ++i)
                {
                    Assert.AreEqual(inputViewer.StyleInfo.ButtonColorAtUp, touch.Pointers[i].PointerImage.color);
                }
            }
            Debug.Log($"Success to Set Cursor Color at TouchPhase.Canceled!");
        }

    }
}
