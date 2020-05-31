using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Linq;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Views
{
    /// <summary>
    /// <seealso cref="CanvasViewObject"/>
    /// </summary>
    public class TestCanvasViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator FixedParamBinderUpdatePasses()
        {
            var canvas = CanvasViewObject.Create();
            var cameraObj = new GameObject();
            var camera = cameraObj.AddComponent<Camera>();
            yield return null;
            {//RenderMode
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.RenderMode = canvas.Canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? RenderMode.ScreenSpaceOverlay
                    : RenderMode.WorldSpace;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.RenderMode, canvas.Canvas.renderMode);
            }
            Debug.LogWarning($"SortingLayerIDをテストするにはSortingLayerを設定する必要があるので今はテストしていません。");
            if (false)
            {//SortingLayerID
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.SortingLayerID = 0;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.SortingLayerID, canvas.Canvas.sortingLayerID);
            }
            {//SortingOrder
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.SortingOrder = 124;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.SortingOrder, canvas.Canvas.sortingOrder);
            }
            {//WorldCamera
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.WorldCamera = camera;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.WorldCamera, canvas.Canvas.worldCamera);
            }
            {//PlaneDistance
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.PlaneDistance = 654f;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.PlaneDistance, canvas.Canvas.planeDistance);
            }
            {//PixelPerfect
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.PixelPerfect = !canvas.Canvas.pixelPerfect;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.PixelPerfect, canvas.Canvas.pixelPerfect);
            }
            {//TargetDisplay
                var paramBinder = new CanvasViewObject.FixedParamBinder();
                paramBinder.TargetDisplay = canvas.Canvas.targetDisplay + 1;
                paramBinder.Update(null, canvas);

                Assert.AreEqual(paramBinder.TargetDisplay, canvas.Canvas.targetDisplay);
            }
        }

        [UnityTest, Description("DepthLayoutがCanvas#sortingOrderと連帯しているかのテスト")]
        public IEnumerator DepthLayoutPasses()
        {
            var canvas = CanvasViewObject.Create();
            yield return null;
            var depth = 542f;
            canvas.DepthLayout = depth;
            Assert.AreEqual(depth, canvas.Canvas.sortingOrder);
        }
    }
}