using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformViewLayoutAccessor : MonoBehaviour
        , IViewObject
        , IRectTransformAnchorXViewLayout
        , IRectTransformAnchorYViewLayout
        , IRectTransformPivotViewLayout
        , IRectTransformSizeViewLayout
        , IRectTransformAnchorMinViewLayout
        , IRectTransformAnchorMaxViewLayout
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

        public static ViewLayouter AddKeywordsAndAutoCreator(ViewLayouter layouter)
        {
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { "anchorX", new RectTransformAnchorXViewLayoutAccessor() },
                { "anchorY", new RectTransformAnchorYViewLayoutAccessor()},
                { "anchorMin", new RectTransformAnchorMinViewLayoutAccessor()},
                { "anchorMax", new RectTransformAnchorMaxViewLayoutAccessor()},
                { "pivot", new RectTransformPivotViewLayoutAccessor()},
                { "size", new RectTransformSizeViewLayoutAccessor()},
            };
            layouter.AddKeywords(
                keywords.Select(_t => (_t.Key, _t.Value))
            );
            layouter.AddAutoCreateViewObject(new AutoCreator(), keywords.Keys);
            return layouter;
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
                R.anchorMin = new Vector2(R.anchorMin.x, value.y);
                R.anchorMax = new Vector2(R.anchorMax.y, value.y);
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
