using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

namespace Hinode.Layouts
{
    /// <summary>
    /// Layoutの基本的な情報を集めたクラス
    /// </summary>
    [System.Serializable]
    public class LayoutInfo
    {
        public static readonly float UNFIXED_VALUE = -1f;
        public static readonly Vector3 UNFIXED_VECTOR3 = Vector3.one * -1f;

        public class OnChangedValueParam
        {
            public Vector3 LayoutSize { get; private set; }
            public Vector3 MinSize { get; private set; }
            public Vector3 MaxSize { get; private set; }
            public bool IgnoreLayoutGroup { get; private set; }
            public float SizeGrowInGroup { get; private set; }
            public int OrderInGroup { get; private set; }

            public OnChangedValueParam(LayoutInfo info)
            {
                LayoutSize = info.LayoutSize;
                MinSize = info.MinSize;
                MaxSize = info.MaxSize;
                IgnoreLayoutGroup = info.IgnoreLayoutGroup;
                SizeGrowInGroup = info.SizeGrowInGroup;
                OrderInGroup = info.OrderInGroup;
            }
        }

        public delegate void OnChangedValueDelegate(LayoutInfo self, ValueKind kind, OnChangedValueParam prevInfo);

        [System.Flags]
        public enum ValueKind
        {
            LayoutSize = 0x1 << 0,
            MinSize = 0x1 << 1,
            MaxSize = 0x1 << 2,
            IgnoreLayoutGroup = 0x1 << 3,
            SizeGrowInGroup = 0x1 << 4,
            OrderInGroup = 0x1 << 5,
        }

        SmartDelegate<OnChangedValueDelegate> _onChangedValue = new SmartDelegate<OnChangedValueDelegate>();

        [SerializeField] Vector3 _layoutSize = UNFIXED_VECTOR3;
        [SerializeField] Vector3 _minSize = UNFIXED_VECTOR3;
        [SerializeField] Vector3 _maxSize = UNFIXED_VECTOR3;
        [SerializeField] bool _ignoreLayoutGroup = false;
        [SerializeField] float _sizeGrowInGroup = 1f;
        [SerializeField] int _orderInGroup = 0;

        public NotInvokableDelegate<OnChangedValueDelegate> OnChangedValue { get => _onChangedValue; }

        /// <summary>
        /// 他のLayout計算で参照されるILayoutTargetの基準となるサイズ
        /// 値が`UNFIXED_VALUE`の場合は設定されていないと判断します。
        /// 
        /// 実際のLayout計算に使用する際は基準サイズは以下の条件を満たすようにしてください。
        /// またこの条件を満たすため、このプロパティを直接使用するよりGetLayoutSize(ILayoutTarget)を使用することを推奨します。
        /// 
        /// 基準サイズ) MinSize <= ILayoutTarget#LocalSize <= LayoutSize
        /// </summary>
        public Vector3 LayoutSize
        {
            get => _layoutSize;
            set
            {
                var v = Inner_GetSize(value);
                if (_layoutSize.AreNearlyEqual(v, LayoutDefines.NUMBER_PRECISION))
                    return;
                var prevInfo = new OnChangedValueParam(this);
                _layoutSize = v;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.LayoutSize, prevInfo, () => $"LayoutInfo#LayoutSize", LayoutDefines.LOG_SELECTOR);
            }
        }

