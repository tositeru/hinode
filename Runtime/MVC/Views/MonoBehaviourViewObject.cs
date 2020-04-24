using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
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
        public virtual Model UseModel { get; set; }
        public virtual ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public virtual ModelViewBinderInstance UseBinderInstance { get; set; }

        public virtual void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
        }
        public virtual void Unbind()
        {
            Destroy(this.gameObject);
        }

        public virtual void OnViewLayouted()
        { }
    }
}
