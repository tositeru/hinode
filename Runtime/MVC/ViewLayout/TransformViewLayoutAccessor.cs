﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    [RequireComponent(typeof(Transform))]
    public class TransformViewLayoutAccessor : MonoBehaviour
        , IViewObject
        , ITransformParentViewLayout
        , ITransformPosViewLayout
        , ITransformRotateViewLayout
        , ITransformLocalPosViewLayout
        , ITransformLocalRotateViewLayout
        , ITransformLocalScaleViewLayout
    {
        public class AutoCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IViewObject CreateImpl(IViewObject viewObj)
            {
                if (!(viewObj is MonoBehaviour)) return null;
                var behaviour = viewObj as MonoBehaviour;
                if (!(behaviour.transform is Transform)) return null;
                return behaviour.gameObject.GetOrAddComponent<TransformViewLayoutAccessor>();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(TransformViewLayoutAccessor).GetInterfaces()
                    .Where(_t => _t.DoHasInterface<IViewLayout>());
            }
        }

        public static ViewLayouter AddKeywordsAndAutoCreator(ViewLayouter layouter)
        {
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { "parent", new TransformParentViewLayoutAccessor() },
                { "pos", new TransformPosViewLayoutAccessor()},
                { "rotate", new TransformRotateViewLayoutAccessor()},
                { "localPos", new TransformLocalPosViewLayoutAccessor()},
                { "localRotate", new TransformLocalRotateViewLayoutAccessor()},
                { "localScale", new TransformLocalScaleViewLayoutAccessor()},
            };
            layouter.AddKeywords(
                keywords.Select(_t => (_t.Key, _t.Value))
            );
            layouter.AddAutoCreateViewObject(new AutoCreator(), keywords.Keys);
            return layouter;
        }

        Transform R { get => transform as Transform; }

        #region TransformViewLayouts
        public Transform TransformParentLayout
        {
            get => transform.parent;
            set
            {
                transform.SetParent(value);
            }
        }
        public Transform SelfTransform { get => transform; }
        public RecieverSelector TransformParentLayoutSelector { get; set; }

        public Vector3 TransformPosLayout
        {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 TransformRotateLayout
        {
            get => transform.rotation.eulerAngles;
            set => transform.rotation = Quaternion.Euler(value);
        }
        public Vector3 TransformLocalPosLayout
        {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }
        public Vector3 TransformLocalRotateLayout
        {
            get => transform.localRotation.eulerAngles;
            set => transform.localRotation = Quaternion.Euler(value);
        }
        public Vector3 TransformLocalScaleLayout
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }
        #endregion

        #region IViewObject
        public Model UseModel { get; set; }
        public ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public ModelViewBinderInstance UseBinderInstance { get; set; }
        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        { }
        public void Unbind()
        {
            Destroy(this);
        }
        #endregion
    }
}
