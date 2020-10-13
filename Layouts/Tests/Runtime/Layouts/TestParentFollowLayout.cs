using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// testの覚書　実際のテストとは異なるかもしれませんので、随時更新してください
    /// ## ILayout#OperationPriority
    /// - デフォルトで最大値になっているか?
    ///
    /// ## ParentFollowLayout#OperationTargetFlags
    /// - 想定した値が返されるようになっているか?
    /// 
    /// ## ParentFollowLayout#UpdateLayout
    /// - ILayoutTarget#FollowParentが呼び出されるかどうか?
    /// 
    /// ## ParentFollowLayout#Validate
    /// - ILayoutTarget#Layoutsの中で先頭にある時だけtrueを返すか？
    /// 
    /// <seealso cref="ParentFollowLayout"/>
    /// </summary>
    public class TestParentFollowLayout
    {
        #region ILayout#OperationPriority
        /// <summary>
        /// <seealso cref="ILayout.OperationPriority"/>
        /// </summary>
        [Test]
        public void OperationPriority_DefaultValue_Passes()
        {
            var layout = new ParentFollowLayout();
            Assert.AreEqual(int.MaxValue, layout.OperationPriority);
        }
        #endregion

        #region ILayout#OperationTargetFlags
        /// <summary>
        /// <seealso cref="ParentFollowLayout.OperationTargetFlags"/>
        /// </summary>
        [Test]
        public void OperationTargetFlags_Value_Passes()
        {
            var layout = new ParentFollowLayout();
            Assert.AreEqual(
                LayoutOperationTarget.Self_LocalSize | LayoutOperationTarget.Self_Offset,
                layout.OperationTargetFlags,
                ""
            );
        }
        #endregion

        #region ILayout#Validate
        class DummyLayout : LayoutBase
        {
            public override LayoutOperationTarget OperationTargetFlags { get => 0; }

            public override void UpdateLayout()
            {
            }

            public override bool Validate()
            {
                return true;
            }
        }

        /// <summary>
        /// <seealso cref="ParentFollowLayout.Validate()"/>
        /// </summary>
        [Test]
        public void Validate_Passes()
        {
            {
                var layout = new ParentFollowLayout();
                Assert.IsFalse(layout.Validate());
                Logger.Log(Logger.Priority.High, () => "Success when ILayout#Target is null!!");
            }

            {
                var layout = new ParentFollowLayout();
                layout.Target = new LayoutTargetObject();
                Assert.IsTrue(layout.Validate());
            }
            Logger.Log(Logger.Priority.High, () => "Success to be set ILayout#Target!!");

            {
                var layout = new ParentFollowLayout();
                layout.Target = new LayoutTargetObject();

                var dummyLayout = new DummyLayout();
                dummyLayout.OperationPriority = 10;
                layout.OperationPriority = dummyLayout.OperationPriority - 1;
                layout.Target.AddLayout(dummyLayout);

                Assert.IsFalse(layout.Validate());
            }
            Logger.Log(Logger.Priority.High, () => "Success when Invalid Order in ILayoutTarget#Layouts !!");
        }
        #endregion

        #region ParentFollowLayout#UpdateLayout
        /// <summary>
        /// <seealso cref="ParentFollowLayout.UpdateLayout()"/>
        /// </summary>
        [Test]
        public void UpdateLayout_Passes()
        {
            var parent = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var correct = new LayoutTargetObject();

            parent.UpdateLocalSize(Vector3.one * 100f, Vector3.zero);

            correct.UpdateAnchorParam(Vector3.zero, Vector3.one, Vector3.zero, Vector3.zero);
            correct.IsAutoUpdate = false;
            correct.SetParent(parent);
            correct.FollowParent();

            var (offsetMin, offsetMax) = correct.AnchorOffsetMinMax();
            target.IsAutoUpdate = false;
            target.UpdateAnchorParam(correct.AnchorMin, correct.AnchorMax, offsetMin, offsetMax);
            target.SetParent(parent);

            //test point
            var layout = new ParentFollowLayout();
            layout.Target = target;
            layout.UpdateLayout();

            AssertionUtils.AreNearlyEqual(correct.LocalSize, target.LocalSize, LayoutDefines.POS_NUMBER_PRECISION);
            AssertionUtils.AreNearlyEqual(correct.Offset, target.Offset, LayoutDefines.POS_NUMBER_PRECISION);
        }
        #endregion
    }
}
