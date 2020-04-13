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
            public string ID { get; }
            public string InstanceKey { get; }
            public string BinderKey { get; }

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
        }

        /// <summary>
        /// IViewObjectとIModelViewParamBinderを作成する抽象クラス
        /// </summary>
        public abstract class IViewInstanceCreator
        {
            public IViewObject CreateViewObj(string instanceKey)
            {
                var viewObj = CreateViewObjImpl(instanceKey);
                Assert.IsNotNull(viewObj, $"Fialed to create ViewObject because don't match ViewObject Key({instanceKey})...");
                return viewObj;
            }

            public IModelViewParamBinder GetParamBinderObj(BindInfo bindInfo)
            {
                var paramBinder = GetParamBinderImpl(bindInfo.BinderKey);
                Assert.IsNotNull(paramBinder, $"Failed to create IModelViewParamBinder because don't match Binder Key({paramBinder})...");
                return paramBinder;
            }

            protected abstract IViewObject CreateViewObjImpl(string instanceKey);
            protected abstract IModelViewParamBinder GetParamBinderImpl(string binderKey);
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
        public IViewObject[] CreateViewObjects(Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Assert.IsTrue(DoMatch(model));

            return BindInfos.Select(_i =>
            {
                var view = ViewInstaceCreator.CreateViewObj(_i.InstanceKey);
                view.UseModel = model;
                view.UseBindInfo = _i;
                view.Bind(model, _i, binderInstanceMap);
                return view;
            }).ToArray();
        }
    }

    /// <summary>
    /// ModelとViewを関連付けしたもの
    /// <seealso cref="ModelViewBinder"/>
    /// </summary>
    public class ModelViewBinderInstance : System.IDisposable
    {
        public ModelViewBinder Binder { get; }
        public Model Model { get; }
        public IViewObject[] ViewObjects { get; }

        public ModelViewBinderInstance(ModelViewBinder binder, Model model, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Binder = binder;
            Model = model;
            ViewObjects = binder.CreateViewObjects(Model, binderInstanceMap);
        }

        void ModelOnUpdated(Model m)
        {
            UpdateViewObjects();
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

        public IEnumerable<IViewObject> QueryViews(string query)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            foreach(var view in ViewObjects)
            {
                view.Unbind();
                view.UseModel = null;
                view.UseBindInfo = null;
            }

            Model?.OnUpdated.Remove(ModelOnUpdated);
        }
    }

    /// <summary>
    /// 汎用のModelViewBinder.IViewInstanceCreator
    ///
    /// 引数無しのコンストラクを持つIViewBinderObject型に対応しています
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="ModelViewBinder.IBindInfo"/>
    /// </summary>
    public class DefaultViewInstanceCreator : ModelViewBinder.IViewInstanceCreator
    {
        Dictionary<string, (System.Type viewObjType, IModelViewParamBinder paramBinder)> _dict = new Dictionary<string, (System.Type viewObjType, IModelViewParamBinder paramBinder)>();

        System.Type[] _emptryArgs = new System.Type[] { };

        public DefaultViewInstanceCreator(params (System.Type viewType, IModelViewParamBinder paramBinder)[] data)
            : this(data.AsEnumerable())
        { }

        public DefaultViewInstanceCreator(IEnumerable<(System.Type viewType, IModelViewParamBinder paramBinder)> data)
        {
            foreach(var d in data)
            {
                var cstor = d.viewType.GetConstructor(_emptryArgs);
                Assert.IsNotNull(cstor, $"空引数のコンストラクターがあるクラスだけに対応しています... viewType={d.viewType.FullName}");
                Assert.IsTrue(d.viewType.DoHasInterface<IViewObject>(), $"IViewObject型を継承した型だけ対応しています... viewType={d.viewType.FullName}");
                Assert.IsNotNull(d.paramBinder, $"paramBinderは必ず設定してください...");
                _dict.Add(d.viewType.FullName, (d.viewType, d.paramBinder));
            }
        }

        protected override IViewObject CreateViewObjImpl(string instanceKey)
        {
            if (!_dict.ContainsKey(instanceKey)) return null;
            var type = _dict[instanceKey].viewObjType;
            var cstor = type.GetConstructor(_emptryArgs);
            return cstor.Invoke(null) as IViewObject;
        }

        protected override IModelViewParamBinder GetParamBinderImpl(string binderKey)
        {
            if (!_dict.ContainsKey(binderKey)) return null;
            return _dict[binderKey].paramBinder;
        }
    }
}
