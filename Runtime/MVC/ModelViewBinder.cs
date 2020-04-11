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
        IReadOnlyDictionary<System.Type, IBindInfo> _bindInfoDict;

        /// <summary>
        /// 対応するModelのクエリパス
        /// </summary>
        public string QueryPath { get; private set; }

        /// <summary>
        /// ModelとViewの関連付けるための情報
        /// </summary>
        public IEnumerable<IBindInfo> BindInfos { get => _bindInfoDict.Values; }

        /// <summary>
        /// ModelとViewの関連付けるための情報の個数
        /// </summary>
        public int BindInfoCount { get => _bindInfoDict.Count; }

        /// <summary>
        /// ModelとViewを関連づけるための情報を持つクラス
        /// </summary>
        public interface IBindInfo
        {
            /// <summary>
            /// 使用するIModelViewParamBinder
            /// </summary>
            IModelViewParamBinder ParamBinder { get; }

            /// <summary>
            /// このBindInfoが対応しているViewオブジェクトを作成する
            /// </summary>
            /// <returns></returns>
            IViewObject CreateViewObject();
        }

        /// <summary>
        /// BindInfoの辞書を簡単に作成するための関数
        /// </summary>
        /// <param name="bindInfoCreator"></param>
        /// <param name="elements"></param>
        /// <returns>ModelとViewを関連づけるための情報の辞書 Key=ViewObjType, Value=IBindInfo</returns>
        public static Dictionary<System.Type, IBindInfo> CreateBindInfoDict(System.Func<System.Type, IModelViewParamBinder, IBindInfo> bindInfoCreator, params (System.Type viewObjType, IModelViewParamBinder)[] elements)
        {
            var dict = new Dictionary<System.Type, IBindInfo>();
            foreach(var e in elements)
            {
                dict.Add(e.Item1, bindInfoCreator(e.Item1, e.Item2));
            }
            return dict;
        }

        /// <summary>
        /// BindInfoの辞書を簡単に作成するための関数
        /// こちらの関数はDefaultBindInfoが辞書の値に設定されます。
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>ModelとViewを関連づけるための情報の辞書 Key=ViewObjType, Value=IBindInfo</returns>
        public static Dictionary<System.Type, IBindInfo> CreateBindInfoDict(params (System.Type viewObjType, IModelViewParamBinder)[] elements)
            => CreateBindInfoDict((viewObjType, paramBinder) => new DefaultBindInfo(paramBinder, viewObjType), elements);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryPath">Modelのクエリパス</param>
        /// <param name="bindInfoDict">ModelとViewを関連づけるための情報の辞書 Key=ViewObjType, Value=IBindInfo</param>
        public ModelViewBinder(string queryPath, IReadOnlyDictionary<System.Type, IBindInfo> bindInfoDict)
        {
            QueryPath = queryPath;
            _bindInfoDict = bindInfoDict;
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
        /// 指定されたViewオブジェクトと対応するIModelViewParamBinderを取得する
        /// </summary>
        /// <param name="viewObj"></param>
        /// <returns></returns>
        public IModelViewParamBinder GetParamBinder(IViewObject viewObj)
        {
            var type = viewObj.GetType();
            if (_bindInfoDict.ContainsKey(type))
            {
                return _bindInfoDict[type].ParamBinder;
            }
            else
            {
                return null;
            }
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
                var view = _i.CreateViewObject();
                view.Create(model, binderInstanceMap);
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
                var paramBinder = Binder.GetParamBinder(viewObj);
                paramBinder.Update(Model, viewObj);
            }
        }

        public void Dispose()
        {
            foreach(var view in ViewObjects)
            {
                view.Destroy();
            }

            Model?.OnUpdated.Remove(ModelOnUpdated);
        }
    }

    /// <summary>
    /// 汎用のModelViewBinder.BindInfo
    ///
    /// 引数無しのコンストラクを持つIViewBinderObject型に対応しています
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="ModelViewBinder.IBindInfo"/>
    /// </summary>
    public class DefaultBindInfo : ModelViewBinder.IBindInfo
    {
        public System.Type ViewObjType { get; }

        public DefaultBindInfo(IModelViewParamBinder paramBinder, System.Type viewObjType)
        {
            ParamBinder = paramBinder;
            ViewObjType = viewObjType;
            Assert.IsTrue(ViewObjType.DoHasInterface(typeof(IViewObject)));
        }

        public static DefaultBindInfo Create<T>(IModelViewParamBinder paramBinder)
            where T : IViewObject
        {
            return new DefaultBindInfo(paramBinder, typeof(T));
        }

        #region ModelViewBinder.IBindInfo interface
        public IModelViewParamBinder ParamBinder { get; }

        public IViewObject CreateViewObject()
        {
            var cstor = ViewObjType.GetConstructor(new System.Type[]{});
            return cstor.Invoke(null) as IViewObject;
        }
        #endregion
    }
}
