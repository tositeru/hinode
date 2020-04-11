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
        Model UseModel { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetModel"></param>
        /// <param name="binderInstanceMap"></param>
        void Create(Model targetModel, ModelViewBinderInstanceMap binderInstanceMap);

        /// <summary>
        /// 削除
        /// </summary>
        void Destroy();
    }
}
