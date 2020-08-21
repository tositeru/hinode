using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// 2D空間のオフセットを表すクラス
    /// 以下の単位を指定できます。
    /// - Pixel
    /// - Ratio
    /// <seealso cref="Hinode.Layouts.Tests.TestLayoutOffset"/>
    /// </summary>
    [System.Serializable]
    public class LayoutOffset
    {
        public delegate void OnChangedValueDelegate(LayoutOffset self, ValueKind kinds);

        [System.Flags]
        public enum ValueKind
        {
            CurrentUnit = 0x1 << 0,
            Left = 0x1 << 1,
            Right = 0x1 << 2,
            Top = 0x1 << 3,
            Bottom = 0x1 << 4,
        }

        public enum Unit
        {
            Pixel, // ピクセル
            Ratio, // 比率
        }

        SmartDelegate<OnChangedValueDelegate> _onChangedValue = new SmartDelegate<OnChangedValueDelegate>();

        [SerializeField] Unit _unit = Unit.Pixel;
        [SerializeField] float _left = 0;
        [SerializeField] float _right = 0;
        [SerializeField] float _top = 0;
        [SerializeField] float _bottom = 0;

        public NotInvokableDelegate<OnChangedValueDelegate> OnChangedValue { get => _onChangedValue; }

        public Unit CurrentUnit
        {
            get => _unit;
            set
            {
                if (_unit == value) return;
                _unit = value;
                InvokeOnChangedValue(ValueKind.CurrentUnit);
            }
        }

        public float Left
        {
            get => _left;
            set
            {
                if (AssignValue(ref _left, value))
                {
                    InvokeOnChangedValue(ValueKind.Left);
                }
            }
        }

        public float Right
        {
            get => _right;
            set
            {
                if (AssignValue(ref _right, value))
                {
                    InvokeOnChangedValue(ValueKind.Right);
                }
            }
        }

        public float Top
        {
            get => _top;
            set
            {
                if (AssignValue(ref _top, value))
                {
                    InvokeOnChangedValue(ValueKind.Top);
                }
            }
        }

        public float Bottom
        {
            get => _bottom;
            set
            {
                if(AssignValue(ref _bottom, value))
                {
                    InvokeOnChangedValue(ValueKind.Bottom);
                }
            }
        }

        static bool AssignValue(ref float target, float value)
        {
            if (MathUtils.AreNearlyEqual(target, value, LayoutDefines.POS_NUMBER_PRECISION)) return false;
            target = value;
            return true;
        }

        public LayoutOffset SetOffsets(float left, float right, float top, float bottom)
        {
            ValueKind changedValueKind = 0;
            changedValueKind |= AssignValue(ref _left, left) ? ValueKind.Left : 0;
            changedValueKind |= AssignValue(ref _right, right) ? ValueKind.Right : 0;
            changedValueKind |= AssignValue(ref _top, top) ? ValueKind.Top : 0;
            changedValueKind |= AssignValue(ref _bottom, bottom) ? ValueKind.Bottom : 0;

            if(changedValueKind != 0)
            {
                InvokeOnChangedValue(changedValueKind);
            }
            return this;
        }

        public LayoutOffset SetHorizontalOffsets(float left, float right)
        {
            return SetOffsets(left, right, Top, Bottom);
        }

        public LayoutOffset SetVerticalOffsets(float top, float bottom)
        {
            return SetOffsets(Left, Right, top, bottom);
        }

        void InvokeOnChangedValue(ValueKind kinds)
        {
            try
            {
                _onChangedValue.Instance?.Invoke(this, kinds);
            }
            catch (System.Exception e)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Exception!! LayoutOffset#OnChangedValue valueKinds={kinds}{System.Environment.NewLine}{e.Message}", LayoutDefines.LOG_SELECTOR);
            }
        }
    }
}
