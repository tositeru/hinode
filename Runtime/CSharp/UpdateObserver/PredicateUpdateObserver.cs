using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// メソッドが返す値の更新を監視するクラス
    /// 
    /// 他のクラスの値を監視する時などに使用してください。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PredicateUpdateObserver<T> : IUpdateObserver
    {
        public delegate void OnChangedValueCallback(T value);

        bool _didUpdated;
        T _currentValue;
        System.Func<T> _predicate;
        SmartDelegate<OnChangedValueCallback> _onChangedValueDelegate = new SmartDelegate<OnChangedValueCallback>();

        public NotInvokableDelegate<OnChangedValueCallback> OnChangedValue { get => _onChangedValueDelegate; }
        public T Value { get => _currentValue; }

        public PredicateUpdateObserver(System.Func<T> predicate)
        {
            _predicate = predicate;
            _currentValue = _predicate();
        }

        public bool Update()
        {
            var v = _predicate();
            _didUpdated = (_currentValue == null)
                ? v == null
                : !_currentValue.Equals(v);
            if (!_didUpdated) return false;

            _currentValue = v;
            _onChangedValueDelegate.Instance?.Invoke(_currentValue);
            return true;
        }

        #region IUpdateObserver interface
        public bool DidUpdated
        {
            get => _didUpdated;
        }

        public object RawValue { get => _currentValue; }

        public void Reset()
        {
            _didUpdated = false;
        }

        public void SetDefaultValue(bool doResetDidUpdate)
        {
            _currentValue = default;
            if (doResetDidUpdate) Reset();
        }
        #endregion
    }
}
