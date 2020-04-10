using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// Prefabを使用するViewオブジェクトのバインダ
    /// </summary>
    public class PrefabBindInfo<T> : ModelViewBinder.IBindInfo
        where T : Component, IViewObject
    {
        T UsePrefab { get; }

        public PrefabBindInfo(T prefab, IModelViewParamBinder binder)
        {
            UsePrefab = prefab;
            ParamBinder = binder;
            Assert.IsTrue(UsePrefab.GetType().DoHasInterface(typeof(IViewObject)));
        }

        #region ModelViewBinder.IBindInfo interface
        public IModelViewParamBinder ParamBinder { get; }

        public IViewObject CreateViewObject()
        {
            return Object.Instantiate(UsePrefab);
        }
        #endregion
    }
}
