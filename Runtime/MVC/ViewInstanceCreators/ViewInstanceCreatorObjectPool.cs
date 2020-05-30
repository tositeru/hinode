using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// IViewInstanceCreator用のObjectPool
    ///
    /// 複数のViewObject型をPoolingできるようになっています。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewInstanceCreatorObjectPool : IObjectPool, System.IDisposable
    {
        protected virtual void OnPopOrCreated(ModelViewBinder.BindInfo bindInfo, IViewObject viewObj) { }
        protected virtual void OnPushed(IViewObject viewObj) { }

        Dictionary<System.Type, ViewObjectPool> _viewObjectPoolDict = new Dictionary<System.Type, ViewObjectPool>();

        public IViewInstanceCreator UseCreator { get; private set; }

        public ViewInstanceCreatorObjectPool(IViewInstanceCreator creator)
        {
            Assert.IsNotNull(creator);
            UseCreator = creator;
        }

        public IViewObject PopOrCreate(ModelViewBinder.BindInfo bindInfo)
        {
            var viewType = UseCreator.GetViewObjType(bindInfo);
            AddDictIfNotContains(viewType);

            var viewObj = _viewObjectPoolDict[viewType].PopOrCreate(UseCreator, bindInfo);

            OnPopOrCreated(bindInfo, viewObj);
            return viewObj;
        }

        public void Push(IViewObject obj)
        {
            AddDictIfNotContains(obj.GetType());

            _viewObjectPoolDict[obj.GetType()].Push(obj);
            OnPushed(obj);
        }

        void AddDictIfNotContains(System.Type viewType)
        {
            if (!_viewObjectPoolDict.ContainsKey(viewType))
            {
                _viewObjectPoolDict.Add(viewType, new ViewObjectPool(this, viewType));
            }
        }
        #region System.IDisposable interface
        public virtual void Dispose()
        {
            foreach(var pool in _viewObjectPoolDict.Values)
            {
                pool.Dispose();
            }
            _viewObjectPoolDict.Clear();
            UseCreator = null;
        }
        #endregion

        class ViewObjectPool : IObjectPool, System.IDisposable
        {
            public System.Type ViewType { get; }
            public ViewInstanceCreatorObjectPool ParentPool { get; private set; }
            public ViewObjectPool(ViewInstanceCreatorObjectPool pool, System.Type type)
            {
                Assert.IsNotNull(pool);
                Assert.IsTrue(type.HasInterface<IViewObject>());

                ParentPool = pool;
                ViewType = type;
            }

            public IViewObject PopOrCreate(IViewInstanceCreator creator, ModelViewBinder.BindInfo bindInfo)
            {
                var viewObj = Pop() as IViewObject;
                if(viewObj == null)
                {
                    viewObj = creator.CreateViewObj(bindInfo, true);
                }
                viewObj.OnUnbinded.Add(OnViewObjectUnbinded);
                return viewObj;
            }

            public void Push(IViewObject viewObject)
            {
                if (viewObject == null) return;

                Assert.IsTrue(viewObject.GetType().Equals(ViewType));
                //
                viewObject.OnDestroyed.Remove(Remove);
                viewObject.OnDestroyed.Add(Remove);
                viewObject.OnUnbinded.Remove(OnViewObjectUnbinded);

                base.Push(viewObject);
            }

            void OnViewObjectUnbinded(IViewObject viewObject)
            {
                ParentPool.Push(viewObject);
            }

            #region System.IDisposable interface
            public void Dispose()
            {
                foreach(var viewObj in Pool.OfType<IViewObject>())
                {
                    viewObj.OnUnbinded.Remove(OnViewObjectUnbinded);
                    viewObj.OnDestroyed.Remove(Remove);
                    viewObj.Destroy();
                }
                Clear();
                ParentPool = null;
            }
            #endregion
        }
    }
}
