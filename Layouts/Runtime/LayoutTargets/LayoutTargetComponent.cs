using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// TransformとLayoutTargetObjectのハブComponent
    ///
    /// Layout計算に関連するGameObjectのみにこのComponentをアタッチするようにしてください。
    ///
    /// LayoutTargetObjectの親子階層に関しては自動的に設定しませんので、任意の場所で設定してください。
    /// その際はDoChangedParent/DoChangedChildrenプロパティを利用すると便利です。
    /// 
	/// 更新タイミングをMonoBehaviour依存にしないために、CopyToLayoutTarget()/CopyToTransfromでLayoutTargetObject/Transformのパラメータを更新するようにしてます。
	/// 
	/// <seealso cref="ILayoutTarget"/>
    /// <seealso cref="LayoutTargetObject"/>
	/// <seealso cref="LayoutManagerComponent"/>
	/// </summary>
    public class LayoutTargetComponent : MonoBehaviour
    {
        public static LayoutTargetComponent GetOrAdd(GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<LayoutTargetComponent>();
        }

        ILayoutTargetUpdater _updater;
        public ILayoutTargetUpdater Updater
        {
            get => _updater;
            set => _updater = value;
        }

        /// <summary>
        /// <see cref="ResetChangedHierarchyFlags()"/>
        /// </summary>
        public bool DoChangedParent { get; private set; } = false;

        /// <summary>
        /// <see cref="ResetChangedHierarchyFlags()"/>
        /// </summary>
        public bool DoChangedChildren { get; private set; } = false;

        public RectTransform R { get => transform as RectTransform; }


        LayoutTargetObject _target;
        public LayoutTargetObject LayoutTarget
        {
            get
            {
                if(_target == null)
                {
                    _target = new LayoutTargetObject();
                    CopyToLayoutTarget();
                }
                return _target;
            }
        }

        public void AutoDetectUpdater()
        {
            if(transform is RectTransform)
            {
                _updater = new RectTransformUpdater();
            }
            else
            {
                _updater = null;
            }
        }

        public void ResetChangedHierarchyFlags()
        {
            DoChangedParent = false;
            DoChangedChildren = false;
        }

        /// <summary>
        /// TransformのパラメータをLayoutTargetにコピーする
        ///
        /// これはLayoutManagerComponent内でレイアウト計算を行う前に呼び出されることを想定されて実装されています。
        /// </summary>
        public void CopyToLayoutTarget()
        {
            if (Updater == null) return;
            Updater.CopyToLayoutTarget(this);
        }

        /// <summary>
        /// LayoutTargetのパラメータをTransformにコピーする
        /// 
        /// これはLayoutManagerComponent内でレイアウト計算が完了された時に呼び出されることを想定されて実装されています。
        /// </summary>
        public void CopyToTransform()
        {
            if (Updater == null) return;
            Updater.CopyToTransform(this);
        }

        #region Unity Callbacks
        private void Start()
        {
            AutoDetectUpdater();
        }

        void OnDestroy()
        {
            LayoutTarget.Dispose();
        }

        private void OnTransformParentChanged()
        {
            DoChangedParent = true;
        }

        private void OnTransformChildrenChanged()
        {
            DoChangedChildren = true;
        }
        #endregion

        public interface ILayoutTargetUpdater
        {
            void CopyToLayoutTarget(LayoutTargetComponent self);
            void CopyToTransform(LayoutTargetComponent self);
        }

        public class RectTransformUpdater : ILayoutTargetUpdater
        {
            public RectTransform GetR(Transform transform)
                => transform as RectTransform;

            public void CopyToLayoutTarget(LayoutTargetComponent self)
            {
                //RのPivotを考慮にいれる必要あり
                var R = GetR(self.transform);
                var ROffset = CalROffset(self);

                self.LayoutTarget.UpdateLocalSizeWithSizeAndAnchorParam(R.rect.size, R.anchorMin, R.anchorMax, ROffset);

                self.LayoutTarget.LocalPos = R.anchoredPosition3D;
            }

            public void CopyToTransform(LayoutTargetComponent self)
            {
                //RのPivotを考慮にいれる必要あり
                var R = GetR(self.transform);
                var layout = self.LayoutTarget;
                var ROffset = CalROffset(self);

                R.anchorMin = layout.AnchorMin;
                R.anchorMax = layout.AnchorMax;
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layout.LocalSize.x);
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.LocalSize.y);
                R.anchoredPosition3D = layout.LocalPos + layout.Offset - (Vector3)ROffset;
            }

            /// <summary>
            /// LayoutTargetObjectのLocalPos、Offsetと
            /// RectTransformのpositionの相互変換のためオフセットを計算する。
            ///
            /// LayoutTargetObject -> RectTransformへのオフセットを表します。
            /// </summary>
            /// <param name="R"></param>
            /// <returns></returns>
            Vector2 CalROffset(LayoutTargetComponent self)
            {
                var R = GetR(self.transform);
                var layout = self.LayoutTarget;
                var parentR = R.parent as RectTransform;

                var anchorSize = parentR != null
                    ? parentR.rect.size.Mul(layout.AnchorMax - layout.AnchorMin)
                    : Vector2.zero;
                var ROffset = anchorSize.Mul(R.pivot - Vector2.one * 0.5f);
                var pivotOffset = R.rect.size.Mul(R.pivot - Vector2.one * 0.5f);
                ROffset += -pivotOffset;
                return ROffset;
            }
        }
    }
}
