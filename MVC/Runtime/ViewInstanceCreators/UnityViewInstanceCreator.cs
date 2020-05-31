using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// 
    /// </summary>
    public class UnityViewInstanceCreator : IViewInstanceCreator
    {
        interface IInstanceCreator
        {
            bool IsValid { get; }
            System.Type GetViewType();
            IViewObject Create();
        }
        Dictionary<string, IInstanceCreator> _viewObjectDict = new Dictionary<string, IInstanceCreator>();
        Dictionary<string, IModelViewParamBinder> _paramBinderDict = new Dictionary<string, IModelViewParamBinder>();

        public Transform PoolingViewObjParent
        {
            get => (ObjectPool as UnityViewInstanceCreatorObjectPool).PoolingObjParent;
        }

        public UnityViewInstanceCreator() { }

        public UnityViewInstanceCreator AddPrefab<T>(T prefab, IModelViewParamBinder paramBinder)
            where T : Component, IViewObject
            => Add(typeof(T).FullName, new PrefabCreator(typeof(T), prefab), typeof(T).FullName, paramBinder);

        public UnityViewInstanceCreator AddPrefab(System.Type prefabType, Component prefab, IModelViewParamBinder paramBinder)
            => Add(prefabType.FullName, new PrefabCreator(prefabType, prefab), prefabType.FullName, paramBinder);

        public UnityViewInstanceCreator AddPrefab(string instanceKey, System.Type prefabType, Component prefab, string binderKey, IModelViewParamBinder paramBinder)
            => Add(instanceKey, new PrefabCreator(prefabType, prefab), binderKey, paramBinder);

        public UnityViewInstanceCreator AddPrefab<T>(string instanceKey, T prefab, string binderKey, IModelViewParamBinder paramBinder)
            where T : Component, IViewObject
            => Add(instanceKey, new PrefabCreator(typeof(T), prefab), binderKey, paramBinder);

        public UnityViewInstanceCreator AddPredicate(System.Type viewType, System.Func<IViewObject> predicate, IModelViewParamBinder paramBinder)
            => Add(viewType.FullName, new PredicateCreator(viewType, predicate), viewType.FullName, paramBinder);
        public UnityViewInstanceCreator AddPredicate(System.Type viewType, string instanceKey, System.Func<IViewObject> predicate, string binderKey, IModelViewParamBinder paramBinder)
            => Add(instanceKey, new PredicateCreator(viewType, predicate), binderKey, paramBinder);

        UnityViewInstanceCreator Add(string instanceKey, IInstanceCreator creator, string binderKey, IModelViewParamBinder paramBinder)
        {
            if (_viewObjectDict.ContainsKey(instanceKey))
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Not add Because already exist instanceKey({instanceKey})...");
            }
            if(!(creator?.IsValid ?? false))
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Not add Because Invalid InstanceCreator... instanceKey({instanceKey})...");
            }
            else
            {
                _viewObjectDict.Add(instanceKey, creator);
            }

            if (!_paramBinderDict.ContainsKey(binderKey))
            {
                _paramBinderDict.Add(binderKey, paramBinder);
            }
            else
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Not add Because already exist binderKey({instanceKey})...");
            }
            return this;
        }

        #region IViewInstanceCreator
        protected override ViewInstanceCreatorObjectPool CreateObjectPool()
            => new UnityViewInstanceCreatorObjectPool(this);

        protected override System.Type GetViewObjTypeImpl(string instanceKey)
        {
            if (_viewObjectDict.ContainsKey(instanceKey))
            {
                return _viewObjectDict[instanceKey].GetViewType();
            }
            else
            {
                Logger.LogError(Logger.Priority.High, () => $"UnityViewInstanceCreator#CreateViewObj: Don't found instanceKey({instanceKey})...");
                return null;
            }
        }

        protected override IViewObject CreateViewObjImpl(string instanceKey)
        {
            if (_viewObjectDict.ContainsKey(instanceKey))
            {
                return _viewObjectDict[instanceKey].Create();
            }
            else
            {
                Logger.LogError(Logger.Priority.High, () => $"UnityViewInstanceCreator#CreateViewObj: Don't found instanceKey({instanceKey})...");
                return null;
            }
        }

        protected override IModelViewParamBinder GetParamBinderImpl(string binderKey)
        {
            if (_paramBinderDict.ContainsKey(binderKey))
            {
                return _paramBinderDict[binderKey];
            }
            else
            {
                Logger.LogError(Logger.Priority.High, () => $"UnityViewInstanceCreator#GetParamBinder: Don't found binderKey({binderKey})...");
                return null;
            }
        }

        #endregion

        class PrefabCreator : IInstanceCreator
        {
            System.Type ViewType { get; set; }
            Component Prefab { get; set; }

            public PrefabCreator(System.Type type, Component prefab)
            {
                Assert.AreEqual(type, prefab.GetType());
                Assert.IsTrue(prefab is IViewObject, $"{type} is not IViewObject...");
                ViewType = type;
                Prefab = prefab;
            }

            public bool IsValid { get => Prefab != null; }

            public System.Type GetViewType()
                => ViewType;

            public IViewObject Create()
            {
                if (Prefab == null) return null;
                return Object.Instantiate(Prefab) as IViewObject;
            }
        }

        class PredicateCreator : IInstanceCreator
        {
            System.Type _viewType;
            System.Func<IViewObject> _predicate;
            public PredicateCreator(System.Type viewType, System.Func<IViewObject> pred)
            {
                _viewType = viewType;
                _predicate = pred;
            }
            public bool IsValid { get => true; }
            public System.Type GetViewType()
                => _viewType;

            public IViewObject Create()
            {
                return _predicate();
            }
        }
    }

    public static partial class UnityViewInstanceCreatorExtension
    {
        public static UnityViewInstanceCreator AddUnityViewObjects(this UnityViewInstanceCreator target)
        {
            target
                .AddPredicate(typeof(MonoBehaviourViewObject), () => MonoBehaviourViewObject.Create(), new EmptyModelViewParamBinder())
                //Primitive
                .AddPredicate(typeof(CubeViewObject), () => CubeViewObject.CreateInstance(), new EmptyModelViewParamBinder())
                //UI
                .AddPredicate(typeof(RectTransformViewObject), () => RectTransformViewObject.Create("__rectTransform"), new EmptyModelViewParamBinder())
                .AddPredicate(typeof(CanvasViewObject), () => CanvasViewObject.Create("__canvas"), new EmptyModelViewParamBinder())
                .AddPredicate(typeof(HVLayoutGroupViewObject), () => HVLayoutGroupViewObject.Create("__HVLayoutGroup"), new EmptyModelViewParamBinder())
                .AddPredicate(typeof(TextViewObject), () => TextViewObject.Create("__text"), new EmptyModelViewParamBinder())
                .AddPredicate(typeof(ImageViewObject), () => ImageViewObject.Create("__image"), new EmptyModelViewParamBinder())
            ;
            return target;
        }
    }
}
