using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public interface IUpdateObserver
    {
        bool DidUpdated { get; }
        object RawValue { get; }
        void Reset();
        void SetDefaultValue(bool doResetDidUpdate);
    }

    /// <summary>
    /// 値が更新されたか監視するクラス
    ///
    /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateObserver<T> : IUpdateObserver
    {
        public delegate void OnChangedValueCallback(T value);

        bool _didUpdated;
        [SerializeField] T _v;

        SmartDelegate<OnChangedValueCallback> _onChangedValueDelegate = new SmartDelegate<OnChangedValueCallback>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver.OnChangedValuePasses()"/>
        /// </summary>
        public NotInvokableDelegate<OnChangedValueCallback> OnChangedValue { get => _onChangedValueDelegate; }

        public UpdateObserver() { }
        public UpdateObserver(T value)
        {
            _v = value;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver.BasicUsagePasses()"/>
        /// </summary>
        public bool DidUpdated { get => _didUpdated; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver.BasicUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver.OnChangedValuePasses()"/>
        /// </summary>
        public T Value
        {
            get => _v;
            set
            {
                if (Equals(_v, value)) return;
                _v = value;
                _didUpdated = true;
                _onChangedValueDelegate.Instance?.Invoke(_v);
            }
        }
        public object RawValue
        {
            get => Value;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.IUpdateObserver.TestUpdateObserver.BasicUsagePasses()"/>
        /// </summary>
        public void Reset()
        {
            _didUpdated = false;
        }

        public void SetDefaultValue(bool doResetDidUpdate)
        {
            Value = default;
            if (doResetDidUpdate) Reset();
        }
    }
}
