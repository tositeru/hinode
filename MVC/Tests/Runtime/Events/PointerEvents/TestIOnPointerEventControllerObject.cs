using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Events.Pointer
{
    /// <summary>
	/// <seealso cref="TestIOnPointerEventControllerObject"/>
	/// </summary>
    public class TestIOnPointerEventControllerObject
    {
        [UnityTest]
        public IEnumerator RootCanvasPasses()
        {
            yield return null;
            var rootCanvas = CanvasViewObject.Create("rootCanvas");
            var parentCanvas = CanvasViewObject.Create("parentCanvas");
            parentCanvas.transform.SetParent(rootCanvas.transform);
            var obj = RectTransformViewObject.Create("obj")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            obj.transform.SetParent(parentCanvas.transform);

            Assert.AreSame(rootCanvas.GetComponent<Canvas>(), obj.RootCanvas);
        }

        [UnityTest]
        public IEnumerator ComparerPasses()
        {
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = Vector3.zero;
            var nearestCameraDistance = 10f;
            var middleCameraDistance = 20f;
            var farestCameraDistance = 100f;

            var nearestOverlayCanvas = CanvasViewObject.Create("nearestScreenOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            nearestOverlayCanvas.RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            nearestOverlayCanvas.RootCanvas.sortingOrder = 100;

            var overlayCanvas = CanvasViewObject.Create("screenOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            overlayCanvas.RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var childInOverlayCanvas = CanvasViewObject.Create("childInScreenOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            childInOverlayCanvas.transform.SetParent(overlayCanvas.transform);

            var child2InOverlayCanvas = CanvasViewObject.Create("child2InScreenOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            child2InOverlayCanvas.transform.SetParent(overlayCanvas.transform);

            var cameraOverlayCanvas = CanvasViewObject.Create("cameraOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            cameraOverlayCanvas.RootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            cameraOverlayCanvas.RootCanvas.worldCamera = mainCamera;
            cameraOverlayCanvas.RootCanvas.planeDistance = nearestCameraDistance;

            var childInCameraOverlayCanvas = CanvasViewObject.Create("childInCameraOverlayCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            childInCameraOverlayCanvas.transform.SetParent(cameraOverlayCanvas.Transform);

            var worldCanvas = CanvasViewObject.Create("worldCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            worldCanvas.RootCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.RootCanvas.worldCamera = mainCamera;
            worldCanvas.Transform.position = mainCamera.transform.position
                + mainCamera.transform.forward * middleCameraDistance;

            var childInWorldCanvas = CanvasViewObject.Create("childInWorldCanvas")
                .gameObject.AddComponent<OnPointerEventControllerMonoBehaivour>();
            childInWorldCanvas.Transform.SetParent(worldCanvas.Transform);

            var cubeObj3D = GameObject.CreatePrimitive(PrimitiveType.Cube)
                .AddComponent<OnPointerEventControllerMonoBehaivour>();
            cubeObj3D.Transform.position = mainCamera.transform.position
                + mainCamera.transform.forward * farestCameraDistance;
            yield return null;

            var list = new OnPointerEventControllerMonoBehaivour[]
            {
                childInWorldCanvas,
                cameraOverlayCanvas,
                overlayCanvas,
                worldCanvas,
                childInOverlayCanvas,
                childInCameraOverlayCanvas,
                child2InOverlayCanvas,
                cubeObj3D,
                nearestOverlayCanvas,
            };

            var comparer = new IOnPointerEventControllerObjectComparer(mainCamera);
            var sortedList = list.OrderBy(_c => _c, comparer);
            Debug.Log($"sorted result -> {sortedList.Select(_o => _o.name).Aggregate("", (_s, _c) => _s + _c + " : ")}");
            AssertionUtils.AssertEnumerable(
                new OnPointerEventControllerMonoBehaivour[]{
                nearestOverlayCanvas,
                child2InOverlayCanvas,
                childInOverlayCanvas,
                overlayCanvas,
                childInCameraOverlayCanvas,
                cameraOverlayCanvas,
                childInWorldCanvas,
                worldCanvas,
                cubeObj3D,
                }, sortedList, "");

        }
    }
}
