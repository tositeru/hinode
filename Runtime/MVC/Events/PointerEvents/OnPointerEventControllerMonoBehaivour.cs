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
    /// <seealso cref="IOnPointerEventHelpObject"/>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public class OnPointerEventControllerMonoBehaivour : MonoBehaviour
        , IOnPointerEventHelpObject
    {
        BoxCollider _autoBoxCollider;

        bool HasAutoBoxCollider { get => _autoBoxCollider != null; }

        void Awake()
        {
            if(!IsScreenOverlay && !TryGetComponent<Collider>(out var _))
            {
                _autoBoxCollider = gameObject.AddComponent<BoxCollider>();
            }
            ResizeAutoBoxCollider();
        }

        void Update()
        {
            ResizeAutoBoxCollider();
        }

        void ResizeAutoBoxCollider()
        {
            if (IsScreenOverlay || _autoBoxCollider == null) return;

            if (RootCanvas == null)
            {
                if(TryGetComponent<Renderer>(out var renderer))
                {
                    _autoBoxCollider.size = renderer.bounds.center;
                    _autoBoxCollider.size = renderer.bounds.size;
                }
            }
            else if(_autoBoxCollider != null)
            {
                var R = Transform as RectTransform;
                _autoBoxCollider.size = R.rect.size;
                _autoBoxCollider.center = R.rect.size * (R.pivot - Vector2.one * 0.5f) * -1f;
            }
        }

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
