using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// IViewObjectとIModelViewParamBinderを作成する抽象クラス
    /// </summary>
    public abstract class IViewInstanceCreator
    {
        public System.Type GetViewObjType(ModelViewBinder.BindInfo bindInfo)
        {
            var type = GetViewObjTypeImpl(bindInfo.InstanceKey);
            Assert.IsTrue(type.DoHasInterface<IViewObject>(), $"'{type.FullName}' don't have IViewObejct interface... instanceKey={bindInfo.InstanceKey}");
            return type;
        }

        public System.Type GetParamBinderType(ModelViewBinder.BindInfo bindInfo)
        {
            return GetParamBinder(bindInfo).GetType();
        }

        public IViewObject CreateViewObj(ModelViewBinder.BindInfo bindInfo)
        {
            var viewObj = CreateViewObjImpl(bindInfo.InstanceKey);
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
