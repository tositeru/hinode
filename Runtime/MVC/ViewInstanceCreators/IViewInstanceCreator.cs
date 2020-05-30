using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum ViewObjectCreateType
    {
        Default,
        Cache,
    }

    /// <summary>
    /// IViewObjectとIModelViewParamBinderを作成する抽象クラス
    /// </summary>
    public abstract class IViewInstanceCreator
    {
        protected virtual ViewInstanceCreatorObjectPool CreateObjectPool()
            => new ViewInstanceCreatorObjectPool(this);

        ViewInstanceCreatorObjectPool _objectPool;
        protected ViewInstanceCreatorObjectPool ObjectPool
        {
            get => _objectPool != null ? _objectPool : _objectPool = CreateObjectPool();
        }

        public System.Type GetViewObjType(ModelViewBinder.BindInfo bindInfo)
        {
            var type = GetViewObjTypeImpl(bindInfo.InstanceKey);
            Assert.IsTrue(type.HasInterface<IViewObject>(), $"'{type.FullName}' don't have IViewObejct interface... instanceKey={bindInfo.InstanceKey}");
            return type;
        }

        public System.Type GetParamBinderType(ModelViewBinder.BindInfo bindInfo)
        {
            return GetParamBinder(bindInfo).GetType();
        }

        public IViewObject CreateViewObj(ModelViewBinder.BindInfo bindInfo, bool doForceCreate = false)
        {
            IViewObject viewObj = null;
            if(doForceCreate)
            {
                viewObj = CreateViewObjImpl(bindInfo.InstanceKey);
            }
            else
            {
                switch(bindInfo.ViewObjectCreateType)
                {
                    case ViewObjectCreateType.Default:
                        viewObj = CreateViewObjImpl(bindInfo.InstanceKey);
                        break;
                    case ViewObjectCreateType.Cache:
                        viewObj = ObjectPool.PopOrCreate(bindInfo);
                        break;
                    default:
                        throw new System.NotImplementedException($"CreateType=>{bindInfo.ViewObjectCreateType}");
                }
            }
            Assert.IsNotNull(viewObj, $"Failed to create ViewObject because don't match ViewObject Key({bindInfo.InstanceKey})...");
            viewObj.UseBindInfo = bindInfo;
            return viewObj;
        }

        public IModelViewParamBinder GetParamBinder(ModelViewBinder.BindInfo bindInfo)
        {
            if(bindInfo.UseParamBinder != null)
            {
                return bindInfo.UseParamBinder;
            }
            else
            {
                var paramBinder = GetParamBinderImpl(bindInfo.BinderKey);
                Assert.IsNotNull(paramBinder, $"Failed to create IModelViewParamBinder because don't match Binder Key({bindInfo.BinderKey})...");
                return paramBinder;
            }
        }

        protected abstract System.Type GetViewObjTypeImpl(string instanceKey);
        protected abstract IViewObject CreateViewObjImpl(string instanceKey);
        protected abstract IModelViewParamBinder GetParamBinderImpl(string binderKey);
    }
}
