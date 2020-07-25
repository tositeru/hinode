using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    public enum LayoutTargetAnchorMode
    {
        Point,
        Area,
    }

    public enum ILayoutTargetOnChangedChildMode
    {
        Add,
        Remove,
    }

    public delegate void ILayoutTargetOnDisposed(ILayoutTarget self);
    public delegate void ILayoutTargetOnChangedParent(ILayoutTarget self, ILayoutTarget parent);
    public delegate void ILayoutTargetOnChangedChildren(ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode);
    public delegate void ILayoutTargetOnChangedLocalPos(ILayoutTarget self);
    public delegate void ILayoutTargetOnChangedLocalSize(ILayoutTarget self);

    /// <summary>
	/// RectまたはCubeを表すLayout対象となるオブジェクトのインターフェイス
	///
	/// ### 領域について
	/// 
	/// 自身の領域については以下の図を参考にしてください
	///
	///      | AnchorArea |
	/// -OI--a-----O------A--OA--
	///  |     LocalArea      |
	///   
	/// - AnchorArea: 親領域と関連付けされるAnchorの領域。親のサイズが変更されるとこちらも変更されるように実装してください。
	/// - LocalArea: AnchorAreaにOffset値を追加した領域。こちらが自身の領域となります。
	/// - a: anchorMin
	/// - A: anchorMax
	/// - OI: anchorMinにAnchorOffsetMinを追加したもの
	/// - OA: anchorMaxにAnchorOffsetMaxを追加したもの
	///
	/// ### AnchorMode
	/// 
	/// Anchor位置によって以下の種類分けがなされています。
	/// - Point Mode: 上の図のaとAが重なり合っている時。
	/// - Area Mode: それ以外の時。
	///
	/// AnchorMin/Maxの値は必ずAnchorMin <= AnchorMaxになるように実装してください。
	/// 
	/// <seealso cref="ILayout"/>
	/// <seealso cref="LayoutManager"/>
	/// </summary>
    public interface ILayoutTarget : System.IDisposable
    {
        NotInvokableDelegate<ILayoutTargetOnDisposed> OnDisposed { get; }
        NotInvokableDelegate<ILayoutTargetOnChangedParent> OnChangedParent { get; }
        NotInvokableDelegate<ILayoutTargetOnChangedChildren> OnChangedChildren { get; }
        NotInvokableDelegate<ILayoutTargetOnChangedLocalPos> OnChangedLocalPos { get; }
        NotInvokableDelegate<ILayoutTargetOnChangedLocalSize> OnChangedLocalSize { get; }

        ILayoutTarget Parent { get; }
        IEnumerable<ILayoutTarget> Children { get; }

        Vector3 LocalPos { get; set; }

        Vector3 LocalSize { get; }
        Vector3 AnchorMin { get; }
        Vector3 AnchorMax { get; }
        Vector3 AnchorOffsetMin { get; }
        Vector3 AnchorOffsetMax { get; }

        /// <summary>
		/// LocalSizeに関係するパラメータを一括で更新する関数。
		///
		/// 計算の結果、LocalSizeの各要素が0より下回るようになる時は0になるように実装してください。
		///
		/// OnChangedLocalSizeは一度だけ呼び出すように実装していください。
		/// </summary>
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		/// <param name="offsetMin"></param>
		/// <param name="offsetMax"></param>
        void UpdateLocalSizeWithAnchorParam(Vector3 anchorMin, Vector3 anchorMax, Vector3 offsetMin, Vector3 offsetMax);

        /// <summary>
		/// LocalSizeに関係するパラメータを一括で更新する関数。
		///
		/// localSizeの各要素が0より下回る場合は0になるように補正されるようにしてください。
		/// OnChangedLocalSizeは一度だけ呼び出すように実装していください。
		/// </summary>
		/// <param name="localSize"></param>
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		/// <param name="offsetAnchor">アンカー領域の中央からのオフセット。AnchorOffsetMin/Maxに影響を与えます</param>
        void UpdateLocalSizeWithSizeAndAnchorParam(Vector3 localSize, Vector3 anchorMin, Vector3 anchorMax, Vector3 offsetAnchor);
    }


    public static partial class ILayoutTargetExtensions
    {
        /// <summary>
		/// ParentがNullの場合は0サイズを返します。
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static Vector3 ParentLocalSize(this ILayoutTarget self)
            => self.Parent?.LocalSize ?? Vector3.zero;

        /// <summary>
		/// 現在のアンカー領域のサイズを返します。
		///
		/// AnchorMode() == Pointの場合は0サイズを返します。
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static Vector3 AnchorAreaSize(this ILayoutTarget self)
            => self.ParentLocalSize().Mul(self.AnchorMax - self.AnchorMin);

        /// <summary>
		/// アンカー領域のMin/Max位置を取得する。
		///
		/// この関数の返り値はAnchorOffsetMin/Maxの影響を受けません。
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static (Vector3 min, Vector3 max) AnchorAreaMinMaxPos(this ILayoutTarget self)
        {
            var halfParentAreaSize = self.AnchorAreaSize() * 0.5f;
            return (-halfParentAreaSize, halfParentAreaSize);
        }

        /// <summary>
		/// 自身のローカル領域のMin/Max位置を取得する。
		/// 
		/// AnchorOffsetMin/Maxの影響を受けます。
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static (Vector3 min, Vector3 max) LocalAreaMinMaxPos(this ILayoutTarget self)
        {
            var (anchorMin, anchorMax) = self.AnchorAreaMinMaxPos();
            return (anchorMin - self.AnchorOffsetMin, anchorMax + self.AnchorOffsetMax);
        }

        /// <summary>
		/// LocalSizeを指定したサイズに変更します。
		///
		/// この関数によってAnchorMin/Maxは変更されません。
		/// AnchorOffsetMin/Maxは現在のselfのアンカー空間上の中央位置を変更されないように再計算されます。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
        public static void SetLocalSize(this ILayoutTarget self, Vector3 localSize)
        {
            localSize = Vector3.Max(localSize, Vector3.zero);

            var halfParentAreaSize = self.AnchorAreaSize() * 0.5f;

            var anchorMinPos = -(halfParentAreaSize + self.AnchorOffsetMin);
            var anchorMaxPos = halfParentAreaSize + self.AnchorOffsetMax;
            var anchorCenterPos = (anchorMaxPos + anchorMinPos) * 0.5f;

            var halfLocalSize = localSize * 0.5f;
            var localMinPos = anchorCenterPos - halfLocalSize;
            var localMaxPos = anchorCenterPos + halfLocalSize;

            Vector3 offsetMin, offsetMax;
            switch (self.AnchorMode())
            {
                case LayoutTargetAnchorMode.Point:
                    offsetMin = -1 * localMinPos;
                    offsetMax = localMaxPos;
                    break;
                case LayoutTargetAnchorMode.Area:
                    offsetMin = -1 * (localMinPos + halfParentAreaSize);
                    offsetMax = (localMaxPos - halfParentAreaSize);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            self.UpdateLocalSizeWithAnchorParam(self.AnchorMin, self.AnchorMax, offsetMin, offsetMax);
        }

        /// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithAnchorParam(min, max, self.AnchorOffsetMin, self.AnchorOffsetMax)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
        public static void SetAnchor(this ILayoutTarget self, Vector3 min, Vector3 max)
        {
            self.UpdateLocalSizeWithAnchorParam(min, max, self.AnchorOffsetMin, self.AnchorOffsetMax);
        }

        /// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithAnchorParam(self.AnchorMin, self.AnchorMax, min, max)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
        public static void SetAnchorOffset(this ILayoutTarget self, Vector3 min, Vector3 max)
        {
            self.UpdateLocalSizeWithAnchorParam(self.AnchorMin, self.AnchorMax, min, max);
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static LayoutTargetAnchorMode AnchorMode(this ILayoutTarget self)
        {
            return self.AnchorMin.AreNearlyEqual(self.AnchorMax, LayoutDefines.NUMBER_PRECISION)
                ? LayoutTargetAnchorMode.Point
                : LayoutTargetAnchorMode.Area;
        }
    }
}
