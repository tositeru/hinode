using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// OnPointerEventDispatcherのためにオブジェクトの当たり判定を表すMonoBehaviour
    ///
    /// 
    /// <seealso cref="IOnPointerEventControllerObject"/>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public class OnPointerEventControllerMonoBehaivour : MonoBehaviour
        , IOnPointerEventControllerObject
    {
        #region IOnPointerEventControllerObject interface
        public bool IsScreenOverlay
        {
            get
            {
                if (!(transform is RectTransform)) return false;
                return RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
            }
        }

        public Canvas RootCanvas
        {
            get
            {
                if (!(transform is RectTransform)) return null;
                if(transform.parent != null)
                {
                    var rootCanvas = transform.GetParentEnumerable()
                        .Select(_p => _p.GetComponent<Canvas>())
                        .Where(_c => _c != null)
                        .LastOrDefault();
                    return rootCanvas;
                }
                else
                {
                    return transform.GetComponent<Canvas>();
                }
            }
        }

        public Transform Transform { get => transform; }

        public bool IsOnPointer(Vector3 screenPos, Camera useCamera)
        {
            if(IsScreenOverlay)
            {
                var R = transform as RectTransform;
                //var parentTransform = transform.parent == null
                //    ? transform
                //    : transform.parent;
                var localPos = R.worldToLocalMatrix.MultiplyPoint3x4(screenPos);
                return R.rect.Overlaps(localPos);
            }
            else
            {
                throw new System.NotImplementedException("このクラスによって自動的に追加されるColliderとのレイキャストで判定する予定");
            }
        }

        public void Destroy()
        {
            Destroy(this);
        }
        #endregion
    }
}
