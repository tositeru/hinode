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
	public delegate void ILayoutTargetOnChangedOffset(ILayoutTarget self, Vector3 prevOffset);
	public delegate void ILayoutTargetOnChangedAnchorMinMax(ILayoutTarget self, Vector3 prevAnchorMin, Vector3 prevAnchorMax);
	public delegate void ILayoutTargetOnChangedPivot(ILayoutTarget self, Vector3 prevPivot);
	public delegate void ILayoutTargetOnChangedLayoutInfo(ILayoutTarget self, LayoutInfo.ValueKind kind);

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
	/// ### プロパティと領域について
	/// 
	/// 自身の領域については以下の図を参考にしてください
	///
	/// (※ 親のサイズが変更されるときはAnchorMin/MaxとAnchorOffsetMin/Maxが変更されないように実装してください。)
	/// 
	///    (AnchorAreaSize)
	///    <-     v     ->
	///   |   AnchorArea  |
	/// --a--OI---O--o----A--OA--
	///      |   LocalArea   |
	///       <- LocalSize ->
	/// - AnchorArea: 親領域と関連付けされるAnchorの領域。 ILayoutTargetExtensions#AnchorAreaSizeで取得できます。
	/// - LocalArea: AnchorAreaにOffsetを追加した領域。こちらが自身の領域となります。
	/// - O: 中心. Parentが設定されている場合はその中央になります。
	/// - o: 中心からのOffset		ILayoutTarget#Offset + ILayoutTargetExtensions#PivotOffset
	/// - a: anchorMin			ILayoutTarget#AnchorMin
	/// - A: anchorMax			ILayoutTarget#AnchorMax
	/// - OI: 中心からのOffset - 0.5*LocalSize ILayoutTargetExtensions#LocalAreaMinMaxPos()で取得できます。
	/// - OA: 中心からのOffset + 0.5*LocalSize ILayoutTargetExtensions#LocalAreaMinMaxPos()で取得できます。
	///
	/// AnchorOffsetMin: (a - OI). ILayoutTargetExtensions#AnchorOffsetMinMax()で取得できます。
	/// AnchorOffsetMax: (A - OA). ILayoutTargetExtensions#AnchorOffsetMinMax()で取得できます。
	/// AnchorAreaSize: ILayoutTargetExtensions#AnchorAreaSize()で取得できます。
	/// LocalSize:		ILayoutTarget#LocalSizeで取得できます
	/// ILayoutTarget#LocalPos: oを原点とした時の位置になります。
	/// 
	/// ### AnchorOffsetMin/Max
	/// AnchorOffsetMin/Maxの符号は以下の図を参考にしてください。
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
		NotInvokableDelegate<ILayoutTargetOnChangedOffset> OnChangedOffset { get; }
		NotInvokableDelegate<ILayoutTargetOnChangedPivot> OnChangedPivot { get; }
		NotInvokableDelegate<ILayoutTargetOnChangedLayoutInfo> OnChangedLayoutInfo { get; }
		NotInvokableDelegate<ILayoutTargetOnChangedAnchorMinMax> OnChangedAnchorMinMax { get; }

		ILayoutTarget Parent { get; }
        IEnumerable<ILayoutTarget> Children { get; }
        int ChildCount { get; }

		/// <summary>
        /// 並び順はILayoutTarget内で操作されますので、変更しないようにしてください。
        /// </summary>
		IReadOnlyListHelper<ILayout> Layouts { get; }

		/// <summary>
        /// null値は許容しません。
        /// ILayoutTargetのインスタンスを生成した時に生成し、そのインスタンスを使い回すようにしてください。
        /// </summary>
		LayoutInfo LayoutInfo { get; }

        Vector3 LocalPos { get; set; }

        Vector3 LocalSize { get; }
        Vector3 AnchorMin { get; }
        Vector3 AnchorMax { get; }

		/// <summary>
		/// 実際のOffset値にPivotOffsetを加えた値を返すようにしてください。
		/// </summary>
		Vector3 Offset { get; }
		Vector3 Pivot { get; set; }

		void AddLayout(ILayout layout);
		void RemoveLayout(ILayout layout);

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
		void UpdateAnchorParam(Vector3 anchorMin, Vector3 anchorMax, Vector3 offsetMin, Vector3 offsetMax);

        /// <summary>
		/// LocalSizeに関係するパラメータを一括で更新する関数。
		///
		/// localSizeの各要素が0より下回る場合は0になるように補正されるようにしてください。
		/// OnChangedLocalSizeは一度だけ呼び出すように実装していください。
		/// </summary>
		/// <param name="localSize"></param>
		/// <param name="offset">アンカー領域の中央からのオフセット。AnchorOffsetMin/Maxに影響を与えます</param>
        void UpdateLocalSize(Vector3 localSize, Vector3 offset);
    }


    public static partial class ILayoutTargetExtensions
    {
		/// <summary>
        /// Pivotによるオフセットを計算します。
        /// ILayoutTarget#Offsetの影響はないものになります。
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
		public static Vector3 PivotOffset(this ILayoutTarget self)
        {
			return -self.LocalSize.Mul(self.Pivot - Vector3.one * 0.5f);
		}

		/// <summary>
		/// レイアウト計算用のサイズを返します。
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static Vector3 LayoutSize(this ILayoutTarget self)
        {
			return self.LayoutInfo.GetLayoutSize(self);
        }

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
        {
			var parentLayoutSize = self.Parent != null
				? self.Parent.LayoutInfo.GetLayoutSize(self.Parent)
				: Vector3.zero;
			return parentLayoutSize.Mul(self.AnchorMax - self.AnchorMin);
        }

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

            self.UpdateLocalSize(localSize, self.Offset);
        }

		/// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithSizeAndAnchorParam(self.LocalSize, offset)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="offset"></param>
		public static void SetOffset(this ILayoutTarget self, Vector3 offset)
        {
			self.UpdateLocalSize(self.LocalSize, offset);
		}

		/// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithAnchorParam(min, max, self.AnchorOffsetMin, self.AnchorOffsetMax)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static void SetAnchor(this ILayoutTarget self, Vector3 min, Vector3 max)
        {
			var offset = self.AnchorOffsetMinMax();
			self.UpdateAnchorParam(min, max, offset.offsetMin, offset.offsetMax);
        }

        /// <summary>
		/// この関数はILayoutTarget.UpdateLocalSizeWithAnchorParam(self.AnchorMin, self.AnchorMax, min, max)と同じです。
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
        public static void SetAnchorOffset(this ILayoutTarget self, Vector3 min, Vector3 max)
        {
            self.UpdateAnchorParam(self.AnchorMin, self.AnchorMax, min, max);
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

		/// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
		public static void ClearLayouts(this ILayoutTarget self)
        {
			while(0 < self.Layouts.Count)
            {
				self.RemoveLayout(self.Layouts[self.Layouts.Count-1]);
            }
        }
    }

	/// <summary>
    /// ILayoutのデフォルトIComparer
    /// </summary>
    public class ILayoutDefaultComparer : IComparer<ILayout>
    {
		public readonly static ILayoutDefaultComparer Default = new ILayoutDefaultComparer();

		public int Compare(ILayout x, ILayout y)
        {
			return x.OperationPriority.CompareTo(y.OperationPriority);
        }
    }
}
