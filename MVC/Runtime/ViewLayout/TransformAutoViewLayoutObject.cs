using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Hinode.MVC
{
    [RequireComponent(typeof(Transform))]
    public class TransformAutoViewLayoutObject : UnityAutoViewLayoutObject
        , ITransformParentViewLayout
        , ITransformPosViewLayout
        , ITransformRotateViewLayout
        , ITransformLocalPosViewLayout
        , ITransformLocalRotateViewLayout
        , ITransformLocalScaleViewLayout
    {
        public class AutoCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
            {
                if (!(viewObj is MonoBehaviour)) return null;
                var behaviour = viewObj as MonoBehaviour;
                var inst = behaviour.gameObject.GetOrAddComponent<TransformAutoViewLayoutObject>();
                return inst;
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(TransformAutoViewLayoutObject).GetInterfaces()
                    .Where(_t => _t.HasInterface<IViewLayout>());
            }
        }

        Transform R { get => transform as Transform; }

        #region TransformViewLayouts
        public Transform TransformParentLayout
        {
            get => transform.parent;
            set
            {
                transform.SetParent(value);
                //TODO UnityEngine.UIのLayout Systemの都合上レイアウト計算が一フレーム遅れるケースがあるので、自前のLayout Systemを作る
            }
        }
        public Transform SelfTransform { get => transform; }

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

        #region IAutoViewLayoutObject interface
        #endregion
    }

    public static partial class ViewLayoutExtensions
    {
        public static ViewLayouter AddTransformKeywordsAndAutoCreator(this ViewLayouter target)
        {
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { TransformViewLayoutName.parent.ToString(), new TransformParentViewLayoutAccessor() },
                { TransformViewLayoutName.pos.ToString(), new TransformPosViewLayoutAccessor()},
                { TransformViewLayoutName.rotate.ToString(), new TransformRotateViewLayoutAccessor()},
                { TransformViewLayoutName.localPos.ToString(), new TransformLocalPosViewLayoutAccessor()},
                { TransformViewLayoutName.localRotate.ToString(), new TransformLocalRotateViewLayoutAccessor()},
                { TransformViewLayoutName.localScale.ToString(), new TransformLocalScaleViewLayoutAccessor()},
            };
            target.AddKeywords(
                keywords.Select(_t => (_t.Key, _t.Value))
            );
            target.AddAutoCreateViewObject(new TransformAutoViewLayoutObject.AutoCreator(), keywords.Keys);
            return target;
        }
    }
}
