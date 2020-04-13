using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode
{
    public class RecieverSelector
    {
        public enum ModelRelationShip
        {
            Self,
            Parent,
            Child,
        }

        public ModelRelationShip RelationShip { get; }
        public string QueryPath { get; } = "";
        public string ViewIdentity { get; } = "";

        public RecieverSelector(ModelRelationShip relationShip, string queryPath, string viewIdentity)
        {
            RelationShip = relationShip;
            QueryPath = queryPath;
            ViewIdentity = viewIdentity;
        }

        public IEnumerable<IControllerReceiver> GetRecieverEnumerable(Model model, ModelViewBinderInstanceMap viewBinderInstance)
            => new RecieverEnumerable(this, model, viewBinderInstance);

        class RecieverEnumerable : IEnumerable<IControllerReceiver>, IEnumerable
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

            public IEnumerator<IControllerReceiver> GetEnumerator()
            {
                Assert.IsNotNull(_viewBinderInstanceMap);

                switch (_target.RelationShip)
                {
                    case ModelRelationShip.Self:
                        if (_target.ViewIdentity == "")
                        {
                            if(_model is IControllerReceiver)
                            {
                                yield return _model as IControllerReceiver;
                            }
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model];
                            foreach (var view in instanceMap.QueryViews(_target.ViewIdentity)
                                .Select(_v => _v as IControllerReceiver)
                                .Where(_v => _v != null))
                            {
                                if (view is IControllerReceiver)
                                {
                                    yield return view as IControllerReceiver;
                                }
                            }
                        }
                        break;
                    case ModelRelationShip.Parent:
                        if (_model.Parent == null)
                            break;

                        if (_target.ViewIdentity == "")
                        {
                            if(_model.Parent is IControllerReceiver)
                            {
                                yield return _model.Parent as IControllerReceiver;
                            }
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model.Parent];
                            foreach (var view in instanceMap
                                .QueryViews(_target.ViewIdentity)
                                .Select(_v => _v as IControllerReceiver)
                                .Where(_v => _v != null))
                            {
                                yield return view as IControllerReceiver;
                            }
                        }
                        break;
                    case ModelRelationShip.Child:
                        var children = (_target.QueryPath != "")
                            ? _model.Query(_target.QueryPath)
                            : _model.Children;
                        if (_target.ViewIdentity == "")
                        {
                            foreach (var child in children
                                .Select(_c => _c as IControllerReceiver)
                                .Where(_c => _c != null))
                            {
                                yield return child;
                            }
                        }
                        else
                        {
                            var views = children
                                .Where(_c => _viewBinderInstanceMap.BindInstances.ContainsKey(_c))
                                .SelectMany(_c => _viewBinderInstanceMap[_c].QueryViews(_target.ViewIdentity))
                                .Select(_v => _v as IControllerReceiver)
                                .Where(_v => _v != null);
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
}
