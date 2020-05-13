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
                var R = transform as RectTransform;
                var rootCanvas = transform.GetParentEnumerable()
                    .Select(_p => _p.GetComponent<Canvas>())
                    .Where(_c => _c != null)
                    .LastOrDefault();
                return rootCanvas;
            }
        }

        public Transform Transform { get => transform; }

        public bool IsOnPointer(Vector3 screenPos, Camera useCamera)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
