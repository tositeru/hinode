using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Layouts
{
    /// <summary>
    /// TransformとLayoutTargetObjectのハブComponent
    ///
    /// Layout計算に関連するGameObjectのみにこのComponentをアタッチするようにしてください。
    ///
    /// ### 親子階層の更新について
    /// 
    /// LayoutTargetの親子階層の更新はUpdateLayoutTargetHierachy関数を使用してください。
    /// その際に、以下の条件を満たすGameObjectがある場合はダミーのILayoutTargetを生成し、LayoutTargetと関連付けします。
    /// - RectTransformである。
    /// - GameObjectにLayoutTargetComponentをアタッチされていない
    ///
    /// ### TransformとLayoutTargetのパラメータの連動について
    /// 
    /// TransformとLayoutTargetのパラメータを連動を行いたい場合は以下の関数を使用してください。
    /// パラメータの連動は自動的に行われませんので注意してください。
    /// - CopyToLayoutTarget() : TransformのパラメータをLayoutTargetにコピーする
    /// - CopyToTransfrom() : LayoutTargetのパラメータをTransformにコピーする
    /// 
    /// <seealso cref="ILayoutTarget"/>
    /// <seealso cref="LayoutTargetObject"/>
    /// <seealso cref="LayoutManagerComponent"/>
    /// </summary>
    [DisallowMultipleComponent()]
    public class LayoutTargetComponent : MonoBehaviour
    {
        public delegate void OnDestroyedCallback(LayoutTargetComponent layoutTarget);

        public static LayoutTargetComponent GetOrAdd(GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<LayoutTargetComponent>();
        }

        SmartDelegate<OnDestroyedCallback> _onDestroyed = new SmartDelegate<OnDestroyedCallback>();
        public NotInvokableDelegate<OnDestroyedCallback> OnDestroyed { get => _onDestroyed; }

        ILayoutTargetUpdater _updater;
        public ILayoutTargetUpdater Updater
        {
            get => _updater;
            set => _updater = value;
        }

        public RectTransform R { get => transform as RectTransform; }

        [SerializeField] LayoutTargetObject _target;
        public LayoutTargetObject LayoutTarget
        {
            get
            {
                if (_target == null)
                {
                    _target = new LayoutTargetObject();
                    AutoDetectUpdater();
                    CopyToLayoutTarget();
                }
                return _target;
            }
        }

        public void AutoDetectUpdater()
        {
            if(transform is RectTransform)
            {
                if(!(_updater is RectTransformUpdater)) _updater = new RectTransformUpdater(this);
            }
            else
            {
                _updater = null;
            }
        }

        /// <summary>
        /// TransformのパラメータをLayoutTargetにコピーする
        /// この関数はレイアウト計算を行う前に呼び出されることを想定されて実装されています。
        /// 
        /// ダミーのLayoutTargetを持っている場合はそちらもコピーします。
        ///
        /// コピーの順序は以下のものになります。
        /// - Dummy Parent
        /// - Self
        /// - Dummy Children
        /// </summary>
        public void CopyToLayoutTarget(bool isOnlyDummy = false)
        {
            if(Updater == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Don't Copy To LayoutTarget because Updater is Null...", LayoutDefines.LOG_SELECTOR);
                return;
            }

            _parentDummy?.CopyFromTransform();

            if(!isOnlyDummy)
            {
                Updater.CopyToLayoutTarget(this);
            }

            foreach(var child in _dummyChildren.Values)
            {
                child.CopyFromTransform();
            }
        }

        /// <summary>
        /// LayoutTargetのパラメータをTransformにコピーします。
        /// この関数はレイアウト計算が完了された時に呼び出されることを想定されて実装されています。
        /// 
        /// ダミーのLayoutTargetを持っている場合はそちらもコピーします。
        ///
        /// コピーの順序は以下のものになります。
        /// - Dummy Parent
        /// - Self
        /// - Dummy Children
        /// </summary>
        public void CopyToTransform()
        {
            if (Updater == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Don't Copy To Transform because Updater is Null...", LayoutDefines.LOG_SELECTOR);
                return;
            }
            _parentDummy?.CopyToTransform();

            Updater.CopyToTransform(this);

            foreach (var child in _dummyChildren.Values)
            {
                child.CopyToTransform();
            }
        }

        #region Dummy LayoutTargets
        class DummyInfo : System.IDisposable
        {
            public RectTransform Target { get; private set; }
            public LayoutTargetObject Dummy { get; private set; } = new LayoutTargetObject();
            public int InstanceCounter { get; set; } = 0;

            public DummyInfo(RectTransform target)
            {
                Assert.IsNotNull(target);
                Target = target;
                RectTransformUpdater.CopyToLayoutTarget(target, Dummy);
            }

            public void CopyFromTransform()
            {
                RectTransformUpdater.CopyToLayoutTarget(Target, Dummy);
            }

            public void CopyToTransform()
            {
                RectTransformUpdater.CopyToTransform(Target, Dummy);
            }

            public void Dispose()
            {
                Target = null;
                Dummy.Dispose();
                Dummy = null;
            }
        }

        /// <summary>
        /// ダミーの削除判定に使用します。
        /// </summary>
        int DummyInstanceCounter { get; set; } = 0;
        DummyInfo _parentDummy = null;
        Dictionary<Transform, DummyInfo> _dummyChildren = new Dictionary<Transform, DummyInfo>();

        /// <summary>
        /// LayoutTargetの親子階層の更新を行う
        /// </summary>
        public void UpdateLayoutTargetHierachy()
        {
            //余分なComponentをアタッチするのを避けるように実装しています。
            UpdateParentLayoutTarget();
            UpdateChildrenLayoutTarget();
        }

        public ILayoutTarget GetDummyLayoutTarget(Transform obj)
        {
            if (_parentDummy?.Target == obj) return _parentDummy.Dummy;

            return _dummyChildren.ContainsKey(obj)
                ? _dummyChildren[obj].Dummy
                : null;
        }

        /// <summary>
        /// 以下の条件に従い、_dummyParentとLayoutTarget#Parentを設定します。
        ///
        /// - when transform#parent has LayoutTargetComponent
        ///   parentDummy = null
        ///   LayoutTarget#Parent = transform#parent.GetComponent<LayoutTargetComponent>()
        /// 
        /// - when transform#parent don't has LayoutTargetComponent
        ///   - when transform#parent is RectTransform
        ///     - when parentDummy == null
        ///         parentDummy = new(transform#parent)
        ///         LayoutTarget#Parent = parentDummy
        ///     - when parentDummy#Target != transform#parent
        ///         parentDummy = new(transform#parent)
        ///         LayoutTarget#Parent = parentDummy
        ///   - when  transform#parent is not RectTransform
        ///     parentDummy = null
        ///     LayoutTarget#Parent = null
        /// </summary>
        void UpdateParentLayoutTarget()
        {
            if (transform.parent != null)
            {
                var parentR = transform.parent as RectTransform;
                if (transform.parent.TryGetComponent<LayoutTargetComponent>(out var parentLayoutTarget))
                {
                    _parentDummy?.Dispose();
                    _parentDummy = null;
                    LayoutTarget.SetParent(parentLayoutTarget.LayoutTarget);
                }
                else if (parentR == null)
                {
                    if (_parentDummy != null)
                    {
                        _parentDummy.Dispose();
                        _parentDummy = null;
                        LayoutTarget.SetParent(null);
                    }
                }
                else if (_parentDummy == null || _parentDummy.Target != transform.parent)
                {
                    _parentDummy = new DummyInfo(parentR);
                    LayoutTarget.SetParent(_parentDummy.Dummy);
                }
            }
            else
            {
                _parentDummy?.Dispose();
                _parentDummy = null;
                LayoutTarget.SetParent(null);
            }
        }

        /// <summary>
        /// 以下の条件に従い、_dummyChildrenとLayoutTarget#Parentを設定します。
        ///
        /// - when change child's parent from this
        ///   remove dummy child and from LayoutTarget#Children
        /// - when child has LayoutTargetComponent
        ///   dummy child = null
        ///   child LayoutTarget#Parent = this#LayoutTarget
        /// - when child don't has LayoutTargetComponent
        ///   - when child is RectTransform
        ///     - when dummy child == null
        ///         dummy child = new(child)
        ///         dummy child#Parent = this#LayoutTarget
        ///   - when  child is not RectTransform
        ///     remove dummy child and from LayoutTarget#Children
        ///
        /// LayoutTarget#Childrenの要素はLayoutTarget#Childrenに登録されている子LayoutTargetComponentからでも変更されますので注意してください。
        /// - LayoutTarget#Dispose()
        /// - LayoutTargetComponent#OnTransformParentChanged()
        /// - LayoutTargetComponent#OnDestroy()
        /// </summary>
        void UpdateChildrenLayoutTarget()
        {
            DummyInstanceCounter++;
            var newInstanceCounter = DummyInstanceCounter;
            //_dummyChildrenからの削除は他のループで行います。
            // DummyInfo#InstanceCounterの更新を行います。
            foreach (var child in transform.GetChildEnumerable())
            {
                var childR = child as RectTransform;
                var childLayoutTargetComponent = child.GetComponent<LayoutTargetComponent>();
                if(_dummyChildren.ContainsKey(child))
                {
                    if(childR != null && childLayoutTargetComponent == null)
                    {
                        _dummyChildren[child].InstanceCounter = newInstanceCounter;
                    }
                    else if(childLayoutTargetComponent != null)
                    {
                        childLayoutTargetComponent.LayoutTarget.SetParent(LayoutTarget);
                    }
                }
                else if(childLayoutTargetComponent != null)
                {
                    childLayoutTargetComponent.LayoutTarget.SetParent(LayoutTarget);
                }
                else if(childR != null)
                {
                    var dummyInfo = new DummyInfo(childR) { InstanceCounter = newInstanceCounter };
                    _dummyChildren.Add(child, dummyInfo);
                    dummyInfo.Dummy.SetParent(LayoutTarget);
                }
            }

            // remove children that it's parent not equal this#LayoutTarget.
            foreach (var dummyChild in _dummyChildren.Values
                .Where(_c => _c.InstanceCounter != newInstanceCounter)
                .ToArray())
            {
                _dummyChildren.Remove(dummyChild.Target);
                dummyChild.Dummy.SetParent(null);
                dummyChild.Dispose();
            }
        }
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            AutoDetectUpdater();
        }

        void OnDestroy()
        {
            _onDestroyed.SafeDynamicInvoke(this, () => "Fail in OnDestroy...", LayoutDefines.LOG_SELECTOR);

            _parentDummy = null;
            if(_target != null)
            {
                _target.Dispose();
                _target = null;
            }
            foreach (var child in _dummyChildren.Values)
            {
                child.Dispose();
            }
            _dummyChildren.Clear();
        }

        private void OnTransformParentChanged()
        {
            UpdateParentLayoutTarget();
        }
        #endregion

        public interface ILayoutTargetUpdater
        {
            void CopyToLayoutTarget(LayoutTargetComponent self);
            void CopyToTransform(LayoutTargetComponent self);
        }

        public class RectTransformUpdater : ILayoutTargetUpdater
        {
            public static ILayoutTarget Create(RectTransform R)
            {
                var layout = new LayoutTargetObject();
                CopyToLayoutTarget(R, layout);
                return layout;
            }

            public static void CopyToLayoutTarget(RectTransform R, ILayoutTarget layout)
            {
                layout.Pivot = R.pivot;
                layout.UpdateAnchorParam(R.anchorMin, R.anchorMax, R.offsetMin, R.offsetMax);
                var offset = R.offsetMin.Mul(Vector2.one - R.pivot) + R.offsetMax.Mul(R.pivot);
                layout.UpdateLocalSize(R.rect.size, offset);

                layout.LocalPos = R.anchoredPosition3D - layout.Offset;
            }

            public static void CopyToTransform(RectTransform R, ILayoutTarget layout)
            {
                R.pivot = layout.Pivot;
                R.anchorMin = layout.AnchorMin;
                R.anchorMax = layout.AnchorMax;
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layout.LocalSize.x);
                R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.LocalSize.y);
                R.anchoredPosition3D = layout.LocalPos + layout.Offset;
            }

            DrivenRectTransformTracker _tracker;

            public RectTransformUpdater(LayoutTargetComponent self)
            {
                var R = GetR(self.transform);

                _tracker.Add(self, R, DrivenTransformProperties.AnchoredPosition3D);
                _tracker.Add(self, R, DrivenTransformProperties.AnchorMin);
                _tracker.Add(self, R, DrivenTransformProperties.AnchorMax);
                _tracker.Add(self, R, DrivenTransformProperties.Pivot);
                _tracker.Add(self, R, DrivenTransformProperties.SizeDelta);
            }

            public RectTransform GetR(Transform transform)
                => transform as RectTransform;

            public void CopyToLayoutTarget(LayoutTargetComponent self)
            {
                var R = GetR(self.transform);
                CopyToLayoutTarget(R, self.LayoutTarget);
            }

            public void CopyToTransform(LayoutTargetComponent self)
            {
                _tracker.Clear();

                var R = GetR(self.transform);
                var layout = self.LayoutTarget;
                CopyToTransform(R, layout);

                _tracker.Add(self, R, DrivenTransformProperties.AnchoredPosition3D);
                _tracker.Add(self, R, DrivenTransformProperties.AnchorMin);
                _tracker.Add(self, R, DrivenTransformProperties.AnchorMax);
                _tracker.Add(self, R, DrivenTransformProperties.Pivot);
                _tracker.Add(self, R, DrivenTransformProperties.SizeDelta);
            }

            /// <summary>
            /// LayoutTargetObjectのLocalPos、Offsetと
            /// RectTransformのpositionの相互変換のためオフセットを計算する。
            ///
            /// LayoutTargetObject -> RectTransformへのオフセットを表します。
            /// </summary>
            /// <param name="R"></param>
            /// <returns></returns>
            static Vector2 CalROffset(RectTransform R, ILayoutTarget layout)
            {
                var parentR = R.parent as RectTransform;

                var anchorSize = parentR != null
                    ? parentR.rect.size.Mul(layout.AnchorMax - layout.AnchorMin)
                    : Vector2.zero;
                var ROffset = anchorSize.Mul(R.pivot - Vector2.one * 0.5f);
                return ROffset;
            }
        }
    }
}
