using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum ModelRelationShip
    {
        Self,
        Parent,
        Child,
    }

    public class ModelViewSelector
    {
        public ModelRelationShip RelationShip { get; }
        public string QueryPath { get; } = "";
        public string ViewIdentity { get; } = "";

        public ModelViewSelector(ModelRelationShip relationShip, string queryPath, string viewIdentity)
        {
            RelationShip = relationShip;
            QueryPath = queryPath;
            ViewIdentity = viewIdentity;
        }

        public IEnumerable<object> Query(System.Type objectType, Model model, ModelViewBinderInstanceMap viewBinderInstance)
        {
            Assert.IsTrue(objectType.IsSubclassOf(typeof(Model))
                || objectType.HasInterface<IViewObject>()
                || objectType.HasInterface<IViewLayout>()
                , $"'{objectType}' is not subclass of Model, IViewObject or IViewLayout...");

            var objs = GetEnumerable(model, viewBinderInstance);
            return objs
                .Where(_r => _r.GetType().Equals(objectType));
        }
        public IEnumerable<T> Query<T>(Model model, ModelViewBinderInstanceMap viewBinderInstance)
            => Query(typeof(T), model, viewBinderInstance).OfType<T>();

        public IEnumerable<object> GetEnumerable(Model model, ModelViewBinderInstanceMap viewBinderInstance)
            => new Enumerable(this, model, viewBinderInstance);

        class Enumerable : IEnumerable<object>, IEnumerable
        {
            ModelViewSelector _target;
            Model _model;
            ModelViewBinderInstanceMap _viewBinderInstanceMap;

            public Enumerable(ModelViewSelector target, Model model, ModelViewBinderInstanceMap viewBinderInstanceMap)
            {
                Assert.IsNotNull(target);
                Assert.IsNotNull(model);
                Assert.IsNotNull(viewBinderInstanceMap);
                _target = target;
                _model = model;
                _viewBinderInstanceMap = viewBinderInstanceMap;
            }

            public IEnumerator<object> GetEnumerator()
            {
                Assert.IsNotNull(_viewBinderInstanceMap);

                switch (_target.RelationShip)
                {
                    case ModelRelationShip.Self:
                        if (_target.ViewIdentity == "")
                        {
                            yield return _model;
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model];
                            foreach (var view in instanceMap.QueryViews(_target.ViewIdentity))
                            {
                                yield return view;
                            }
                        }
                        break;
                    case ModelRelationShip.Parent:
                        if (_model.Parent == null)
                            break;

                        if (_target.QueryPath == ""
                            && _target.ViewIdentity == "")
                        {
                            yield return _model.Parent;
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
                            foreach (var p in parentModels)
                            {
                                yield return p;
                            }
                        }
                        else
                        {
                            var instanceMap = _viewBinderInstanceMap[_model.Parent];
                            foreach (var view in parentModels
                                .Where(_p => _viewBinderInstanceMap.BindInstances.ContainsKey(_p))
                                .SelectMany(_p => _viewBinderInstanceMap[_p].QueryViews(_target.ViewIdentity)))
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
                            foreach (var child in children)
                            {
                                yield return child;
                            }
                        }
                        else
                        {
                            var views = children
                                .Where(_c => _viewBinderInstanceMap.BindInstances.ContainsKey(_c))
                                .SelectMany(_c => _viewBinderInstanceMap[_c].QueryViews(_target.ViewIdentity));
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

        #region Object
        public override string ToString()
        {
            return $"Selector({RelationShip}:'{QueryPath}':{ViewIdentity})";
        }
        #endregion
    }
}
