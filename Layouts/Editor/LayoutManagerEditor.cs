using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Hinode.Layouts.Editors
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomEditor(typeof(LayoutManagerComponent))]
    public class LayoutManagerEditor : Editor
    {
        public static void UpdateAllLayouts()
        {

        }

        /// <summary>
        /// 指定したLayoutTargetComponentに関係するLayoutTargetを全て更新する。
        ///
        /// 更新対象は以下のものになります。
        /// - targetが所属しているGameObject階層の中でLayoutTargetComponentがアタッチされているGameObject階層以下の全LayoutTargetComponent
        /// </summary>
        /// <param name="target"></param>
        public static void UpdateLayoutHierachy(LayoutTargetComponent target)
        {
            var root = target.transform.GetParentEnumerable()
                .Select(_p => _p.GetComponent<LayoutTargetComponent>())
                .LastOrDefault(_p => _p != null);
            if (root == null)
                root = target;

            var layoutTargets = root.transform.GetHierarchyEnumerable()
                .Select(_t => _t.GetComponent<LayoutTargetComponent>())
                .Where(_t => _t != null);
            foreach (var t in layoutTargets)
            {
                t.LayoutTarget.IsAutoUpdate = false;
            }

            foreach(var t in layoutTargets)
            {
                LayoutTargetComponentEditor.UpdateSelf(t);
            }
        }
    }
}
