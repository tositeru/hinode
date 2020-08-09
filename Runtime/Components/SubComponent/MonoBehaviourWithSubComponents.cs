using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// SubComponentManagerをラップしたMonoBehaviour
    /// 
    /// <see cref="ISubComponent{T}"/>
    /// <see cref="SubComponentManager{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoBehaviourWithSubComponents<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        SubComponentManager<T> _subComponents;

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
    }
}
