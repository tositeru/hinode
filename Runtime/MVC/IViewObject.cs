using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    public delegate void OnViewObjectDestroyed(IViewObject viewObject);
    public delegate void OnViewObjectBinded(IViewObject viewObject);
    public delegate void OnViewObjectUnbinded(IViewObject viewObject);

    /// <summary>
    /// Viewに当たるオブジェクトを表すinterface
    ///
    /// 将来のインターフェイスの変更に対応するため、直接このinterfaceを継承するよりもEmptyViewObjectかMonoBehaviourViewObjectを継承することを推奨します。
    /// <seealso cref="EmptyViewObject"/>
    /// <seealso cref="MonoBehaviourViewObject"/>
    /// </summary>
    public interface IViewObject
    {
        bool IsVisibility { get; set; }

        Model UseModel { get; set; }
        ModelViewBinder.BindInfo UseBindInfo { get; set; }
        ModelViewBinderInstance UseBinderInstance { get; set; }

        NotInvokableDelegate<OnViewObjectDestroyed> OnDestroyed { get; }
        NotInvokableDelegate<OnViewObjectBinded> OnBinded { get; }
        NotInvokableDelegate<OnViewObjectUnbinded> OnUnbinded { get; }

        /// <summary>
        /// 破棄する
        /// </summary>
        void Destroy();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childID"></param>
        object QueryChild(string childID);

        /// <summary>
        /// Modelとバインドする
        /// </summary>
        /// <param name="targetModel"></param>
        /// <param name="binderInstanceMap"></param>
        void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap);

        /// <summary>
        /// Modelとのバインドを解除する
        /// </summary>
        void Unbind();

        /// <summary>
        /// IViewLayoutの適応後に呼び出されるイベント
        /// </summary>
        void OnViewLayouted();
    }

    public static partial class IViewObjectExtensions
    {
        public static string ToString(this IViewObject viewObject)
        {
            return $"{viewObject.GetType().FullName} in Model={(viewObject.UseModel != null ? viewObject.UseModel.GetPath(): "(none)")} BindInfo={(viewObject.UseBindInfo != null ? viewObject.UseBindInfo.ID : "(none)")}";
        }

        public static bool DoBinding(this IViewObject target)
            => target.UseModel != null && target.UseBindInfo != null;

        /// <summary>
        /// 使用しているModelと自身の型の名前を返します。
        /// </summary>
        /// <param name="viewObject"></param>
        /// <returns></returns>
        public static string GetModelAndTypeName(this IViewObject viewObject)
        {
            return $"(Model:ViewType)=>{viewObject.UseModel}:{viewObject.GetType().FullName}";
        }

        public static T QueryChild<T>(this IViewObject viewObject, string childID)
            where T : class
            => viewObject.QueryChild(childID) as T;

        public static object QueryChild(this IViewObject viewObject, IEnumerable<string> childIDs)
        {
            var curViewObj = viewObject;
            object childView = null;
            int nestCount = childIDs.Count();
            int index = 0;
            foreach (var childID in childIDs)
            {
                childView = curViewObj.QueryChild(childID);
                if (childView == null) return null;

                index++;

                if (!(childView is IViewObject)) break;
                curViewObj = childView as IViewObject;
            }
            return nestCount == index ? childView : null;
        }
        public static T QueryChild<T>(this IViewObject viewObject, IEnumerable<string> childIDs)
            where T : class
            => viewObject.QueryChild(childIDs) as T;
    }

    /// <summary>
    /// 空のIViewObject
    ///
    /// IViewObjectのインターフェイス変更に対応するために作成しましたので、こちらを継承するようにしてください。
    ///
    /// UnityのMonoBehaviourをIViewObject化したい場合はMonoBehaviourViewObjectを使用してください。
    /// <seealso cref="MonoBehaviourViewObject"/>
    /// </summary>
    public class EmptyViewObject : IViewObject
    {
        protected virtual void OnDestroy() { }
        protected virtual void OnBind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap) { }
        protected virtual void OnUnbind() { }

        SmartDelegate<OnViewObjectDestroyed> _onDestroyed = new SmartDelegate<OnViewObjectDestroyed>();
        SmartDelegate<OnViewObjectBinded> _onBinded = new SmartDelegate<OnViewObjectBinded>();
        SmartDelegate<OnViewObjectUnbinded> _onUnbinded = new SmartDelegate<OnViewObjectUnbinded>();

        public bool IsVisibility { get; set; } = true;

        public Model UseModel { get; set; }
        public ModelViewBinder.BindInfo UseBindInfo { get; set; }
        public ModelViewBinderInstance UseBinderInstance { get; set; }

        public NotInvokableDelegate<OnViewObjectDestroyed> OnDestroyed { get => _onDestroyed; }
        public NotInvokableDelegate<OnViewObjectBinded> OnBinded { get => _onBinded; }
        public NotInvokableDelegate<OnViewObjectUnbinded> OnUnbinded { get => _onUnbinded; }

        public void Destroy()
        {
            Unbind();
            OnDestroy();
            _onDestroyed.Instance?.Invoke(this);

            _onBinded.Clear();
            _onUnbinded.Clear();
            _onDestroyed.Clear();
        }

        public virtual object QueryChild(string childID)
        {
            Logger.LogWarning(Logger.Priority.Debug, () => $"Please Override QueryChild(string childID) in SubClass!!");
            return null;
        }

        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
            Unbind();

            UseModel = targetModel;
            UseBindInfo = bindInfo;
            UseBinderInstance = (binderInstanceMap?.Contains(UseModel) ?? false)
                ? binderInstanceMap.BindInstances[UseModel]
                : null;
            OnBind(targetModel, bindInfo, binderInstanceMap);
            _onBinded.Instance?.Invoke(this);
        }
        public void Unbind()
        {
            if (this.DoBinding())
            {
                _onUnbinded.Instance?.Invoke(this);
                OnUnbind();
            }

            UseModel = null;
            UseBindInfo = null;
            UseBinderInstance = null;
        }

        public virtual void OnViewLayouted()
        { }

        public override string ToString()
        {
            return IViewObjectExtensions.ToString(this);
        }
    }
}
