using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformViewLayoutAccessor : MonoBehaviourViewObject
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
            protected override IViewObject CreateImpl(IViewObject viewObj)
            {
                if (!(viewObj is MonoBehaviour)) return null;
                var behaviour = viewObj as MonoBehaviour;
                if (!(behaviour.transform is RectTransform)) return null;
                return behaviour.gameObject.GetOrAddComponent<RectTransformViewLayoutAccessor>();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(RectTransformViewLayoutAccessor).GetInterfaces()
                    .Where(_t => _t.DoHasInterface<IViewLayout>());
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

        #region IViewObject
        #endregion
    }

    public static partial class ViewLayoutExtensions
    {
        public static ViewLayouter AddRectTransformKeywordsAndAutoCreator(this ViewLayouter target)
        {
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { "anchorX", new RectTransformAnchorXViewLayoutAccessor() },
                { "anchorY", new RectTransformAnchorYViewLayoutAccessor()},
                { "anchorMin", new RectTransformAnchorMinViewLayoutAccessor()},
                { "anchorMax", new RectTransformAnchorMaxViewLayoutAccessor()},
                { "pivot", new RectTransformPivotViewLayoutAccessor()},
                { "size", new RectTransformSizeViewLayoutAccessor()},
                { "offsetMin", new RectTransformOffsetMinViewLayoutAccessor() },
                { "offsetMax", new RectTransformOffsetMaxViewLayoutAccessor() },
            };
            target.AddKeywords(
                keywords.Select(_t => (_t.Key, _t.Value))
            );
            target.AddAutoCreateViewObject(new RectTransformViewLayoutAccessor.AutoCreator(), keywords.Keys);
            return target;
        }
    }
}
