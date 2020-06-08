using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformAutoViewLayoutObject : UnityAutoViewLayoutObject
        , IRectTransformAnchorXViewLayout
        , IRectTransformAnchorYViewLayout
        , IRectTransformPivotViewLayout
        , IRectTransformSizeViewLayout
        , IRectTransformAnchorMinViewLayout
        , IRectTransformAnchorMaxViewLayout
        , IRectTransformOffsetMinViewLayout
        , IRectTransformOffsetMaxViewLayout
    {
        public class AutoCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
            {
                if (!(viewObj is MonoBehaviour)) return null;
                var behaviour = viewObj as MonoBehaviour;
                var inst = behaviour.gameObject.GetOrAddComponent<RectTransformAutoViewLayoutObject>();
                inst.Attach(viewObj);
                return inst;
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(RectTransformAutoViewLayoutObject).GetInterfaces()
                    .Where(_t => _t.HasInterface<IViewLayout>());
            }
        }

        RectTransform R { get => transform as RectTransform; }

        #region RectTransformViewLayouts
        public Vector2 RectTransformAnchorXLayout
        {
            get => new Vector2(R.anchorMin.x, R.anchorMax.x);
            set
            {
                R.anchorMin = new Vector2(value.x, R.anchorMin.y);
                R.anchorMax = new Vector2(value.y, R.anchorMax.y);
            }
        }
        public Vector2 RectTransformAnchorYLayout
        {
            get => new Vector2(R.anchorMin.y, R.anchorMax.y);
            set
            {
                R.anchorMin = new Vector2(R.anchorMin.x, value.x);
                R.anchorMax = new Vector2(R.anchorMax.x, value.y);
            }
        }
        public Vector2 RectTransformAnchorMinLayout
        {
            get => R.anchorMin;
            set => R.anchorMin = value;
        }
        public Vector2 RectTransformAnchorMaxLayout
        {
            get => R.anchorMax;
            set => R.anchorMax = value;
        }
        public Vector2 RectTransformPivotLayout
        {
            get => R.pivot; set => R.pivot = value;
        }
        public Vector2 RectTransformSizeLayout
        {
            get => R.rect.size;
            set
            {
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
            }
        }
        public Vector2 RectTransformOffsetMinLayout
        {
            get => R.offsetMin;
            set => R.offsetMin = value;
        }
        public Vector2 RectTransformOffsetMaxLayout
        {
            get => R.offsetMax;
            set => R.offsetMax = value;
        }
        #endregion

        #region IAutoViewLayoutObject
        #endregion
    }

    public static partial class ViewLayoutExtensions
    {
        public static ViewLayouter AddRectTransformKeywordsAndAutoCreator(this ViewLayouter target)
        {
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { RectTransformViewLayoutName.anchorX.ToString(), new RectTransformAnchorXViewLayoutAccessor() },
                { RectTransformViewLayoutName.anchorY.ToString(), new RectTransformAnchorYViewLayoutAccessor()},
                { RectTransformViewLayoutName.anchorMin.ToString(), new RectTransformAnchorMinViewLayoutAccessor()},
                { RectTransformViewLayoutName.anchorMax.ToString(), new RectTransformAnchorMaxViewLayoutAccessor()},
                { RectTransformViewLayoutName.pivot.ToString(), new RectTransformPivotViewLayoutAccessor()},
                { RectTransformViewLayoutName.size.ToString(), new RectTransformSizeViewLayoutAccessor()},
                { RectTransformViewLayoutName.offsetMin.ToString(), new RectTransformOffsetMinViewLayoutAccessor() },
                { RectTransformViewLayoutName.offsetMax.ToString(), new RectTransformOffsetMaxViewLayoutAccessor() },
            };
            target.AddKeywords(
                keywords.Select(_t => (_t.Key, _t.Value))
            );
            target.AddAutoCreateViewObject(new RectTransformAutoViewLayoutObject.AutoCreator(), keywords.Keys);
            return target;
        }
    }
}
