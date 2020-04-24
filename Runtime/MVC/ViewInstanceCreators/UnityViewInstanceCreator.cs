﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    public class UnityViewInstanceCreator : IViewInstanceCreator
    {
        interface IInstanceCreator
        {
            System.Type GetViewType();
            IViewObject Create();
        }
        Dictionary<string, IInstanceCreator> _viewObjectDict = new Dictionary<string, IInstanceCreator>();
        Dictionary<string, IModelViewParamBinder> _paramBinderDict = new Dictionary<string, IModelViewParamBinder>();

        public UnityViewInstanceCreator() { }

        public UnityViewInstanceCreator AddPrefab<T>(T prefab, IModelViewParamBinder paramBinder)
            where T : Component, IViewObject
            => Add(typeof(T).FullName, new PrefabCreator<T>(prefab), typeof(T).FullName, paramBinder);
        public UnityViewInstanceCreator AddPrefab<T>(string instanceKey, T prefab, string binderKey, IModelViewParamBinder paramBinder)
            where T : Component, IViewObject
            => Add(instanceKey, new PrefabCreator<T>(prefab), binderKey, paramBinder);

        public UnityViewInstanceCreator AddPredicate(System.Type viewType, System.Func<IViewObject> predicate, IModelViewParamBinder paramBinder)
            => Add(viewType.FullName, new PredicateCreator(viewType, predicate), viewType.FullName, paramBinder);
        public UnityViewInstanceCreator AddPredicate(System.Type viewType, string instanceKey, System.Func<IViewObject> predicate, string binderKey, IModelViewParamBinder paramBinder)
            => Add(instanceKey, new PredicateCreator(viewType, predicate), binderKey, paramBinder);

        UnityViewInstanceCreator Add(string instanceKey, IInstanceCreator creator, string binderKey, IModelViewParamBinder paramBinder)
        {
            if (!_viewObjectDict.ContainsKey(instanceKey))
            {
                _viewObjectDict.Add(instanceKey, creator);
            }
            else
            {
                Hinode.Logger.LogWarning(Hinode.Logger.Priority.Debug, () => $"Not add Because already exist instanceKey({instanceKey})...");
            }

            if (!_paramBinderDict.ContainsKey(binderKey))
            {
                _paramBinderDict.Add(binderKey, paramBinder);
            }
            else
            {
                Hinode.Logger.LogWarning(Hinode.Logger.Priority.Debug, () => $"Not add Because already exist binderKey({instanceKey})...");
            }
            return this;
        }

        #region IViewInstanceCreator
        protected override System.Type GetViewObjTypeImpl(string instanceKey)
        {
            if (_viewObjectDict.ContainsKey(instanceKey))
            {
                return _viewObjectDict[instanceKey].GetViewType();
            }
            else
            {
                Hinode.Logger.LogError(Hinode.Logger.Priority.High, () => $"UnityViewInstanceCreator#CreateViewObj: Don't found instanceKey({instanceKey})...");
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
                Hinode.Logger.LogError(Hinode.Logger.Priority.High, () => $"UnityViewInstanceCreator#CreateViewObj: Don't found instanceKey({instanceKey})...");
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
                Hinode.Logger.LogError(Hinode.Logger.Priority.High, () => $"UnityViewInstanceCreator#GetParamBinder: Don't found binderKey({binderKey})...");
                return null;
            }
        }

        #endregion

        class PrefabCreator<T> : IInstanceCreator
            where T : Component, IViewObject
        {
            T Prefab { get; set; }
            public PrefabCreator(T prefab)
            {
                Prefab = prefab;
            }

            public System.Type GetViewType()
                => typeof(T);

            public IViewObject Create()
            {
                return UnityEngine.Object.Instantiate(Prefab);
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
                .AddPredicate(typeof(CanvasViewObject), () => CanvasViewObject.Create("__canvas"), new EmptyModelViewParamBinder())
                .AddPredicate(typeof(HVLayoutGroupViewObject), () => HVLayoutGroupViewObject.Create("__HVLayoutGroup"), new EmptyModelViewParamBinder())
            ;
            return target;
        }
    }
}