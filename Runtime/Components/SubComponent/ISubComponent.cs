using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// MonoBehaviourを継承したクラス内の機能を分けたものを表すinterface
    ///
    /// このクラスを使用した場合はSubComponentManagerの使用を推奨します。
    /// <seealso cref="SubComponentManager{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubComponent<T>
        where T : MonoBehaviour
    {
        T RootComponent { get; set; }
        void Init();
        void Destroy();
        void UpdateUI();
    }

    public static class SubComponentDefines
    {
        /// <summary>
        /// Loggerのためのセレクタ
        /// </summary>
        public static readonly string LOGGER_SELECTOR = "SubComponent";
    }
}
