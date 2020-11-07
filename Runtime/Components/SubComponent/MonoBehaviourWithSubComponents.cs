using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// SubComponentManagerをラップしたMonoBehaviour
    ///
    /// 継承先のクラスが持つSubComponentの情報を閲覧することができるEditorWindowを提供してます。
    /// Hinode > Tools > SubComponent Summary からそのWindowを開けます。
    /// 
    /// <see cref="ISubComponent{T}"/>
    /// <see cref="SubComponentManager{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoBehaviourWithSubComponents<T> : MonoBehaviour
        , ISubComponent<T>
        where T : MonoBehaviour
    {
        SubComponentManager<T> _subComponents;

        protected SubComponentManager<T> SubComponents { get => _subComponents; }
        public T RootComponent { get; set; }

        public void BindCallbacks(object obj)
        {
            var labelObj = LabelObject.GetLabelObject(obj);
            if (labelObj == null) return;

            var matchingMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(_m => {
                    var attrs = _m.GetCustomAttributes<BindCallbackAttribute>();
                    var useAttr = attrs?.FirstOrDefault(_a => _a.DoMatch(Labels.MatchOp.Included, labelObj.AllLabels))
                        ?? null;
                    return (methodInfo: _m, attr: useAttr);
                })
                .Where(_t => _t.attr != null);

            foreach (var (info, attr) in matchingMethods)
            {
                var com = labelObj.gameObject.GetComponent(attr.CallbackBaseType);
                if (com == null) continue;

                attr.Bind(this, info, com);
            }
        }

        protected virtual void Awake()
        {
            Assert.IsTrue(this is T, $"{this.GetType()} is not {typeof(T)}...");

            _subComponents = new SubComponentManager<T>(this as T);
            _subComponents.Init();
        }

        protected virtual void Start()
        {
            _subComponents.UpdateUI();
        }

        protected virtual void OnDestroy()
        {
            _subComponents.Destroy();
        }

        #region ISubComponent
        public virtual void Init()
        {
            var selfScene = SceneExtensions.GetSceneEnumerable()
                .First(_s => _s.GetRootGameObjects().Any(_o => _o == gameObject));
            foreach (var obj in selfScene.GetGameObjectEnumerable())
            {
                BindCallbacks(obj);
            }
        }
        public virtual void Destroy() { }
        public virtual void UpdateUI() { }
        #endregion
    }
}
