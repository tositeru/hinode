﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hinode
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasViewObject : MonoBehaviour
        , IViewObject
        , IDepthViewLayout
    {
        public static CanvasViewObject Create(string name="canvasViewObj")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            var canvas = obj.AddComponent<Canvas>();
            obj.AddComponent<CanvasScaler>();
            obj.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return obj.AddComponent<CanvasViewObject>();
        }

        public Canvas Canvas { get => GetComponent<Canvas>(); }
        public float DepthLayout
        {
            get => Canvas.sortingOrder;
            set
            {
                Canvas.sortingOrder = (int)value;
                Debug.Log("pass");
            }
        }

        #region IViewObject
        public Model UseModel { get; set; }
        public ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public ModelViewBinderInstance UseBinderInstance { get; set; }

        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
        }

        public void Unbind()
        {
        }
        public void OnViewLayouted() { }
        #endregion

    }
}
