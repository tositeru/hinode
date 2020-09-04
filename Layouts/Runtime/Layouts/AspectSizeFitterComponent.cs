using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// <seealso cref="AspectSizeFitter"/>
    /// </summary>
    [RequireComponent(typeof(LayoutTargetComponent))]
    public class AspectSizeFitterComponent : MonoBehaviour
        , ILayoutComponent
    {
#pragma warning disable CS0649
        [SerializeField] AspectSizeFitter _instance;
#pragma warning restore CS0649

        LayoutTargetComponent _target;
        public LayoutTargetComponent Target
        {
            get
            {
                if(_target == null)
                {
                    _target = GetComponent<LayoutTargetComponent>();
                    LayoutInstance.Target = Target.LayoutTarget;
                }
                return _target;
            }
        }

        public LayoutTargetComponent Parent
        {
            get
            {
                if (transform.parent != null)
                {
                    var parent = transform.parent.gameObject.GetOrAddComponent<LayoutTargetComponent>();
                    _target.LayoutTarget.SetParent(parent.LayoutTarget);
                    return parent;
                }
                else
                {
                    _target.LayoutTarget.SetParent(null);
                    return null;
                }
            }
        }

        private void Awake()
        {
            LayoutInstance.Target = Target.LayoutTarget;

            LayoutInstance.Target.OnDisposed.Add((self) => {
                if (self != LayoutInstance.Target) return;
                LayoutInstance.Target = null;
            });
        }

        private void OnDestroy()
        {
            _instance.Dispose();
        }

        #region ILayoutComponent
        public ILayout LayoutInstance { get => _instance; }
        #endregion
    }
}
