﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// UnityのMonoBehaviour用のViewInstanceCreatorObjectPool
    ///
    /// 複数のViewObject型をPoolingできるようになっています。
    /// </summary>
    public class UnityViewInstanceCreatorObjectPool : ViewInstanceCreatorObjectPool
    {
        Transform _poolingObjParent = null;
        public Transform PoolingObjParent
        {
            get
            {
                if(_poolingObjParent == null)
                {
                    _poolingObjParent = new GameObject("__poolingViewObjParent").transform;
                    Object.DontDestroyOnLoad(_poolingObjParent);
                }
                return _poolingObjParent;
            }
        }

        public UnityViewInstanceCreatorObjectPool(UnityViewInstanceCreator creator)
            : base(creator)
        { }

        #region ViewInstanceCreatorObjectPool
        protected override void OnPopOrCreated(ModelViewBinder.BindInfo bindInfo, IViewObject viewObj)
        {
            if(viewObj is MonoBehaviour)
            {
                var behaviour = viewObj as MonoBehaviour;
                behaviour.gameObject.SetActive(true);
                behaviour.transform.SetParent(null);
            }
        }

        protected override void OnPushed(IViewObject viewObj)
        {
            if(viewObj is MonoBehaviourViewObject)
            {
                var behaviour = viewObj as MonoBehaviourViewObject;
                if(behaviour.IsAlive)
                {
                    behaviour.gameObject.SetActive(false);
                    behaviour.transform.SetParent(PoolingObjParent);
                }
            }
            else if (viewObj is MonoBehaviour)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Please Use MonoBehaviourViewObject instead of MonoBehaviour, Because GameObject was destroyed...");
                var behaviour = viewObj as MonoBehaviour;
                behaviour.gameObject.SetActive(false);
                behaviour.transform.SetParent(PoolingObjParent);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if(PoolingObjParent != null)
            {
                Object.Destroy(PoolingObjParent.gameObject);
            }
        }
        #endregion
    }
}
