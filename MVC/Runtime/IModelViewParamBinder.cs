using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
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
            if (!src.Equals(dest))
            {
                dest = src;
            }
        }
    }

    public class EmptyModelViewParamBinder : IModelViewParamBinder
    {
        public void Update(Model model, IViewObject viewObj) { }
    }
}
