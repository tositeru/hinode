using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// ModelとViewのパラメータを関連づけるinterface
    /// </summary>
    public interface IModelViewParamBinder
    {
        void Update(Model model, IViewObject viewObj);
    }

    public static class IModelViewParamBinderExtenstions
    {
        /// <summary>
        /// 値が異なる時だけ代入する関数。
        /// IModelViewParamBinder内で値が変更されたかどうかチェックする方針なので用意しました。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void SmartAssign<T>(this IModelViewParamBinder binder, ref T dest, ref T src)
        {
            if(!src.Equals(dest))
            {
                dest = src;
            }
        }
    }

    public class EmptyModelViewParamBinder : IModelViewParamBinder
    {
        public void Update(Model model, IViewObject viewObj) { }
    }

    /// <summary>
    /// ModelとViewを関連づけるクラス
    ///
    /// このクラスは関連付けのための情報を保持するだけなので、
    /// 実際に関連付けしたい時はModelViewBinder#CreateBindInstance()でBindInstanceクラスを作成し、
    /// そのインスタンスを使用してください。
    /// 
    /// パラメータの流れはModel -> Viewのみになっています
    /// <seealso cref="Model"/>
    /// <seealso cref="IModelViewParamBinder"/>
    /// <seealso cref="IViewObject"/>
    /// <seealso cref="ModelViewBinderInstance"/>
    /// <seealso cref="ModelViewBinderMap"/>
    /// </summary>
    public class ModelViewBinder
    {
        HashSet<System.Type> _enabledModelTypes = new HashSet<System.Type>();
        Dictionary<string, BindInfo> _bindInfoDict = new Dictionary<string, BindInfo>();
        Dictionary<string, ControllerInfo> _controllers = new Dictionary<string, ControllerInfo>();

        /// <summary>
        /// 対応するModelのクエリ
        ///
        /// クエリパスは指定できませんので注意してください
        /// </summary>
        public string Query { get; private set; }

        /// <summary>
        /// 対応しているModelの型、ない場合はすべてのModelに対応していることを表します。
        /// </summary>
        public IReadOnlyCollection<System.Type> EnabledModelTypes { get => _enabledModelTypes; }

        /// <summary>
        /// 
        /// </summary>
        public IViewInstanceCreator ViewInstaceCreator { get; set; }

        /// <summary>
        /// ModelとViewの関連付けるための情報
        /// </summary>
        public IEnumerable<BindInfo> BindInfos { get => _bindInfoDict.Values; }

        /// <summary>
        /// ModelとViewの関連付けるための情報の個数
        /// </summary>
        public int BindInfoCount { get => _bindInfoDict.Count; }

        /// <summary>
        /// Model自体に設定されたController情報
        /// </summary>
        public IReadOnlyDictionary<string, ControllerInfo> Controllers { get => _controllers; }

        /// <summary>
        /// ModelとViewを関連づけるための情報を持つクラス
        /// </summary>
        public class BindInfo
        {
            Dictionary<string, ControllerInfo> _controllers = new Dictionary<string, ControllerInfo>();
            Dictionary<string, object> _viewLayouts = new Dictionary<string, object>();

            public string ID { get; }
            public string InstanceKey { get; }
            public string BinderKey { get; }
            public IModelViewParamBinder UseParamBinder { get; set; }

            public IReadOnlyDictionary<string, ControllerInfo> Controllers { get => _controllers; }
            public IReadOnlyDictionary<string, object> ViewLayouts { get => _viewLayouts; }

            public BindInfo(string id, string instanceKey, string binderKey)
            {
                ID = id;
                InstanceKey = instanceKey;
                BinderKey = binderKey;
            }

            public BindInfo(string id, System.Type viewType)
                : this(id, viewType.FullName, viewType.FullName)
            { }

            public BindInfo(System.Type viewType)
                : this(viewType.FullName, viewType.FullName, viewType.FullName)
            { }

            public BindInfo SetUseParamBinder(IModelViewParamBinder paramBinder)
            {
                UseParamBinder = paramBinder;
                return this;
            }

            public BindInfo AddControllerInfo(ControllerInfo controllerInfo)
            {
                Assert.IsFalse(_controllers.ContainsKey(controllerInfo.Keyword), $"Controller({controllerInfo.Keyword}) already exist...");
                _controllers.Add(controllerInfo.Keyword, controllerInfo);
                return this;
            }

            public BindInfo AddViewLayout(string keyword, object value)
            {
                if(_viewLayouts.ContainsKey(keyword))
                {
                    throw new System.ArgumentException($"Already set ViewLayout keyword({keyword})...");
                }
                _viewLayouts.Add(keyword, value);
                return this;
            }

            public BindInfo AddViewLayout(System.Enum keyword, object value)
                => AddViewLayout(keyword.ToString(), value);

            public object GetViewLayoutValue(string keyword)
            {
                if (!_viewLayouts.ContainsKey(keyword))
                {
                    throw new System.ArgumentException($"Not exist ViewLayout keyword({keyword})...");
                }
                return _viewLayouts[keyword];
            }
        }

        public ModelViewBinder(string query, IViewInstanceCreator instanceCreator, params BindInfo[] bindInfos)
            : this(query, instanceCreator, bindInfos.AsEnumerable())
        { }

        public ModelViewBinder(string query, IViewInstanceCreator instanceCreator, IEnumerable<BindInfo> bindInfos)
        {
            Query = query;
            ViewInstaceCreator = instanceCreator;
            foreach (var i in bindInfos)
            {
                _bindInfoDict.Add(i.ID, i);
            }
        }

        /// <summary>
        ///
        /// ## 設定用のソースファイルの構文素案
        /// <model queryPath>:
        ///      - <ComponentType>: bind(<viewParamName>=<modelParamName>, ...)
        ///      - <ComponentType>: bind(
        ///         <viewParamName>=<modelParamName>,
        ///         <viewParamName>=<modelParamName>,
        ///         ...)
        ///      - ...
        /// </summary>
        /// <param name="sourceCode"></param>
        public ModelViewBinder(string sourceCode)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"Binder({Query}:{EnabledModelTypes.Select(_t => _t.FullName).Aggregate("", (_s, _c) => _s+_c+";")})";
        }

        #region Enabled Model Type
        public bool ContainEnabledModelType(System.Type modelType)
            => _enabledModelTypes.Contains(modelType);
        public bool ContainEnabledModelType<T>()
            where T : Model
            => _enabledModelTypes.Contains(typeof(T));
        public bool ContainEnabledModelType(Model model)
            => ContainEnabledModelType(model.GetType());

        public ModelViewBinder AddEnabledModelType<TModel>()
            where TModel : Model
            => AddEnabledModelType(typeof(TModel));
        public ModelViewBinder AddEnabledModelType(System.Type modelType)
        {
            if (!_enabledModelTypes.Contains(modelType))
            {
                _enabledModelTypes.Add(modelType);
            }
            return this;
        }

        public ModelViewBinder RemoveEnabledModelType<TModel>()
            where TModel : Model
            => RemoveEnabledModelType(typeof(TModel));
        public ModelViewBinder RemoveEnabledModelType(System.Type modelType)
        {
            if (_enabledModelTypes.Contains(modelType))
            {
                _enabledModelTypes.Remove(modelType);
            }
            return this;
        }
        #endregion

        #region ControllerInfo
        public ModelViewBinder AddControllerInfo(ControllerInfo controllerInfo)
        {
            Assert.IsFalse(_controllers.ContainsKey(controllerInfo.Keyword), $"Controller({controllerInfo.Keyword}) already exist...");
            _controllers.Add(controllerInfo.Keyword, controllerInfo);
            return this;
        }
        public ModelViewBinder RemoveControllerInfo(string senderName)
        {
            Assert.IsTrue(_controllers.ContainsKey(senderName), $"Controller({senderName}) don't exist...");
            _controllers.Remove(senderName);
            return this;
        }
        #endregion

        public ModelViewBinder AddInfosFromOtherBinder(ModelViewBinder otherBinder)
        {
            foreach(var bindInfo in otherBinder.BindInfos)
            {
                if(_bindInfoDict.ContainsKey(bindInfo.ID))
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Not Add bindInfo because already exist ID({bindInfo.ID})...  otherBinder({otherBinder})");
                    continue;
                }
                else if(_bindInfoDict.Values.Contains(bindInfo))
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Not Add bindInfo because already exist same reference BindInfo...  otherBinder({otherBinder})");
                    continue;
                }
                else
                {
                    _bindInfoDict.Add(bindInfo.ID, bindInfo);
                }
            }
            return this;
        }

        /// <summary>
        /// 指定されたBindInfoオブジェクトと対応するIModelViewParamBinderを取得する
        /// </summary>
        /// <param name="bindInfo"></param>
        /// <returns></returns>
        public IModelViewParamBinder GetParamBinder(BindInfo bindInfo)
        {
            return ViewInstaceCreator.GetParamBinder(bindInfo);
        }

        /// <summary>
        /// Modelと設定されているクエリパスが一致しているか確認します
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DoMatch(Model model)
        {
            if(EnabledModelTypes.Count <= 0)
            {
                return  model.DoMatchQuery(Query);
            }
            else if(ContainEnabledModelType(model))
            {
                return Query == null || Query == ""
                    ? true
                    : model.DoMatchQuery(Query);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ModelとViewを実際に関連付けしたインスタンスを作成します。
        ///
        /// Modelの状態が変更された結果クエリパスと一致するようになるかもしれないので、生成自体は行うようにしています。
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ModelViewBinderInstance CreateBindInstance(Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            return new ModelViewBinderInstance(this, model, binderInstanceMap);
        }

        /// <summary>
        /// Modelと関連するViewのオブジェクトを作成する
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<IViewObject> CreateViewObjects(Model model, ModelViewBinderInstance binderInstance, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Assert.IsTrue(DoMatch(model));

            return BindInfos.Select(_i =>
            {
                ModelViewValidator.ValidateBindInfo(model, _i, ViewInstaceCreator);

                var view = ViewInstaceCreator.CreateViewObj(_i);
                view.UseModel = model;
                view.UseBinderInstance = binderInstance;
                view.Bind(model, _i, binderInstanceMap);
                Logger.Log(Logger.Priority.Low, () => $"ModelViewBinder#CreateViewObjects: Create View Obj {view.GetModelAndTypeName()}!!");

#if UNITY_EDITOR
                if (view is MonoBehaviour)
                {
                    (view as MonoBehaviour).name = $"{model} viewID={view.UseBindInfo.ID}";
                }
#endif

                return view;
            }).ToList();
        }
    }

    /// <summary>
    /// ModelとViewを関連付けしたもの
    /// <seealso cref="ModelViewBinder"/>
    /// </summary>
    public class ModelViewBinderInstance : System.IDisposable
    {
        List<IViewObject> _viewObjects = new List<IViewObject>();
        Dictionary<IViewObject, HashSet<IViewObject>> _autoLayoutViewObjects = new Dictionary<IViewObject, HashSet<IViewObject>>();

        Dictionary<IViewObject, HashSet<IEventDispatcherHelper>> _eventDispathcerHelpObjectListDict = new Dictionary<IViewObject, HashSet<IEventDispatcherHelper>>();

        public bool IsValid { get => Model != null && !Model.IsMarkedDestory && Binder != null; }

        public ModelViewBinder Binder { get; private set; }
        public ModelViewBinderInstanceMap UseInstanceMap { get; set; }
        public Model Model { get; private set; }
        public IEnumerable<IViewObject> ViewObjects { get => _viewObjects; }
        public IReadOnlyDictionary<IViewObject, HashSet<IViewObject>> AutoLayoutViewObjects { get => _autoLayoutViewObjects; }
        public IReadOnlyDictionary<IViewObject, HashSet<IEventDispatcherHelper>> EventDispatcherHelpObjectsForView { get => _eventDispathcerHelpObjectListDict; }

        public ModelViewBinderInstance(ModelViewBinder binder, Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Binder = binder;
            Model = model;
            UseInstanceMap = binderInstanceMap;
            _viewObjects = binder.CreateViewObjects(Model, this, binderInstanceMap);
            foreach(var v in ViewObjects)
            {
                v.UseBinderInstance = this;
            }

            if (binderInstanceMap != null && binderInstanceMap.UseEventDispatcherMap != null)
            {
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                foreach (var view in ViewObjects
                    .Where(_v => eventDispatcherMap.IsCreatableControllerObjects(Model, _v, _v.UseBindInfo.Controllers.Values)))
                {
                    var controllerObjects = eventDispatcherMap.CreateEventDispatcherHelpObjects(Model, view, view.UseBindInfo.Controllers.Values);
                    var objsTypes = controllerObjects.Select(_o => _o.GetType().FullName).Aggregate((_s, _c) => _s + ";" + _c);
                    _eventDispathcerHelpObjectListDict.Add(view, controllerObjects);
                }
            }

            if (binderInstanceMap != null && (binderInstanceMap.UseViewLayouter?.DoEnableToAutoCreateViewObject ?? false))
            {
                var viewLayouter = binderInstanceMap.UseViewLayouter;
                foreach (var viewObj in ViewObjects)
                {
                    var bindInfo = viewObj.UseBindInfo;

                    var autoViewObjHash = new HashSet<IViewObject>();
                    foreach(var creator in viewLayouter.GetAutoViewObjectCreator(viewObj, bindInfo.ViewLayouts.Keys))
                    {
                        var autoViewObj = creator.Create(viewObj);
                        autoViewObjHash.Add(autoViewObj);
                    }
                    if(autoViewObjHash.Any()) {
                        foreach(var v in autoViewObjHash)
                        {
                            v.UseBinderInstance = this;
                        }
                        _autoLayoutViewObjects.Add(viewObj, autoViewObjHash);

                        Logger.Log(Logger.Priority.Debug, () => {
                            var list = autoViewObjHash.Select(_v => _v.GetType().FullName).Aggregate("",(_s, _c) => _s + _c + ", ");
                            return $"Add Auto ViewObjs!! {model} : {viewObj} : {list}";
                        });
                    }
                }
            }

            AttachModelCallback();
        }

        void ModelOnUpdated(Model m)
        {
            if(UseInstanceMap != null)
            {
                UseInstanceMap.Update(m);
            }
            else
            {
                UpdateViewObjects();
            }
        }

        void ModelOnDestroyed(Model m)
        {
            Dispose();
        }

        public void AttachModelCallback()
        {
            DettachModelCallback();

            Model.OnUpdated.Add(ModelOnUpdated);
            Model.OnDestroyed.Add(ModelOnDestroyed);
        }

        public void DettachModelCallback()
        {
            Model.OnUpdated.Remove(ModelOnUpdated);
            Model.OnUpdated.Remove(ModelOnDestroyed);
        }

        public void UpdateViewObjects()
        {
            foreach (var viewObj in ViewObjects)
            {
                var paramBinder = Binder.GetParamBinder(viewObj.UseBindInfo);
                paramBinder.Update(Model, viewObj);
            }
        }

        public void ApplyViewLayout()
        {
            if (UseInstanceMap == null || UseInstanceMap.UseViewLayouter == null)
                return;
            var viewLayouter = UseInstanceMap.UseViewLayouter;
            var allViewObjs = ViewObjects
                .Concat(ViewObjects
                    .Where(_v => AutoLayoutViewObjects.ContainsKey(_v))
                    .SelectMany(_v => AutoLayoutViewObjects[_v]));
            foreach (var viewObj in allViewObjs
                .Where(_v => _v.UseBindInfo != null && _v.UseBindInfo.ViewLayouts != null))
            {
                viewLayouter.SetAllMatchLayouts(viewObj, viewObj.UseBindInfo.ViewLayouts);
                Logger.Log(Logger.Priority.Low, () => {
                    var log = viewObj.UseBindInfo.ViewLayouts
                        .Select(_v => $"{_v}={viewLayouter.IsVaildViewObject(_v.Key, viewObj)}:{viewLayouter.IsVaildValue(_v.Key, _v.Value)}")
                        .Aggregate("layout=", (_s, _c) => _s + _c + ";");
                    return $"ModelViewBinderInstance#ApplyViewLayout -> {viewObj.GetModelAndTypeName()}: {log}";
                });
            }

            //他のViewObjectの影響を受けるかもしれないので、ここで行っている。
            foreach (var viewObj in allViewObjs)
            {
                viewObj.OnViewLayouted();
            }
        }

        public IEnumerable<IViewObject> QueryViews(string query)
        {
            return ViewObjects.Where(_v => _v.UseBindInfo.ID == query);
        }

        #region IEventDispatcherHelper
        public bool HasEventDispatcherHelpObject(IViewObject viewObject, System.Type helpObjectType)
        {
            Assert.IsTrue(helpObjectType.HasInterface<IEventDispatcherHelper>(), $"{helpObjectType} is not IEventDispatcherHelper interface...");
            Assert.IsTrue(ViewObjects.Contains(viewObject), $"This BinderInstance don't have '{viewObject}'...");

            if (!_eventDispathcerHelpObjectListDict.ContainsKey(viewObject)) return false;
            return _eventDispathcerHelpObjectListDict[viewObject]
                .Any(_c => _c.GetType().IsSameOrInheritedType(helpObjectType));
        }

        public bool HasEventDispathcerHelpObject<T>(IViewObject viewObject)
            where T : class, IEventDispatcherHelper
            => HasEventDispatcherHelpObject(viewObject, typeof(T));

        public IEventDispatcherHelper GetEventDispathcerHelpObject(IViewObject viewObject, System.Type controllerObjectType)
        {
            Assert.IsTrue(controllerObjectType.HasInterface<IEventDispatcherHelper>(), $"{controllerObjectType} is not IEventDispatcherHelper interface...");
            Assert.IsTrue(ViewObjects.Contains(viewObject), $"This BinderInstance don't have '{viewObject}'...");


            if (!_eventDispathcerHelpObjectListDict.ContainsKey(viewObject)) return null;
            return _eventDispathcerHelpObjectListDict[viewObject]
                .FirstOrDefault(_c => _c.GetType().IsSameOrInheritedType(controllerObjectType));
        }

        public T GetEventDispathcerHelpObject<T>(IViewObject viewObject)
            where T : class, IEventDispatcherHelper
            => GetEventDispathcerHelpObject(viewObject, typeof(T)) as T;
        #endregion

        #region IDisposabe interface
        public void Dispose()
        {
            if(ViewObjects.Any())
            {
                foreach (var view in ViewObjects)
                {
                    if(_eventDispathcerHelpObjectListDict.ContainsKey(view))
                    {
                        foreach(var sender in _eventDispathcerHelpObjectListDict[view])
                        {
                            sender.Destroy();
                        }
                    }
                    if(_autoLayoutViewObjects.ContainsKey(view))
                    {
                        foreach(var autoView in _autoLayoutViewObjects[view])
                        {
                            autoView.Unbind();
                            autoView.UseModel = null;
                            autoView.UseBinderInstance = null;
                        }
                    }
                    view.Unbind();
                    view.UseModel = null;
                    view.UseBinderInstance = null;
                }
                _autoLayoutViewObjects.Clear();
                _eventDispathcerHelpObjectListDict.Clear();
                _viewObjects.Clear();
            }

            if(UseInstanceMap != null)
            {
                var instanceMap = UseInstanceMap;
                UseInstanceMap = null;
                instanceMap?.Remove(Model);
            }

            if(Model != null)
            {
                DettachModelCallback();
                Model = null;
            }
            Binder = null;
        }
        #endregion
    }

    public static class ModelViewBinderInstanceExtensions
    {
        /// <summary>
        /// target内にある全てのControllerInfoを取得する 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<(Model model, IViewObject viewObj, ControllerInfo controllerInfo)>
            GetControllerInfos(this ModelViewBinderInstance target)
        {
            return target.GetControllersAttachedModel()
                .Concat(target.GetControllersAttachedViewObjs());
        }

        /// <summary>
        /// Modelに設定されたControllerInfoを全て取得する
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<(Model model, IViewObject viewObj, ControllerInfo controllerInfo)> GetControllersAttachedModel(this ModelViewBinderInstance target)
        {
            if (!target.IsValid)
                return Enumerable.Range(0, 0).Select(t => ((Model)null, (IViewObject)null, (ControllerInfo)null));

            return target.Binder.Controllers
                .Select(_c => (
                    model: target.Model,
                    viewObj: default(IViewObject),
                    controllerInfo: _c.Value
                ));
        }

        /// <summary>
        /// 全てのViewObjectに対して設定されているControllerInfoを全て取得する
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<(Model model, IViewObject viewObj, ControllerInfo controllerInfo)> GetControllersAttachedViewObjs(this ModelViewBinderInstance target)
        {
            if (!target.IsValid)
                return Enumerable.Range(0, 0).Select(t => ((Model)null, (IViewObject)null, (ControllerInfo)null));

            return target.ViewObjects
                .SelectMany(_v =>
                    _v.UseBindInfo.Controllers.Select(_c => (
                        model: _v.UseModel,
                        viewObj: _v,
                        controllerInfo: _c.Value
                    ))
                );
        }
    }

    public static partial class IViewObjectExtensions
    {
        public static bool HasEventDispathcerHelpObject(this IViewObject viewObject, System.Type controllerObjectType)
        {
            if (viewObject.UseBinderInstance == null) return false;
            return viewObject.UseBinderInstance.HasEventDispatcherHelpObject(viewObject, controllerObjectType);
        }

        public static bool HasEventDispatcherHelpObject<T>(this IViewObject viewObject)
            where T : class, IEventDispatcherHelper
            => viewObject.UseBinderInstance.HasEventDispathcerHelpObject<T>(viewObject);

        public static IEventDispatcherHelper GetEventDispathcerHelpObject(this IViewObject viewObject, System.Type controllerObjectType)
        {
            if (viewObject.UseBinderInstance == null) return null;
            return viewObject.UseBinderInstance.GetEventDispathcerHelpObject(viewObject, controllerObjectType);
        }

        public static T GetEventDispathcerHelpObject<T>(this IViewObject viewObject)
            where T : class, IEventDispatcherHelper
            => viewObject.UseBinderInstance.GetEventDispathcerHelpObject<T>(viewObject);

    }
}
