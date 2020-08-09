using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// ISubComponentAttributeとそれのメソッドを結びつけるクラス
    ///
    /// static constructor内で使用することを推奨します。
    ///
    /// ISubComponentに対してAttributeによる処理を割り込みさせたいタイミングは以下のものがあります。
    /// - Init : SubComponentManager$Init()の中でISubComponent#Initが呼び出される前に実行されます。
    /// - Destroy : SubComponentManager$Destroy()の中でISubComponent#Destroyが呼び出される前に実行されます。
    /// - UpdateUI : SubComponentManager$UpdateUI()の中でISubComponent#UpdateUIが呼び出される前に実行されます。
    ///
    /// このクラスにメソッドを登録する際は、希望したタイミングへ設定してください。
    /// <see cref="ISubComponentAttribute"/>
    /// </summary>
    public static class SubComponentAttributeManager
    {
        public delegate void AttributeMethodDelegate(object componentInstance);

        #region Init
        static readonly Dictionary<System.Type, AttributeMethodDelegate> _initMethods = new Dictionary<System.Type, AttributeMethodDelegate>();

        public static bool ContainsInitMethod<T>()
            where T : ISubComponentAttribute
            => ContainsInitMethod(typeof(T));

        public static bool ContainsInitMethod(System.Type attrType)
            => _initMethods.ContainsKey(attrType);

        public static void AddInitMethod<T>(AttributeMethodDelegate method)
            where T : ISubComponentAttribute
            => AddInitMethod(typeof(T), method);

        public static void AddInitMethod(System.Type attrType, AttributeMethodDelegate method)
        {
            Assert.IsNotNull(method);
            Assert.IsFalse(_initMethods.ContainsKey(attrType));
            _initMethods.Add(attrType, method);
        }

        public static void RemoveInitMethod<T>()
            where T : ISubComponentAttribute
            => RemoveInitMethod(typeof(T));

        public static void RemoveInitMethod(System.Type attrType)
        {
            _initMethods.Remove(attrType);
        }

        public static void ClearInitMethod()
        {
            _initMethods.Clear();
        }

        public static void RunInitMethods<T>(ISubComponent<T> com)
            where T : MonoBehaviour
        {
            foreach(var method in _initMethods.Values)
            {
                method(com);
            }
        }
        #endregion

        #region Destroy
        static readonly Dictionary<System.Type, AttributeMethodDelegate> _destroyMethods = new Dictionary<System.Type, AttributeMethodDelegate>();

        public static bool ContainsDestroyMethod<T>()
            where T : ISubComponentAttribute
            => ContainsDestroyMethod(typeof(T));

        public static bool ContainsDestroyMethod(System.Type attrType)
            => _destroyMethods.ContainsKey(attrType);

        public static void AddDestroyMethod<T>(AttributeMethodDelegate method)
            where T : ISubComponentAttribute
            => AddDestroyMethod(typeof(T), method);

        public static void AddDestroyMethod(System.Type attrType, AttributeMethodDelegate method)
        {
            Assert.IsNotNull(method);
            Assert.IsFalse(_destroyMethods.ContainsKey(attrType));
            _destroyMethods.Add(attrType, method);
        }

        public static void RemoveDestroyMethod<T>()
            where T : ISubComponentAttribute
            => RemoveDestroyMethod(typeof(T));

        public static void RemoveDestroyMethod(System.Type attrType)
        {
            _destroyMethods.Remove(attrType);
        }

        public static void ClearDestroyMethod()
        {
            _destroyMethods.Clear();
        }

        public static void RunDestroyMethods<T>(ISubComponent<T> com)
            where T : MonoBehaviour
        {
            foreach (var method in _destroyMethods.Values)
            {
                method(com);
            }
        }
        #endregion

        #region UpdateUI
        static readonly Dictionary<System.Type, AttributeMethodDelegate> _updateUIMethods = new Dictionary<System.Type, AttributeMethodDelegate>();

        public static bool ContainsUpdateUIMethod<T>()
            where T : ISubComponentAttribute
            => ContainsUpdateUIMethod(typeof(T));

        public static bool ContainsUpdateUIMethod(System.Type attrType)
            => _updateUIMethods.ContainsKey(attrType);

        public static void AddUpdateUIMethod<T>(AttributeMethodDelegate method)
            where T : ISubComponentAttribute
            => AddUpdateUIMethod(typeof(T), method);

        public static void AddUpdateUIMethod(System.Type attrType, AttributeMethodDelegate method)
        {
            Assert.IsNotNull(method);
            Assert.IsFalse(_updateUIMethods.ContainsKey(attrType));
            _updateUIMethods.Add(attrType, method);
        }

        public static void RemoveUpdateUIMethod<T>()
            where T : ISubComponentAttribute
            => RemoveUpdateUIMethod(typeof(T));

        public static void RemoveUpdateUIMethod(System.Type attrType)
        {
            _updateUIMethods.Remove(attrType);
        }

        public static void ClearUpdateUIMethod()
        {
            _updateUIMethods.Clear();
        }

        public static void RunUpdateUIMethods<T>(ISubComponent<T> com)
            where T : MonoBehaviour
        {
            foreach (var method in _updateUIMethods.Values)
            {
                method(com);
            }
        }
        #endregion

    }
}
