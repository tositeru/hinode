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
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateObserver<T> : IUpdateObserver
    {
        bool _didUpdated;
        [SerializeField] T _v;

        public UpdateObserver() { }
        public UpdateObserver(T value)
        {
            _v = value;
        }
        public bool DidUpdated { get => _didUpdated; }

        public T Value
        {
            get => _v;
            set
            {
                if (Equals(_v, value)) return;
                _v = value;
                _didUpdated = true;
            }
        }
        public object RawValue
        {
            get => Value;
        }

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
