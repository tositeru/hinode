using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// Layoutの基本的な情報を集めたクラス
    /// </summary>
    [System.Serializable]
    public class LayoutInfo
    {
        public delegate void OnChangedValueDelegate(LayoutInfo self, ValueKind kind);

        [System.Flags]
        public enum ValueKind
        {
            UnitSize = 0x1 << 0,
        }

        SmartDelegate<OnChangedValueDelegate> _onChangedValue = new SmartDelegate<OnChangedValueDelegate>();

        [SerializeField] Vector3 _unitSize;

        public NotInvokableDelegate<OnChangedValueDelegate> OnChangedValue { get => _onChangedValue; }

        /// <summary>
        /// ILayoutTargetの基準となるサイズ
        ///
        /// ILayoutの計算内で補助的に使用されます。
        /// </summary>
        public Vector3 UnitSize
        {
            get => _unitSize;
            set
            {
                if (_unitSize.AreNearlyEqual(value, LayoutDefines.NUMBER_PRECISION))
                    return;
                _unitSize = value;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.UnitSize, () => $"LayoutInfo#UnitSize", LayoutDefines.LOG_SELECTOR);
            }
        }
    }
}
