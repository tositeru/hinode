using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class RecieverSelector
    {
        public ModelRelationShip RelationShip { get; }
        public string QueryPath { get; } = "";
        public string ViewIdentity { get; } = "";

        public bool IsFooking { get => FookingRecieverType != null; }
        public System.Type FookingRecieverType { get; set; }
        public object FookEventData { get; set; }

        public RecieverSelector(ModelRelationShip relationShip, string queryPath, string viewIdentity)
        {
            RelationShip = relationShip;
            QueryPath = queryPath;
            ViewIdentity = viewIdentity;
        }

        public IEnumerable<(System.Type recieverType, IControllerReciever reciever, object eventData)> Query(System.Type recieverType, Model model, ModelViewBinderInstanceMap viewBinderInstance, object eventData)
        {
            var recievers = GetRecieverEnumerable(model, viewBinderInstance);
            if (IsFooking)
            {
                return recievers
                    .Where(_r => _r.GetType().HasInterface(FookingRecieverType))
                    .Select(_r => (recieverType: FookingRecieverType, reciever: _r, eventData: FookEventData));
            }
            else
            {
                return recievers
                    .Where(_r => _r.GetType().HasInterface(recieverType))
                    .Select(_r => (recieverType: recieverType, reciever: _r, eventData: eventData));
            }
        }

        public IEnumerable<IControllerReciever> GetRecieverEnumerable(Model model, ModelViewBinderInstanceMap viewBinderInstance)
            => new RecieverEnumerable(this, model, viewBinderInstance);

        public IEnumerable<T> GetRecieverEnumerable<T>(Model model, ModelViewBinderInstanceMap viewBinderInstance)
            where T : IControllerSender
            => new RecieverEnumerable(this, model, viewBinderInstance).OfType<T>();

        class RecieverEnumerable : IEnumerable<IControllerReciever>, IEnumerable
        {
            RecieverSelector _target;
            Model _model;
            ModelViewBinderInstanceMap _viewBinderInstanceMap;

            public RecieverEnumerable(RecieverSelector target, Model model, ModelViewBinderInstanceMap viewBinderInstanceMap)
            {
                Assert.IsNotNull(target);
                Assert.IsNotNull(model);
                Assert.IsNotNull(viewBinderInstanceMap);
                _target = target;
                _model = model;
                _viewBinderInstanceMap = viewBinderInstanceMap;
            }

            public IEnumerator<IControllerReciever> GetEnumerator()
            {
                Assert.IsNotNull(_viewBinderInstanceMap);

                switch (_target.RelationShip)
                {
                    case ModelRelationShip.Self:
                        if (_target.ViewIdentity == "")
                        {
                            if (_model is IControllerReciever)
                            {
                                yield return _model as IControllerReciever;
                            }
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model];
                            foreach (var view in instanceMap.QueryViews(_target.ViewIdentity)
                                .OfType<IControllerReciever>())
                            {
                                yield return view as IControllerReciever;
                            }
                        }
                        break;
                    case ModelRelationShip.Parent:
                        if (_model.Parent == null)
                            break;

                        if (_target.QueryPath == ""
                            && _target.ViewIdentity == "")
                        {
                            if (_model.Parent is IControllerReciever)
                                yield return _model.Parent as IControllerReciever;
                            break;
                        }

                        IEnumerable<Model> parentModels;
                        if (_target.QueryPath != ""
                            && _viewBinderInstanceMap.RootModel != null)
                        {
                            parentModels = _viewBinderInstanceMap.RootModel.Query(_target.QueryPath)
                                .Where(_m => _model != _m && _model.GetTraversedRootEnumerable().Any(_p => _p == _m));
                        }
                        else
                        {
                            parentModels = new Model[] { _model.Parent };
                        }


                        if (_target.ViewIdentity == "")
                        {
                            foreach (var p in parentModels
                                .OfType<IControllerReciever>())
                            {
                                yield return p;
                            }
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model.Parent];
                            foreach (var view in parentModels
                                .Where(_p => _viewBinderInstanceMap.BindInstances.ContainsKey(_p))
                                .SelectMany(_p => _viewBinderInstanceMap[_p].QueryViews(_target.ViewIdentity))
                                .OfType<IControllerReciever>())
                            {
                                yield return view;
                            }
                        }
                        break;
                    case ModelRelationShip.Child:
                        var children = (_target.QueryPath != "")
                            ? _model.Query(_target.QueryPath)
                            : _model.Children;
                        children = children.Where(_m => _m != _model);
                        if (_target.ViewIdentity == "")
                        {
                            foreach (var child in children
                                .OfType<IControllerReciever>())
                            {
                                yield return child;
                            }
                        }
                        else
                        {
                            var views = children
                                .Where(_c => _viewBinderInstanceMap.BindInstances.ContainsKey(_c))
                                .SelectMany(_c => _viewBinderInstanceMap[_c].QueryViews(_target.ViewIdentity))
                                .OfType<IControllerReciever>();
                            foreach (var v in views)
                            {
                                yield return v;
                            }
                        }
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }

    public static partial class RecieverSelectorExtensions
    {
        /// <summary>
        /// 指定したtargetModel,eventDataをselectorとマッチするIControllerRecieverへ送信する
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="recieverType"></param>
        /// <param name="targetModel"></param>
        /// <param name="eventData"></param>
        /// <param name="binderInstanceMap"></param>
        public static void Send(this RecieverSelector selector, System.Type recieverType, Model targetModel, object eventData, ModelViewBinderInstanceMap binderInstanceMap)
        {
            foreach (var (useRecieverType, reciever, useEventData) in selector
                .Query(recieverType, targetModel, binderInstanceMap, eventData))
            {
                ControllerTypeManager.DoneRecieverExecuter(useRecieverType, reciever, targetModel, useEventData);
            }
        }
        public static void Send<TReciever>(this RecieverSelector selector, Model targetModel, object eventData, ModelViewBinderInstanceMap binderInstanceMap)
            where TReciever : IControllerReciever
            =>  selector.Send(typeof(TReciever), targetModel, eventData, binderInstanceMap);

    }
}
