using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Hinode
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [AvailableModelViewParamBinder(typeof(CanvasViewObject.FixedParamBinder))]
    [DisallowMultipleComponent()]
    public class CanvasViewObject : RectTransformViewObject
        , IDepthViewLayout
        , RectTransformViewObject.IOptionalViewObject
    {
        public static new CanvasViewObject Create(string name="canvasViewObj")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<Canvas>();
            obj.AddComponent<CanvasScaler>();
            obj.AddComponent<GraphicRaycaster>();

            return obj.AddComponent<CanvasViewObject>();
        }
        Canvas _canvas;
        public Canvas Canvas { get => _canvas != null ? _canvas : _canvas = gameObject.GetOrAddComponent<Canvas>(); }

        #region IDepthViewLayout
        public float DepthLayout
        {
            get => Canvas.sortingOrder;
            set
            {
                Canvas.sortingOrder = (int)value;
            }
        }
        #endregion

        #region IViewObject
        #endregion

        public new class FixedParamBinder : RectTransformViewObject.FixedParamBinder
        {
            public bool Contains(Params paramType)
                => Contains(paramType.ToString());

            public FixedParamBinder Set(Params param, object value)
                => Set(param.ToString(), value) as FixedParamBinder;
            public FixedParamBinder Set<T>(Params param, T value)
                => Set(param.ToString(), value) as FixedParamBinder;

            public T Get<T>(Params param)
                => (T)Get(param.ToString());

            public FixedParamBinder Delete(Params param)
                => Delete(param.ToString()) as FixedParamBinder;

            protected override void UpdateImpl(Model model, IViewObject viewObj)
            {
                Assert.IsTrue(viewObj is CanvasViewObject, $"ViewObj Type Must be CanvasViewObject... viewObj={viewObj} model={model}");
                var canvas = viewObj as CanvasViewObject;
                var c = canvas.Canvas;
                UpdateParams(c);
            }

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/CanvasViewObject/DefineParamsTemplate.asset
public enum Params
{
RenderMode,
SortingLayerID,
SortingOrder,
WorldCamera,
PlaneDistance,
PixelPerfect,
TargetDisplay,
}

public RenderMode RenderMode { get => Get<RenderMode>(Params.RenderMode); set => Set(Params.RenderMode, value); }
public int SortingLayerID { get => Get<int>(Params.SortingLayerID); set => Set(Params.SortingLayerID, value); }
public int SortingOrder { get => Get<int>(Params.SortingOrder); set => Set(Params.SortingOrder, value); }
public Camera WorldCamera { get => Get<Camera>(Params.WorldCamera); set => Set(Params.WorldCamera, value); }
public float PlaneDistance { get => Get<float>(Params.PlaneDistance); set => Set(Params.PlaneDistance, value); }
public bool PixelPerfect { get => Get<bool>(Params.PixelPerfect); set => Set(Params.PixelPerfect, value); }
public int TargetDisplay { get => Get<int>(Params.TargetDisplay); set => Set(Params.TargetDisplay, value); }

void UpdateParams(Canvas c)
{
if (Contains(Params.RenderMode)) c.renderMode = RenderMode;
if (Contains(Params.SortingLayerID)) c.sortingLayerID = SortingLayerID;
if (Contains(Params.SortingOrder)) c.sortingOrder = SortingOrder;
if (Contains(Params.WorldCamera)) c.worldCamera = WorldCamera;
if (Contains(Params.PlaneDistance)) c.planeDistance = PlaneDistance;
if (Contains(Params.PixelPerfect)) c.pixelPerfect = PixelPerfect;
if (Contains(Params.TargetDisplay)) c.targetDisplay = TargetDisplay;
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/CanvasViewObject/DefineParamsTemplate.asset
        }
    }
}
