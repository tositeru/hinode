using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// PointerEventDispatcherでオブジェクトの領域などを取得するために利用されるオブジェクト
    ///
    /// このクラスのインスタンスはModelViewBinderInstance内で管理されます。
    /// <seealso cref="ModelViewBinderInstance"/>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public interface IOnPointerEventControllerObject : IControllerObject
    {
        bool IsScreenOverlay { get; }
        bool IsOnPointer(Vector3 screenPos, Camera useCamera);
        Canvas RootCanvas { get; }
        Transform Transform { get; }
    }

    /// <summary>
    /// IOnPointerEventControllerObjectを以下の優先順位でソートします。
    ///   - Canvas(ScreenOverlay)
    ///     - RootCanvasのsortingOrderが大きい方
    ///     - シーン上の親子階層でしたの方にあるもの
    ///   - Other
    ///     - UseCameraの位置から近い方
    ///
    /// NOTE: 実際にOnPointerEventを処理する時はUseCameraからのRaycastも併用されます。
    /// </summary>
    public class IOnPointerEventControllerObjectComparer : IComparer<IOnPointerEventControllerObject>
    {
        public Camera UseCamera { get; }
        public IOnPointerEventControllerObjectComparer(Camera useCamera)
        {
            UseCamera = useCamera;
        }

        #region IComparer<IOnPointerEventControllerObject>
        public int Compare(IOnPointerEventControllerObject left, IOnPointerEventControllerObject right)
        {
            if (left.IsScreenOverlay && !right.IsScreenOverlay)
                return -1;
            if (!left.IsScreenOverlay && right.IsScreenOverlay)
                return 1;

            if (left.IsScreenOverlay && right.IsScreenOverlay)
            {
                return CompareBothScreenOverlay(left, right);
            }
            else
            {
                return CompareBothInWorld(left, right);
            }
        }
        #endregion

        int CompareBothScreenOverlay(IOnPointerEventControllerObject left, IOnPointerEventControllerObject right)
        {
            Assert.IsNotNull(left.RootCanvas);
            Assert.IsNotNull(right.RootCanvas);
            Assert.IsNotNull(left.Transform);
            Assert.IsNotNull(right.Transform);
            Assert.IsTrue(left.IsScreenOverlay);
            Assert.IsTrue(right.IsScreenOverlay);
            if (left == right || left.Transform == right.Transform)
                return 0;

            var leftRootCanvas = left.RootCanvas;
            var rightRootCanvas = right.RootCanvas;
            if (leftRootCanvas.sortingOrder > rightRootCanvas.sortingOrder) return -1;
            if (leftRootCanvas.sortingOrder < rightRootCanvas.sortingOrder) return 1;

            if (leftRootCanvas == rightRootCanvas)
            {
                var first = leftRootCanvas.transform.GetHierarchyEnumerable()
                    .First(_t => _t == left.Transform || _t == right.Transform);
                return first == left.Transform
                    ? 1
                    : -1;
            }
            else
            {
                return leftRootCanvas.transform.GetSiblingIndex().CompareTo(rightRootCanvas.transform.GetSiblingIndex());
            }
        }

        int CompareBothInWorld(IOnPointerEventControllerObject left, IOnPointerEventControllerObject right)
        {
            Assert.IsFalse(left.IsScreenOverlay);
            Assert.IsFalse(right.IsScreenOverlay);
            Assert.IsNotNull(left.Transform);
            Assert.IsNotNull(right.Transform);
            if (left == right || left.Transform == right.Transform)
                return 0;

            return (left.Transform.position - UseCamera.transform.position).sqrMagnitude
                .CompareTo(right.Transform.position - UseCamera.transform.position);
        }
    }
}
