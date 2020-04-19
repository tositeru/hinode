using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    [RequireComponent(typeof(RectTransform))]
    class RectTransformViewLayoutAccessor : MonoBehaviour
        , IViewObject
        , IRectTransformAncherXViewLayout
    {
        public class AutoCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IViewObject CreateImpl(IViewObject viewObj)
            {
                if (!(viewObj is MonoBehaviour)) return null;
                var behaviour = viewObj as MonoBehaviour;
                if (!(behaviour.transform is RectTransform))return null;
                return behaviour.gameObject.GetOrAddComponent<RectTransformViewLayoutAccessor>();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(RectTransformViewLayoutAccessor).GetInterfaces()
                    .Where(_t => _t.DoHasInterface<IViewLayout>());
            }
        }

        public Vector2 RectTransformAnchorXLayout { get; set; }

        #region IViewObject
        public Model UseModel { get; set; }
        public ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        { }
        public void Unbind()
        {
            Destroy(this);
        }
        #endregion
    }
}
