using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// メソッドが返す値の更新を監視するクラス
    /// 
    /// 他のクラスの値を監視する時などに使用してください。
    /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PredicateUpdateObserver<T> : IUpdateObserver
    {
        public delegate void OnChangedValueCallback(T value);

        bool _didUpdated;
        T _currentValue;
        System.Func<T> _predicate;
        SmartDelegate<OnChangedValueCallback> _onChangedValueDelegate = new SmartDelegate<OnChangedValueCallback>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.OnUpdatedPasses()"/>
        /// </summary>
        public NotInvokableDelegate<OnChangedValueCallback> OnChangedValue { get => _onChangedValueDelegate; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.BasicUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// </summary>
        public T Value { get => _currentValue; }

        public PredicateUpdateObserver(System.Func<T> predicate)
        {
            _predicate = predicate;
            _currentValue = _predicate();
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.BasicUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.OnUpdatedPasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.NullValuePasses()"/>
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            var v = _predicate();
            _didUpdated = (_currentValue == null)
                ? v == null
                : !_currentValue.Equals(v);
            if (!_didUpdated) return false;

            _currentValue = v;
            _onChangedValueDelegate.SafeDynamicInvoke(_currentValue, () => $"PredicateUpdateObserver#Update");
            return true;
        }

        #region IUpdateObserver interface
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.BasicUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// </summary>
        public bool DidUpdated
        {
            get => _didUpdated;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.BasicUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// </summary>
        public object RawValue { get => _currentValue; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// </summary>
        public void Reset()
        {
            _didUpdated = false;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestPredicateUpdateObserver.ResetAndSetDefaultValuePasses()"/>
        /// </summary>
        /// <param name="doResetDidUpdate"></param>
        public void SetDefaultValue(bool doResetDidUpdate)
        {
            _currentValue = default;
            if (doResetDidUpdate) Reset();
        }
        #endregion
    }
}
