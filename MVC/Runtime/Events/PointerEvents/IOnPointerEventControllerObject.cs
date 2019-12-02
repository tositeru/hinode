using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// PointerEventDispatcherでオブジェクトの領域などを取得するために利用されるオブジェクト
    ///
    /// このクラスのインスタンスはModelViewBinderInstance内で管理されます。
    /// <seealso cref="ModelViewBinderInstance"/>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public interface IOnPointerEventHelpObject : IEventDispatcherHelper
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
    public class IOnPointerEventControllerObjectComparer : IComparer<IOnPointerEventHelpObject>
    {
        public Camera UseCamera { get; }
        public IOnPointerEventControllerObjectComparer(Camera useCamera)
        {
            Assert.IsNotNull(useCamera);
            UseCamera = useCamera;
        }

        #region IComparer<IOnPointerEventControllerObject>
        public int Compare(IOnPointerEventHelpObject left, IOnPointerEventHelpObject right)
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

        int CompareBothScreenOverlay(IOnPointerEventHelpObject left, IOnPointerEventHelpObject right)
        {
            Assert.IsNotNull(left.RootCanvas);
            Assert.IsNotNull(right.RootCanvas);
            Assert.IsNotNull(left.Transform);
            Assert.IsNotNull(right.Transform);
            Assert.IsTrue(left.IsScreenOverlay);
            Assert.IsTrue(right.IsScreenOverlay);
            if (left == right || left.Transform == right.Transform)
            {
                return 0;
            }

            var leftRootCanvas = left.RootCanvas;
            var rightRootCanvas = right.RootCanvas;
            if (leftRootCanvas.sortingOrder > rightRootCanvas.sortingOrder) return -1;
            if (leftRootCanvas.sortingOrder < rightRootCanvas.sortingOrder) return 1;

            if (leftRootCanvas == rightRootCanvas)
            {
                return CompareObjectHierarchyBySameRoot(leftRootCanvas.transform, left.Transform, right.Transform);
            }
            else
            {
                return leftRootCanvas.transform.GetSiblingIndex()
                    .CompareTo(rightRootCanvas.transform.GetSiblingIndex());
            }
        }

        int CompareBothInWorld(IOnPointerEventHelpObject left, IOnPointerEventHelpObject right)
        {
            Assert.IsFalse(left.IsScreenOverlay);
            Assert.IsFalse(right.IsScreenOverlay);
            Assert.IsNotNull(left.Transform);
            Assert.IsNotNull(right.Transform);
            if (left == right || left.Transform == right.Transform)
                return 0;

            if (left.RootCanvas != null && right.RootCanvas != null)
            {
                if (left.RootCanvas == right.RootCanvas)
                {
                    return CompareObjectHierarchyBySameRoot(left.RootCanvas.transform, left.Transform, right.Transform);
                }
                else
                {
                    return CompareWithCameraDistance(left.RootCanvas.transform, right.RootCanvas.transform);
                }
            }
            else
            {//どちらかがCanvas内のオブジェクトとは異なる場合
                var leftTransform = left.RootCanvas != null ? left.RootCanvas.transform : left.Transform;
                var rightTransform = right.RootCanvas != null ? right.RootCanvas.transform : right.Transform;
                return CompareWithCameraDistance(leftTransform, rightTransform);
            }
        }

        int CompareObjectHierarchyBySameRoot(Transform root, Transform left, Transform right)
        {
            var first = root.transform.GetHierarchyEnumerable()
                .First(_t => _t == left || _t == right);
            return first == left
                ? 1
                : -1;
        }

        int CompareWithCameraDistance(Transform left, Transform right)
        {
            return (left.position - UseCamera.transform.position).sqrMagnitude
                .CompareTo((right.position - UseCamera.transform.position).sqrMagnitude);
        }
    }
}
