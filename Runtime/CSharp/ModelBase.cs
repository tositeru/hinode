// Copyright 2019~ tositeru
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// MVCにおけるModelのベースクラス
    ///
    /// 専用のAttributeとしてIModelAttributeを継承したAttributeを提供します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValueKind"></typeparam>
    public abstract class ModelBase<T, TValueKind>
        where T : ModelBase<T, TValueKind>
        where TValueKind : System.Enum
    {
        public delegate void OnChangedValueDelegate(T model, TValueKind kind, object value, object prevValue);
        SmartDelegate<OnChangedValueDelegate> _onChangedValue = new SmartDelegate<OnChangedValueDelegate>();

        public NotInvokableDelegate<OnChangedValueDelegate> OnChangedValue { get => _onChangedValue; }

        /// <summary>
        /// ModelFieldLabelAttributeが指定されているT型の全てのFieldに対してOnChangedValueコールバックを呼び出します。
        /// </summary>
        public void ForceToCallAllOnChangedValue()
        {
            var bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = GetType().GetFields(bindFlags)
                .Select(_f => (filed: _f, fieldLabel: _f.GetCustomAttribute<ModelFieldLabelAttribute>()))
                .Where(_t => _t.fieldLabel != null);

            T self = this as T;
            foreach (var (field, fieldLabel) in fields)
            {
                var valueKind = fieldLabel.ValueKind<TValueKind>();
                var value = field.GetValue(this);
                _onChangedValue.SafeDynamicInvoke(self, valueKind, value, value, () => "Fail in ForceToCallAllOnChangedValue...");
            }
        }

        protected void CallOnChangedValueDirect(TValueKind valueKind, object value, object prevValue, System.Func<string> getErrorMessage)
        {
            _onChangedValue.SafeDynamicInvoke(this as T, valueKind, value, prevValue, getErrorMessage);
        }

        protected void CallOnChangedValue<TValue>(ref TValue origin, TValue value, TValueKind valueKind, System.Func<string> getErrorMessage)
        {
            if(origin == null)
            {
                if (value == null) return;
            }
            else if (origin.Equals(value)) return;

            var prev = origin;
            origin = value;
            _onChangedValue.SafeDynamicInvoke(this as T, valueKind, origin, prev, getErrorMessage);
        }

        protected void CallOnChangedNumberValue(ref float origin, float value, TValueKind valueKind, System.Func<string> getErrorMessage, float epsilon = float.Epsilon)
        {
            if (MathUtils.AreNearlyEqual(origin, value, epsilon)) return;
            var prev = origin;
            origin = value;
            _onChangedValue.SafeDynamicInvoke(this as T, valueKind, origin, prev, getErrorMessage);
        }

        protected void CallOnChangedNumberValue(ref double origin, double value, TValueKind valueKind, System.Func<string> getErrorMessage, double epsilon = double.Epsilon)
        {
            if (MathUtils.AreNearlyEqual(origin, value, epsilon)) return;
            var prev = origin;
            origin = value;
            _onChangedValue.SafeDynamicInvoke(this as T, valueKind, origin, prev, getErrorMessage);
        }

    }

    public interface IModelAttribute
    { }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ModelFieldLabelAttribute
        : System.Attribute
        , IModelAttribute
    {
        object _valueKind;
        public T ValueKind<T>()
            where T : System.Enum
            => (T)_valueKind;

        public ModelFieldLabelAttribute(int valueKind)
        {
            _valueKind = valueKind;
        }
    }
}
