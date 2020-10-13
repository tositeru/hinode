using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode.Layouts
{
    /// <summary>
    /// <seealso cref="LayoutManager"/>
    /// </summary>
    [DisallowMultipleComponent()]
    public class LayoutManagerComponent : SingletonMonoBehaviour<LayoutManagerComponent>
    {
        public LayoutManager Manager { get; } = new LayoutManager();

        HashSetHelper<LayoutTargetComponent> _targets = new HashSetHelper<LayoutTargetComponent>();

        public IReadOnlyHashSetHelper<LayoutTargetComponent> Targets { get => _targets; }

        public LayoutManagerComponent Entry(LayoutTargetComponent target)
        {
            if (_targets.Contains(target)) return this;

            _targets.Add(target);
            Manager.Entry(target.LayoutTarget);
            target.OnDestroyed.Add(LayoutTargetComponentOnDestroyed);
            target.LayoutTarget.OnDisposed.Add(LayoutTargetOnDisposed);
            return this;
        }

        public LayoutManagerComponent Exit(LayoutTargetComponent target)
        {
            if(_targets.Contains(target))
            {
                _targets.Remove(target);
                target.OnDestroyed.Remove(LayoutTargetComponentOnDestroyed);
            }

            if(target.LayoutTarget != null)
            {
                Manager.Exit(target.LayoutTarget);
                target.LayoutTarget.OnDisposed.Remove(LayoutTargetOnDisposed);
            }
            return this;
        }

        void LayoutTargetComponentOnDestroyed(LayoutTargetComponent layoutTarget)
        {
            Exit(layoutTarget);
        }

        void LayoutTargetOnDisposed(ILayoutTarget target)
        {
            var com = _targets.FirstOrDefault(_t => _t.LayoutTarget == target);
            if (com != null)
            {
                Exit(com);
            }
        }

        public void CaluculateLayouts()
        {
            foreach(var t in _targets)
            {
                t.UpdateLayoutTargetHierachy();
            }
            Manager.CaluculateLayouts();

            foreach (var t in _targets)
            {
                t.CopyToTransform();
            }
        }

        #region override SingletonMonoBehaviour
        protected override string DefaultInstanceName { get => "__LayoutManager"; }

        protected override void OnAwaked()
        {
        }

        protected override void OnDestroyed(bool isInstance)
        {
            foreach(var t in _targets)
            {
                t.LayoutTarget.OnDisposed.Remove(LayoutTargetOnDisposed);
                t.OnDestroyed.Remove(LayoutTargetComponentOnDestroyed);
            }
        }
        #endregion
    }
}
