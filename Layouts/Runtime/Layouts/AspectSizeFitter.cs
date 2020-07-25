using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
	/// ILayout#TargetのAspect比を固定するILayout.
	/// 2D空間(XY)上のみを対象にしています。
	/// 
	/// 変形タイミングとしては
	///
	/// - ILayout#Parentの基準領域が変更された時。
	/// - (AnchorFitのみ) ILayout#AnchorMin,AnchorMaxが変更された時。
	/// 
	/// ILayout#Parentの基準領域内に収まるように変形します。
	/// ILayoutTarget#Sizeのみ制御します。
	///
	/// ILayoutTarget#LocalPosは制御しませんので、位置ズレによるILayout#Parentの基準領域からのはみ出しは保証しません。
	///
	/// 以下の変形モードがあります。
	///
	/// - ParentFit: ILayout#Parentの基準領域一杯に広げるように変形します。
	/// - AnchorFit: ILayout#AnchorMin/AnchorMax内一杯に広げるように変形します。
	/// - FixedWidth: 指定した横幅を固定したまま変形します。もし、ILayout#Parentの基準領域からはみ出してしまう場合ははみ出さないように変形します。
	/// - FixedHeight: 指定した縦幅を固定したまま変形します。もし、ILayout#Parentの基準領域からはみ出してしまう場合ははみ出さないように変形します。
	/// <seealso cref="ILayout"/>
	/// <seealso cref="LayoutBase"/>
	/// </summary>
    public class AspectSizeFitter : LayoutBase
    {
        public enum Mode
        {
            ParentFit,
            AnchorFit,
            FixedWidth,
            FixedHeight,
        }

        public Mode CurrentMode { get; set; } = Mode.ParentFit;
        public float AspectRatio { get; set; } = 1;

        public float FixedLength { get; set; } = 100;

        #region LayoutBase
        public override bool DoChanged { get => throw new System.NotImplementedException(); }
        public override Vector3 UnitSize { get => throw new System.NotImplementedException(); }

        public override void UpdateUnitSize()
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateLayout()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