        /// <summary>
        /// ILayoutTarget#LocalSizeの最小値
        /// 値が`UNFIXED_VALUE`の場合は設定されていないと判断します。
        ///
        /// 設定する際にMaxSizeより値が大きくなる場合は自動的にMaxSizeの値に変更されます。
        /// 
        /// 以下の条件を満たすように設定されます。
        /// - MinSize <= MaxSize
        /// 
        /// </summary>
        public Vector3 MinSize
        {
            get => _minSize;
            set
            {
                Vector3 v = Inner_GetMinValue(value, MaxSize);
                if (_minSize.AreNearlyEqual(v, LayoutDefines.NUMBER_PRECISION))
                    return;
                var prevInfo = new OnChangedValueParam(this);
                _minSize = v;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.MinSize, prevInfo, () => $"LayoutInfo#MinSize", LayoutDefines.LOG_SELECTOR);
            }
        }

        /// <summary>
        /// ILayoutTarget#LocalSizeの最大値
        /// 値が`UNFIXED_VALUE`の場合は設定されていないと判断します。
        ///
        /// 設定する際にMinSizeより値が小さくなる場合は自動的にMinSizeの値に変更されます。
        /// 
        /// 以下の条件を満たすように設定されます。
        /// - MinSize <= MaxSize
        /// 
        /// </summary>
        public Vector3 MaxSize
        {
            get => _maxSize;
            set
            {
                var v = Inner_GetMaxValue(MinSize, value);
                if (_maxSize.AreNearlyEqual(v, LayoutDefines.NUMBER_PRECISION))
                    return;
                var prevInfo = new OnChangedValueParam(this);
                _maxSize = v;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.MaxSize, prevInfo, () => $"LayoutInfo#MaxSize", LayoutDefines.LOG_SELECTOR);
            }
        }

        /// <summary>
        /// ILayoutGroupの計算対象から除外するか？
        /// </summary>
        public bool IgnoreLayoutGroup
        {
            get => _ignoreLayoutGroup;
            set
            {
                if (_ignoreLayoutGroup == value) return;
                var prevInfo = new OnChangedValueParam(this);
                _ignoreLayoutGroup = value;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.IgnoreLayoutGroup, prevInfo, () => "LayoutInfo#IgnoreLayoutGroup", LayoutDefines.LOG_SELECTOR);
            }
        }

        /// <summary>
        /// ILayoutGroup内での要素のサイズ比率
        ///
        /// CSSのFlexboxのgrowプロパティと似たニュアンスになります。
        /// </summary>
        public float SizeGrowInGroup
        {
            get => _sizeGrowInGroup;
            set
            {
                var v = Max(0f, value);
                if (MathUtils.AreNearlyEqual(_sizeGrowInGroup, v, LayoutDefines.NUMBER_PRECISION)) return;
                var prevInfo = new OnChangedValueParam(this);
                _sizeGrowInGroup = v;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.SizeGrowInGroup, prevInfo, () => "LayoutInfo#SizeGrowInGroup", LayoutDefines.LOG_SELECTOR);
            }
        }

        /// <summary>
        /// ILayoutGroup内での要素の順番
        ///
        /// CSSのFlexboxのorderプロパティと似たニュアンスになります。
        /// </summary>
        public int OrderInGroup
        {
            get => _orderInGroup;
            set
            {
                if (_orderInGroup == value) return;
                var prevInfo = new OnChangedValueParam(this);
                _orderInGroup = value;
                _onChangedValue.SafeDynamicInvoke(this, ValueKind.OrderInGroup, prevInfo, () => "LayoutInfo#SizeGrowInGroup", LayoutDefines.LOG_SELECTOR);
            }
        }

        public LayoutInfo()
        { }

        public LayoutInfo(LayoutInfo other)
        {
            Assign(other);
        }

        /// <summary>
        /// 実際のレイアウト計算で使用されるLayoutSizeを返します。
        ///
        /// 返り値の要素の値は以下の条件から決定されます。
        /// - layoutSize == UNFIXED_VALUEの時
        ///   Max(MinSize, target#LocalSize)
        /// - layoutSize != UNFIXED_VALUEの時
        ///   Min(layoutSize, Max(MinSize, target#LocalSize))
        ///   
        /// MinSizeより小さくなる場合がありますので注意してください。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector3 GetLayoutSize(ILayoutTarget target)
        {
            var result = Vector3.Max(MinSize, target.LocalSize);
            if (LayoutSize.x >= 0) result.x = Min(result.x, LayoutSize.x);
            if (LayoutSize.y >= 0) result.y = Min(result.y, LayoutSize.y);
            if (LayoutSize.z >= 0) result.z = Min(result.z, LayoutSize.z);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public LayoutInfo SetMinMaxSize(Vector3 min, Vector3 max)
        {
            var prevInfo = new OnChangedValueParam(this);
            ValueKind changedKinds = Inner_SetMinMaxSize(min, max);
            if(changedKinds != 0)
            {
                _onChangedValue.SafeDynamicInvoke(this, changedKinds, prevInfo, () => $"LayoutInfo#SetMinMaxSize", LayoutDefines.LOG_SELECTOR);
            }
            return this;
        }

        /// <summary>
        /// 他のLayoutInfoからパラメータをコピーする
        /// </summary>
        /// <param name="other"></param>
        public void Assign(LayoutInfo other)
        {
            var prevInfo = new OnChangedValueParam(this);

            ValueKind changedKinds = 0;
            if (!_layoutSize.AreNearlyEqual(other.LayoutSize, LayoutDefines.NUMBER_PRECISION))
            {
                _layoutSize = other.LayoutSize;
                changedKinds |= ValueKind.LayoutSize;
            }

            changedKinds |= Inner_SetMinMaxSize(other.MinSize, other.MaxSize);

            if(_ignoreLayoutGroup != other.IgnoreLayoutGroup)
            {
                _ignoreLayoutGroup = other.IgnoreLayoutGroup;
                changedKinds |= ValueKind.IgnoreLayoutGroup;
            }

            if (!MathUtils.AreNearlyEqual(_sizeGrowInGroup, other.SizeGrowInGroup, LayoutDefines.NUMBER_PRECISION))
            {
                _sizeGrowInGroup = other.SizeGrowInGroup;
                changedKinds |= ValueKind.SizeGrowInGroup;
            }
            if (_orderInGroup != other.OrderInGroup)
            {
                _orderInGroup = other.OrderInGroup;
                changedKinds |= ValueKind.OrderInGroup;
            }

            if(changedKinds != 0)
            {
                _onChangedValue.SafeDynamicInvoke(this, changedKinds, prevInfo, () => $"LayoutInfo#Assign", LayoutDefines.LOG_SELECTOR);
            }
        }

        Vector3 Inner_GetSize(Vector3 size)
        {
            Vector3 v = size;
            if (v.x < 0) v.x = UNFIXED_VALUE;
            if (v.y < 0) v.y = UNFIXED_VALUE;
            if (v.z < 0) v.z = UNFIXED_VALUE;
            return v;
        }

        Vector3 Inner_GetMinValue(Vector3 min, Vector3 max)
        {
            Vector3 v = Vector3.zero;
            v.x = max.x > 0 ? Min(min.x, max.x) : min.x;
            v.y = max.y > 0 ? Min(min.y, max.y) : min.y;
            v.z = max.z > 0 ? Min(min.z, max.z) : min.z;
            return v;
        }

        Vector3 Inner_GetMaxValue(Vector3 min, Vector3 max)
        {
            Vector3 v = default;
            v.x = (max.x > 0) ? Max(min.x, max.x) : UNFIXED_VALUE;
            v.y = (max.y > 0) ? Max(min.y, max.y) : UNFIXED_VALUE;
            v.z = (max.z > 0) ? Max(min.z, max.z) : UNFIXED_VALUE;
            return v;
        }

        ValueKind Inner_SetMinMaxSize(Vector3 min, Vector3 max)
        {
            Vector3 m = Inner_GetMinValue(min, max);
            Vector3 M = Inner_GetMaxValue(min, max);

            ValueKind changedKinds = 0;
            if (!_minSize.AreNearlyEqual(m, LayoutDefines.NUMBER_PRECISION))
            {
                _minSize = m;
                changedKinds |= ValueKind.MinSize;
            }
            if (!_maxSize.AreNearlyEqual(M, LayoutDefines.NUMBER_PRECISION))
            {
                _maxSize = M;
                changedKinds |= ValueKind.MaxSize;
            }

            return changedKinds;
        }
    }
}
