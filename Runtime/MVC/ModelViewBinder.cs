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
        Dictionary<string, BindInfo> _bindInfoDict = new Dictionary<string, BindInfo>();

        /// <summary>
        /// 対応するModelのクエリパス
        /// </summary>
        public string QueryPath { get; private set; }

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

            //TODO public ViewLayoutet? ViewLayouter? { get; set; }
            //TODO public HashSet<IController> UseControllers { get; set; }

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

            public object GetViewLayoutValue(string keyword)
            {
                if (!_viewLayouts.ContainsKey(keyword))
                {
                    throw new System.ArgumentException($"Not exist ViewLayout keyword({keyword})...");
                }
                return _viewLayouts[keyword];
            }
        }

        public class ControllerInfo
        {
            List<RecieverSelector> _recieverInfos = new List<RecieverSelector>();
            public string Keyword { get; set; }
            public IEnumerable<RecieverSelector> RecieverSelectors { get => _recieverInfos; }

            public ControllerInfo(string keyword, params RecieverSelector[] recieverInfos)
                : this(keyword, recieverInfos.AsEnumerable())
            { }

            public ControllerInfo(string keyword, IEnumerable<RecieverSelector> recieverInfos)
            {
                Keyword = keyword;
                _recieverInfos = recieverInfos.ToList();
            }

            public void AddRecieverInfo(RecieverSelector selector)
            {
                _recieverInfos.Add(selector);
            }
        }

        public ModelViewBinder(string queryPath, IViewInstanceCreator instanceCreator, params BindInfo[] bindInfos)
            : this(queryPath, instanceCreator, bindInfos.AsEnumerable())
        { }

        public ModelViewBinder(string queryPath, IViewInstanceCreator instanceCreator, IEnumerable<BindInfo> bindInfos)
        {
            QueryPath = queryPath;
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

        /// <summary>
        /// 指定されたBindInfoオブジェクトと対応するIModelViewParamBinderを取得する
        /// </summary>
        /// <param name="bindInfo"></param>
        /// <returns></returns>
        public IModelViewParamBinder GetParamBinder(BindInfo bindInfo)
        {
            return ViewInstaceCreator.GetParamBinderObj(bindInfo);
        }

        /// <summary>
        /// Modelと設定されているクエリパスが一致しているか確認します
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DoMatch(Model model)
        {
            return  model.DoMatchQueryPath(QueryPath);
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
                var view = ViewInstaceCreator.CreateViewObj(_i);
                view.UseModel = model;
                view.UseBinderInstance = binderInstance;
                view.Bind(model, _i, binderInstanceMap);
                Logger.Log(Logger.Priority.Low, () => $"ModelViewBinder#CreateViewObjects: Create View Obj {view.GetModelAndTypeName()}!!");
                return view;
            }).ToList();
        }

        public IReadOnlyCollection<IControllerSenderInstance> CreateControllerSenderInstances(IViewObject viewObj, Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Assert.IsTrue(DoMatch(model));
            Assert.IsNotNull(binderInstanceMap);
            Assert.IsNotNull(binderInstanceMap.UseControllerMap);

            if (viewObj.UseBindInfo.Controllers == null
                || viewObj.UseBindInfo.Controllers.Count <= 0)
                return null;
            var controllerMap = binderInstanceMap.UseControllerMap;
            var senderInstanceHash = new HashSet<IControllerSenderInstance>();
            foreach(var controllerInfo in viewObj.UseBindInfo.Controllers.Values
                .Where(_c => controllerMap.ContainsSenderKeyword(_c.Keyword)))
            {
                var matchSender = senderInstanceHash
                    .FirstOrDefault(_sender => _sender.UseSenderGroup.ContainsSenderKeyword(controllerInfo.Keyword));
                if (matchSender == null)
                {
                    matchSender = controllerMap.CreateController(controllerInfo.Keyword, viewObj, model, binderInstanceMap);
                    Assert.IsNotNull(matchSender);
                    senderInstanceHash.Add(matchSender);
                    Logger.Log(Logger.Priority.Low, () => $"ModelViewBinder#CreateControllerSenderInstance: Create Controller Sender Instance!! Type=>({matchSender.GetType()})!!");
                }

                var senderType = matchSender.UseSenderGroup.GetSenderType(controllerInfo.Keyword);
                if(!matchSender.DoEnableSender(senderType))
                {
                    matchSender.EnableSender(senderType);
                }
                matchSender.AddSelectors(senderType, controllerInfo.RecieverSelectors);
            }
            return senderInstanceHash.Count() <= 0 ? null : senderInstanceHash;
        }
    }

    /// <summary>
    /// ModelとViewを関連付けしたもの
    /// <seealso cref="ModelViewBinder"/>
    /// </summary>
    public class ModelViewBinderInstance : System.IDisposable
    {
        List<IViewObject> _viewObjects = new List<IViewObject>();
        Dictionary<IViewObject, IReadOnlyCollection<IControllerSenderInstance>> _controllerSenderInstances = new Dictionary<IViewObject, IReadOnlyCollection<IControllerSenderInstance>>();
        Dictionary<IViewObject, HashSet<IViewObject>> _autoLayoutViewObjects = new Dictionary<IViewObject, HashSet<IViewObject>>();

        public ModelViewBinder Binder { get; }
        public ModelViewBinderInstanceMap UseInstanceMap { get; set; }
        public Model Model { get; }
        public IEnumerable<IViewObject> ViewObjects { get => _viewObjects; }
        public IReadOnlyDictionary<IViewObject, HashSet<IViewObject>> AutoLayoutViewObjects { get => _autoLayoutViewObjects; }

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

            if(binderInstanceMap != null && binderInstanceMap.UseControllerMap != null)
            {
                foreach(var view in ViewObjects)
                {
                    var senders = binder.CreateControllerSenderInstances(view, Model, binderInstanceMap);
                    if(senders != null)
                    {
                        _controllerSenderInstances.Add(view, senders);
                    }
                }
            }

            if(binderInstanceMap != null && (binderInstanceMap.UseViewLayouter?.DoEnableToAutoCreateViewObject ?? false))
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
                    }
                }
            }

            AttachModelOnUpdated();
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

        public void AttachModelOnUpdated()
        {
            Model.OnUpdated.Remove(ModelOnUpdated);
            Model.OnUpdated.Add(ModelOnUpdated);
        }

        public void DettachModelOnUpdated()
        {
            Model.OnUpdated.Remove(ModelOnUpdated);
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

        public IReadOnlyCollection<IControllerSenderInstance> GetControllerSenders(IViewObject viewObject)
        {
            Assert.IsTrue(ViewObjects.Contains(viewObject));
            if(_controllerSenderInstances.ContainsKey(viewObject))
            {
                return _controllerSenderInstances[viewObject];
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            foreach(var keyAndViews in _autoLayoutViewObjects)
            {
                var view = keyAndViews.Key;
                var autoViews = keyAndViews.Value;
                foreach(var v in autoViews)
                {
                    v.Unbind();
                }
            }
            _autoLayoutViewObjects.Clear();

            foreach (var view in ViewObjects)
            {
                if(_controllerSenderInstances.ContainsKey(view))
                {
                    foreach(var sender in _controllerSenderInstances[view])
                    {
                        sender.Destroy();
                    }
                }

                view.Unbind();
                view.UseModel = null;
                view.UseBinderInstance = null;
            }
            _viewObjects.Clear();

            DettachModelOnUpdated();
        }
    }
}
