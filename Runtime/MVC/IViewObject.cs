using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Viewに当たるオブジェクトを表すinterface
    /// </summary>
    public interface IViewObject
    {
        Model UseModel { get; set; }
        ModelViewBinder.BindInfo UseBindInfo { get; set; }
        ModelViewBinderInstance UseBinderInstance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetModel"></param>
        /// <param name="binderInstanceMap"></param>
        void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap);

        /// <summary>
        /// 削除
        /// </summary>
        void Unbind();
    }

    public static partial class IViewObjectExtensions
    {
        /// <summary>
        /// 使用しているModelと自身の型の名前を返します。
        /// </summary>
        /// <param name="viewObject"></param>
        /// <returns></returns>
        public static string GetModelAndTypeName(this IViewObject viewObject)
        {
            return $"{viewObject.UseModel}:{viewObject.GetType().FullName}";
        }
    }
}
