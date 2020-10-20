using System.Collections;
using System.Collections.Generic;
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
        public virtual void Init() {}
        public virtual void Destroy() {}
        public virtual void UpdateUI() {}
        #endregion
    }
}
