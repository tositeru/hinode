using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// IViewObjectを継承したMonoBehaviour
    ///
    /// Unbind時にこのComponentを持つGameObjectも破棄するようになっています。
    /// 
    /// IViewObjectのインターフェイス変更に対応するために作成しましたので、こちらを継承するようにしてください。
    /// <seealso cref="EmptyViewObject"/>
    /// </summary>
    public class MonoBehaviourViewObject : MonoBehaviour, IViewObject
    {
        public static MonoBehaviourViewObject Create(string name="__monoBehaviourViewObj")
        {
            var gameObj = new GameObject(name);
            return gameObj.AddComponent<MonoBehaviourViewObject>();
        }

        #region Unity Callback
        void OnDestroy()
        {
            Debug.Log($"debug -- destroy {name}");
            IsAlive = false;
        }
        #endregion

        protected virtual void OnDestroyViewObj() { }
        protected virtual void OnBind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap) { }
        protected virtual void OnUnbind() { }

        SmartDelegate<OnViewObjectDestroyed> _onDestroyed = new SmartDelegate<OnViewObjectDestroyed>();
        SmartDelegate<OnViewObjectBinded> _onBinded = new SmartDelegate<OnViewObjectBinded>();
        SmartDelegate<OnViewObjectUnbinded> _onUnbinded = new SmartDelegate<OnViewObjectUnbinded>();

        public bool IsAlive { get; private set; } = true;
        public bool IsVisibility { get => gameObject.activeInHierarchy; set => gameObject.SetActive(value); }

        public virtual Model UseModel { get; set; }
        public virtual ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public virtual ModelViewBinderInstance UseBinderInstance { get; set; }

        public NotInvokableDelegate<OnViewObjectDestroyed> OnDestroyed { get => _onDestroyed; }
        public NotInvokableDelegate<OnViewObjectBinded> OnBinded { get => _onBinded; }
        public NotInvokableDelegate<OnViewObjectUnbinded> OnUnbinded { get => _onUnbinded; }

        /// <summary>
        /// ModelのBindを解除したい時はUnbind()を使用してください。
        /// </summary>
        public void Destroy()
        {
            Unbind();
            _onDestroyed.Instance?.Invoke(this);
            _onDestroyed.Clear();
            OnDestroyViewObj();
            if(IsAlive)
                Destroy(this.gameObject);
            IsAlive = false;
        }

        public virtual object QueryChild(string childID)
        {
            Logger.LogWarning(Logger.Priority.Debug, () => $"Please Override QueryChild(string childID) in SubClass!!");
            return null;
        }

        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Unbind();

            UseModel = targetModel;
            UseBindInfo = bindInfo;
            UseBinderInstance = (binderInstanceMap?.Contains(UseModel) ?? false)
                ? binderInstanceMap[UseModel]
                : null;
            OnBind(targetModel, bindInfo, binderInstanceMap);
            _onBinded.Instance?.Invoke(this);
        }

        /// <summary>
        /// GameObjectを破棄したい時はDestroyを呼び出してください。
        /// </summary>
        public void Unbind()
        {
            if (this.DoBinding())
            {
                _onUnbinded.Instance?.Invoke(this);
                OnUnbind();
            }

            UseModel = null;
            UseBindInfo = null;
            UseBinderInstance = null;
        }

        public virtual void OnViewLayouted()
        { }

        public override string ToString()
        {
            return IViewObjectExtensions.ToString(this);
        }
    }
}
