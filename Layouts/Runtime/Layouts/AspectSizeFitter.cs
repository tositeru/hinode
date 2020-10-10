using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

namespace Hinode.Layouts
{
    /// <summary>
    /// ILayout#TargetのAspect比を固定するILayout.
    /// 2D空間(XY)上のみを対象にしています。
    /// Z値は処理対象にはなりませんので注意してください。
    /// Target#Parentがnullの時はTargetのLocalSize/Offsetを0に設定します。
    ///
    /// Aspect比は縦/横の比率になります。(ただしFixedHeightの時はその反対になります。)
    /// 
    /// ### 変形タイミング
    /// 変形タイミングとしては(DoChangedがtrueになる時は)
    ///
    /// - ILayout#Targetが変更された時
    /// - ILayoutTarget#Parentが変更された時
    /// - パラメータが変更された時
    /// - ILayout#ParentのLocalSizeが変更された時。
    /// - ILayoutTarget#LocalSize/Offsetが変更された時。
    /// - ILayoutTarget#AnchorMin,AnchorMaxが変更された時。
    /// 
    /// ILayoutTarget#Parentの領域内に収まるように変形します。
    /// ILayoutTarget#LocalSize/Offsetを制御します。
    ///
    /// ILayoutTarget#LocalPosは制御しませんので、位置ズレによるILayoutTarget#Parentの基準領域からのはみ出しは保証しません。
    ///
    /// ### 変形モード
    /// 以下の変形モードがあります。
    ///
    /// - ParentFit: ILayoutTarget#Parentの基準領域一杯に広げるように変形します。
    /// - AnchorFit: ILayoutTarget#AnchorMin/AnchorMax内一杯に広げるように変形します。
    /// - FixedWidth: 指定した横幅を固定したまま変形します。もし、基準領域からはみ出してしまう場合ははみ出さないように変形します。
    /// - FixedHeight: 指定した縦幅を固定したまま変形します。もし、基準領域からはみ出してしまう場合ははみ出さないように変形します。
    ///
    /// FixedWidth/Heightのサイズ固定はPaddingを適応した後の基準領域を使用して処理されます。
    /// 
    /// ### Padding
    /// Paddingを指定した時は、何も指定しなかった時の領域を基準にPaddingが適応されます。
    /// Padding#CurrentUnit == Ratioの場合は 各々のCurrentModeの基準領域を1とした比率で計算されます。
    ///
    /// <seealso cref="ILayout"/>
    /// <seealso cref="LayoutBase"/>
    /// </summary>
    [System.Serializable]
    public class AspectSizeFitter : LayoutBase
    {
        public static int DefaultOperationPriority { get => 1000; }

        public enum Mode
        {
            ParentFit,
            AnchorFit,
            FixedWidth,
            FixedHeight,
        }

        [SerializeField] Mode _currentMode = Mode.ParentFit;
        [SerializeField, Min(0.0000001f)] float _aspectRatio = 1;
        [SerializeField] float _fixedLength = 100;
        [SerializeField] LayoutOffset _padding = new LayoutOffset();

