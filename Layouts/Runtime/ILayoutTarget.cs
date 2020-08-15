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
    public delegate void ILayoutTargetOnChangedParent(ILayoutTarget self, ILayoutTarget parent, ILayoutTarget prevParent);
    public delegate void ILayoutTargetOnChangedChildren(ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode);
    public delegate void ILayoutTargetOnChangedLocalPos(ILayoutTarget self, Vector3 prevLocalPos);
    public delegate void ILayoutTargetOnChangedLocalSize(ILayoutTarget self, Vector3 prevLocalSize);

    /// <summary>
	/// RectまたはCubeを表すLayout対象となるオブジェクトのインターフェイス
	///
    /// ###　実装上の注意点
    /// 
	/// - System.IDisposable#Dispose()では以下のILayoutTargetに関係するパラメータをクリアーするようにしてください。
	/// ex)
	///   - OnChangeParentなど全てのILayoutTargetのDelegateに設定されているコールバックのクリアー
	///   - ParentがNullを返すようにする
	///   - Childrenが空になるようにする
	///
    /// - 親のLocalSizeが変更された時は、AnchorMin/MaxとAnchorOffsetMin/Maxが保持されるようにパラメータを変更してください。
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
        int ChildCount { get; }

        Vector3 LocalPos { get; set; }

        Vector3 LocalSize { get; }
        Vector3 AnchorMin { get; }
        Vector3 AnchorMax { get; }
        Vector3 Offset { get; }

		/// <summary>
		/// LocalSizeに関係するパラメータを一括で更新する関数。
		/// AnchorMin/MaxとAnchorAreaからのoffsetを指定してください。
		///
		/// 計算の結果は以下の条件を守るようにしてください。
        /// - LocalSize.xyz >= 0
		/// - LocalAreaMin <= LocalAreaMax
        /// 
		/// OnChangedLocalSizeは一度だけ呼び出すように実装していください。
		///
		/// figure.1 AnchorMin/MaxとOffsetの関係
		///     + | - <- AnchorOffsetMin
		/// --m---a--o-O----M-A--
		///               - | + <- AnchorOffsetMax
		/// O: 原点
		/// o: Offset
		/// a: AnchorMin
		/// A: AnchorMax
		/// m: LocalAreaMin
		/// M: LocalAreaMax
		///
		/// figure.2 LocalAreaMin(m) <= LocalAreaMax(M)
		/// + | - <- AnchorOffsetMin
		/// --a---O-m-M----A---
		///              - | + <- AnchorOffsetMax
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
		/// <param name="offset">アンカー領域の中央からのオフセット。AnchorOffsetMin/Maxに影響を与えます</param>
        void UpdateLocalSizeWithSizeAndAnchorParam(Vector3 localSize, Vector3 anchorMin, Vector3 anchorMax, Vector3 offset);
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
            var halfSize = self.LocalSize * 0.5f;
            return (-halfSize + self.Offset, halfSize + self.Offset);
        }

		/// <summary>
		/// AnchorAreaからのLocalSizeのオフセットを返します。
		/// 返される値の正負はAnchorAreaから外側方向に正の数、内側に負の数になっています。
		///
		/// 
		/// + | - <- AnchorOffsetMin
		/// --a-----O-----A---
		///             - | + AnchorOffsetMax
        ///
        /// a: AnchorMinPos
        /// A: AnchorMaxPos
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static (Vector3 offsetMin, Vector3 offsetMax) AnchorOffsetMinMax(this ILayoutTarget self)
		{
			var (min, max) = self.AnchorAreaMinMaxPos();
			var (localMin, localMax) = self.LocalAreaMinMaxPos();
			return (-(localMin - min), localMax - max);
		}

		/// <summary>
		/// LocalSizeを指定したサイズに変更します。
		///
		/// この関数によってAnchorMin/Max, Offsetは変更されません。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		public static void SetLocalSize(this ILayoutTarget self, Vector3 localSize)
        {
            localSize = Vector3.Max(localSize, Vector3.zero);

            self.UpdateLocalSizeWithSizeAndAnchorParam(localSize, self.AnchorMin, self.AnchorMax, self.Offset);
        }

		/// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithSizeAndAnchorParam(self.LocalSize, self.AnchorMin, self.AnchorMax, offset)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="offset"></param>
		public static void SetOffset(this ILayoutTarget self, Vector3 offset)
        {
			self.UpdateLocalSizeWithSizeAndAnchorParam(self.LocalSize, self.AnchorMin, self.AnchorMax, offset);
		}

		/// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithSizeAndAnchorParam(self.LocalSize, min, max, self.Offset)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static void SetAnchor(this ILayoutTarget self, Vector3 min, Vector3 max)
        {
			self.UpdateLocalSizeWithSizeAndAnchorParam(self.LocalSize, min, max, self.Offset);
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
