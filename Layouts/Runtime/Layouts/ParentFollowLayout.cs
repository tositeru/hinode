using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// ILayoutTarget#FollowParentを呼び出すためのILayout
    ///
    /// このLayoutはILayoutTarget#Layoutsの中で必ず先頭に配置されることを期待しています。
    /// <seealso cref="ILayoutTarget"/>
    /// </summary>
    public class ParentFollowLayout : LayoutBase
    {
        public ParentFollowLayout()
        {
            OperationPriority = int.MaxValue; //
        }

        #region overide LayoutBase
        public override LayoutOperationTarget OperationTargetFlags
        {
            get => LayoutOperationTarget.Self_LocalSize | LayoutOperationTarget.Self_Offset;
        }

        public override void UpdateLayout()
        {
            Target.FollowParent();
        }

        public override bool Validate()
        {
            return Target != null && this == Target.Layouts.FirstOrDefault();
        }
        #endregion
    }
}