        public Mode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode == value) return;
                _currentMode = value;
                DoChanged = true;
            }
        }

        /// <summary>
        /// 縦/横の比率になります。
        /// CurrentMode == FixedHeightの時は横/縦の比率になります。
        /// </summary>
        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                var v = Max(LayoutDefines.NUMBER_PRECISION, value);
                if (MathUtils.AreNearlyEqual(_aspectRatio, v, float.Epsilon)) return;
                _aspectRatio = v;
                DoChanged = true;
            }
        }

        public float FixedLength
        {
            get => _fixedLength;
            set
            {
                var v = Max(0f, value);
                if (MathUtils.AreNearlyEqual(_fixedLength, v, float.Epsilon)) return;
                _fixedLength = v;
                if (CurrentMode == Mode.FixedHeight || CurrentMode == Mode.FixedWidth)
                {
                    DoChanged = true;
                }
            }
        }

        public LayoutOffset Padding
        {
            get => _padding;
            set
            {
                if (_padding == value || value == null) return;
                _padding.SetOffsets(value.Left, value.Right, value.Top, value.Bottom);
                //DoChangedへの反映はLayoutOffset#OnChangedValueで行っています。
            }
        }

        public AspectSizeFitter()
        {
            OperationPriority = DefaultOperationPriority;
            InnerOnChangedTarget(Target, null);
            _padding.OnChangedValue.Add(OffsetOnChanged);
        }

        void OffsetOnChanged(LayoutOffset self, LayoutOffset.ValueKind kinds)
        {
            if (self != _padding) return;
            DoChanged = true;
        }

        #region LayoutBase
        public override LayoutOperationTarget OperationTargetFlags
        {
            get => LayoutOperationTarget.Self_LocalSize | LayoutOperationTarget.Self_Offset;
        }

        public override bool Validate()
        {
            if (Target == null || Target.Parent == null) return false;

            //Parentの判定
            var selfFlags = (int)OperationTargetFlags;
            foreach (var l in Target.Parent.Layouts)
            {
                int parentFlags = (int)l.OperationTargetFlags;
                parentFlags >>= 5;
                if (0 != (parentFlags & selfFlags))
                    return false;
            }

            //自身の判定
            foreach(var otherLayout in Target.Layouts)
            {
                if (otherLayout == this) return true;

                if (0 != (otherLayout.OperationTargetFlags & OperationTargetFlags))
                    return false;
            }
            return true;
        }

        public override void UpdateLayout()
        {
            if (!DoChanged) return;
            if (!Validate())
            {
                if(Target != null && Target.Parent == null)
                {
                    var s = Vector3.zero;
                    s.z = Target.LocalSize.z;
                    var o = Vector3.zero;
                    o.z = Target.Offset.z;
                    Target.UpdateLocalSize(s, o);
                }

                DoChanged = false;
                return;
            }

            var parent = Target.Parent;

            Vector3 layoutSize = Vector3.zero;
            switch (CurrentMode)
            {
                case Mode.ParentFit:
                    layoutSize = parent.LayoutSize();
                    break;
                case Mode.AnchorFit:
                case Mode.FixedWidth:
                case Mode.FixedHeight:
                    layoutSize = parent.LayoutSize().Mul(Target.AnchorMax - Target.AnchorMin);
                    break;
            }
            var (baseSize, offset) = CalSizeAndOffset(layoutSize, Padding);
            //Paddingを適用した後にFixedLengthを適応すします
            var size = AdjustSize(baseSize, AspectRatio);

            offset.z = Target.Offset.z;
            size.z = Target.LocalSize.z;
            Target.UpdateLocalSize(size, offset);
            DoChanged = false;
        }
        #endregion

        protected (Vector3 size, Vector3 offset) CalSizeAndOffset(Vector3 baseSize, LayoutOffset padding)
        {
            Vector3 size = baseSize;
            Vector3 offset = Vector3.zero;
            switch (padding.CurrentUnit)
            {
                case LayoutOffset.Unit.Pixel:
                    size.x -= padding.Left + padding.Right;
                    size.y -= padding.Top + padding.Bottom;

                    offset = new Vector3(
                        0.5f * (padding.Left - padding.Right),
                        0.5f * (padding.Bottom - padding.Top),
                        0
                    );
                    break;
                case LayoutOffset.Unit.Ratio:
                    size.x -= baseSize.x * (padding.Left + padding.Right);
                    size.y -= baseSize.y * (padding.Top + padding.Bottom);

                    offset = new Vector3(
                        0.5f * baseSize.x * (padding.Left - padding.Right),
                        0.5f * baseSize.y * (padding.Bottom - padding.Top),
                        0
                    );
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            return (size, offset);
        }

        protected Vector3 AdjustSize(Vector3 baseSize, float aspectRatio)
        {
            Vector3 size;
            switch (CurrentMode)
            {
                case Mode.ParentFit:
                case Mode.AnchorFit:
                case Mode.FixedWidth:
                    size = baseSize;
                    var height = baseSize.x * aspectRatio;
                    if (size.y < height)
                    {
                        size.x = size.y / Max(LayoutDefines.NUMBER_PRECISION, aspectRatio);
                    }
                    else
                    {
                        size.y = height;
                    }

                    if (CurrentMode == Mode.FixedWidth)
                    {
                        if (FixedLength <= size.x && FixedLength * AspectRatio <= size.y)
                        {
                            size.x = FixedLength;
                            size.y = FixedLength * AspectRatio;
                        }
                    }
                    break;
                case Mode.FixedHeight:
                    size = baseSize;
                    var width = baseSize.y * aspectRatio;
                    if (size.x < width)
                    {
                        size.y = size.x / Max(LayoutDefines.NUMBER_PRECISION, aspectRatio);
                    }
                    else
                    {
                        size.x = width;
                    }

                    if (FixedLength <= size.y && FixedLength * AspectRatio <= size.x)
                    {
                        size.y = FixedLength;
                        size.x = FixedLength * AspectRatio;
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return size;
        }


        #region override LayoutBase
        protected override void InnerOnChangedTarget(ILayoutTarget current, ILayoutTarget prev)
        {
            if(prev != null)
            {
                prev.OnChangedParent.Remove(TargetOnChangedParent);
                prev.OnChangedLocalSize.Remove(TargetOnChangedLocalSize);
                prev.OnChangedOffset.Remove(TargetOnChangedOffset);
                prev.OnChangedAnchorMinMax.Remove(TargetOnChangedAnchorMinMax);
                prev.Parent?.OnChangedLocalSize.Remove(TargetParentOnChangedLocalSize);
            }

            if (current != null)
            {
                current.OnDisposed.Add(TargetOnDisposed);
                current.OnChangedParent.Add(TargetOnChangedParent);
                current.OnChangedLocalSize.Add(TargetOnChangedLocalSize);
                current.OnChangedOffset.Add(TargetOnChangedOffset);
                current.OnChangedAnchorMinMax.Add(TargetOnChangedAnchorMinMax);
                current.Parent?.OnChangedLocalSize.Add(TargetParentOnChangedLocalSize);
            }
        }

        //protected override void InnerOnChanged(bool doChanged)
        //{
        //}

        void TargetOnDisposed(ILayoutTarget self)
        {
            if (self != Target) return;
            DoChanged = true;
        }

        void TargetOnChangedParent(ILayoutTarget self, ILayoutTarget parent, ILayoutTarget prevParent)
        {
            if (self != Target) return;
            prevParent?.OnChangedLocalSize.Remove(TargetParentOnChangedLocalSize);
            parent?.OnChangedLocalSize.Add(TargetParentOnChangedLocalSize);
            DoChanged = true;
        }

        void TargetOnChangedLocalSize(ILayoutTarget self, Vector3 prev)
        {
            if (self != Target) return;
            DoChanged = true;
        }

        void TargetOnChangedOffset(ILayoutTarget self, Vector3 prev)
        {
            if (self != Target) return;
            DoChanged = true;
        }

        void TargetOnChangedAnchorMinMax(ILayoutTarget self, Vector3 prevAnchorMin, Vector3 prevAnchorMax)
        {
            if (self != Target) return;
            DoChanged = true;
        }
        void TargetParentOnChangedLocalSize(ILayoutTarget self, Vector3 prevLocalSize)
        {
            if (Target.Parent != self) return;
            DoChanged = true;
        }

        #endregion
    }
}
