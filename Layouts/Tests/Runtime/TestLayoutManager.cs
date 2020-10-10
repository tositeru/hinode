using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using Hinode.Tests;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// testの覚書　実際のテストとは異なるかもしれませんので、随時更新してください
    /// 
    /// ## LayoutManager#Entry
    /// - 新しいILayoutTargetを渡した時
    /// - 既存のGroups#Rootのオブジェクト階層内にあるILayoutTargetを渡したとき
    /// - 既存のGroups#Rootのオブジェクト階層にないILayoutTargetを渡したとき
    /// - 既存のGroups#Rootのオブジェクト階層にないILayoutTargetを渡したとき(Priority指定版)
    /// - ILayoutTarget#LayoutsにParentFollowLayoutが既にある時
    /// - 後処理型のILayoutを持つILayoutTargetを渡した時のGroup#CaluculationOrderの並び順
    /// - 既に登録されているILayoutTargetを渡した時
    /// - 指定したGroupを親Groupに設定して登録させたい時
    /// 
    /// ## LayoutManager#Exit
    /// - 登録されているILayoutTargetを渡した時
    /// - 登録を解除された後、Groupに他のILayoutTargetがまだ残っている時(Normal)
    /// - 登録を解除された時に一緒にGroupが削除される場合に、親Groupが指定されている時
    /// - 登録を解除された時に一緒にGroupが削除される場合に、親Groupが指定されている時
    /// - 登録されていないILayoutTargetを渡した時
    /// 
    /// ## LayoutManager#Group
    /// - Group#Priorityが変更された時
    /// 
    /// ## Caluculate Layouts
    /// - 全てのGroupを計算する
    /// - 単一のGroupのみを計算する
    /// - 更新フラグが立っているもののみを計算する
    /// - Groupsの要素の計算順序のテスト
    ///
    /// ## others
    /// - Disposeされた時
    /// - Disposeされた時 -- 子LayoutObjectの場合
    /// - 登録されているILayoutTargetのオブジェクト階層が変更された時
    ///    - sub1 既存のGroups#Rootのオブジェクト階層から外れた時
    ///    - sub2 同じGroups#Root内で移動した時
    ///    - sub3 他のGroupに移動した時
    /// - test Case 登録されているILayoutTarget#Layoutsが変更された時
    ///    - sub1 -> 追加
    ///    - sub2 -> 削除
    ///    - sub3 -> ParentFollowLayoutが追加された時 -> 一つだけにする
    ///    - sub4 -> ParentFollowLayoutが削除された時 -> 一つだけにする
    /// 
    /// <seealso cref="LayoutManager"/>
    /// </summary>
    public class TestLayoutManager
    {
        const int HIGHEST_ORDER = -100;
        const int ENTRY_TEST_ORDER = 0;
        const int EXIT_TEST_ORDER = 100;
        const int GROUP_TEST_ORDER = 200;
        const int CALUCULATE_LAYOUTS_TEST_ORDER = 200;
        const int LAYOUT_TARGET_ORDER = 200;
        const int ORDER_CHANGED_LAYOUT_IN_LAYOUT_TARGETS = 200;

        class LayoutCallbackCounter : LayoutBase
        {
            public LayoutCallbackCounter()
                : this (LayoutKind.Normal)
            { }

            public LayoutCallbackCounter(LayoutKind kind)
            {
                _kind = kind;
            }

            public void ResetCounter()
            {
                CallUpdateLayoutCounter = 0;
            }
            public int CallUpdateLayoutCounter { get; private set; }

            public void SetDoChanged(bool doChanged) => DoChanged = doChanged;

            LayoutKind _kind = LayoutKind.Normal;
            public LayoutCallbackCounter SetKind(LayoutKind kind)
            {
                _kind = kind;
                return this;
            }

            bool _doAllowDumplicate = true;
            public LayoutCallbackCounter SetDoAllowDumplicate(bool doAllow)
            {
                _doAllowDumplicate = doAllow;
                return this;
            }

            #region override LayoutBase
            public override LayoutKind Kind { get => _kind; }
            public override bool DoAllowDuplicate { get => _doAllowDumplicate; }
            public override LayoutOperationTarget OperationTargetFlags { get => 0; }

            public override bool Validate() => true;

            public override void UpdateLayout()
            {
                CallUpdateLayoutCounter++;
            }
            #endregion
        }

        #region Utility Methods
        void AssertRootsInLayoutManagerGroup(LayoutManager manager, ILayoutTarget[] roots)
        {
            AssertionUtils.AssertEnumerable(
                roots
                , manager.Groups.Select(_g => _g.Root)
                , "Don't Match Root in LayoutManager#Groups... Must sort by Group#Priority..."
            );
        }

        void AssertLayoutManagerGroup(
              LayoutManager.Group group
            , ILayoutTarget root
            , int priority
            , LayoutManager.Group parentGroup
            , LayoutManager.Group[] childGroups
            , ILayoutTarget[] targets)
        {
            Assert.AreSame(root, group.Root, $"Don't match Group#Root...");
            Assert.AreEqual(priority, group.Priority, $"Don't match Group#Priority...");
            Assert.AreSame(parentGroup, group.ParentGroup, $"Don't match Group#ParentGroup...");
            AssertionUtils.AssertEnumerable(
                childGroups != null ? childGroups : new LayoutManager.Group[] { }
                , group.ChildGroups
                , $"Don't match Group#ChildGroups... Must sort caluculation order..."
            );

            AssertionUtils.AssertEnumerable(
                targets != null ? targets : new ILayoutTarget[] { } 
                , group.Targets
                , $"Don't match Group#Targets... Must sort caluculation order..."
            );
            if (targets != null)
            {
                //TODO 遅延計算Layout対応はまだ
                AssertionUtils.AssertEnumerable(
                    targets.SelectMany(_t => _t.Layouts)
                    , group.CaluculationOrder
                    , $"Don't match Group#CaluculationOrder... Must sort caluculation order..."
                );
            }
        }
        #endregion

        #region Entry
        /// <summary>
        /// <seealso cref="LayoutManager.Add(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(HIGHEST_ORDER), Description("新しいILayoutTargetを渡した時")]
        public void Entry_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var layout = new LayoutCallbackCounter();

            target.AddLayout(layout);
            var ON_DISPOSED = "onDisposed";
            var ON_CHANGED_PARENT = "onChangedParent";
            var ON_CHANGED = "onChanged";
            var ON_CHANGED_OP_PRIORITY = "onChangedOpPriority";
            var targetRegistedDelegateCountDict = new Dictionary<string, int>()
            {
                { ON_DISPOSED, target.OnDisposed.RegistedDelegateCount},
                { ON_CHANGED_PARENT, target.OnChangedParent.RegistedDelegateCount },
            };
            var layoutRegistedDelegateCountDict = new Dictionary<string, int>()
            {
                { ON_DISPOSED, layout.OnDisposed.RegistedDelegateCount},
                { ON_CHANGED, layout.OnChanged.RegistedDelegateCount },
                { ON_CHANGED_OP_PRIORITY, layout.OnChangedOperationPriority.RegistedDelegateCount },
            };

            // auto add `ParentFollowLayout` to `target` in LayoutManager!
            var group = manager.Entry(target);// <- test point

            {//Check ILayoutTarget side
                Assert.AreEqual(2, target.Layouts.Count());
                Assert.IsTrue(target.Layouts.Any(_l => _l is ParentFollowLayout), "Not auto add ParentFollowLayout to target...");
                Assert.AreEqual(targetRegistedDelegateCountDict[ON_DISPOSED]+ 2, target.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(targetRegistedDelegateCountDict[ON_CHANGED_PARENT]+1, target.OnChangedParent.RegistedDelegateCount);
            }
            Debug.Log($"Success to Set ILayoutTarget!");

            {//Check ILayout in ILayoutTarget side
                Assert.AreEqual(layoutRegistedDelegateCountDict[ON_DISPOSED], layout.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(layoutRegistedDelegateCountDict[ON_CHANGED], layout.OnChanged.RegistedDelegateCount);
                Assert.AreEqual(layoutRegistedDelegateCountDict[ON_CHANGED_OP_PRIORITY], layout.OnChangedOperationPriority.RegistedDelegateCount);

                //以下のコードはlayoutに自動的に追加されるCallback以外は追加されていないのを想定しています。
                var parentFollowLayout = target.Layouts.Select(_l => _l as ParentFollowLayout).FirstOrDefault();
                Assert.AreEqual(parentFollowLayout.OnDisposed.RegistedDelegateCount, layout.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(parentFollowLayout.OnChanged.RegistedDelegateCount, layout.OnChanged.RegistedDelegateCount);
                Assert.AreEqual(parentFollowLayout.OnChangedOperationPriority.RegistedDelegateCount, layout.OnChangedOperationPriority.RegistedDelegateCount);
            }
            Debug.Log($"Success to Set ILayout!");

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    target
                });
                AssertLayoutManagerGroup(group, target, 0, null, null, new ILayoutTarget[] {
                    target
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("既存のGroups#Rootのオブジェクト階層内にあるILayoutTargetを渡したとき")]
        public void Entry_ToAlreadyExistGroup_Passes()
        {
            var manager = new LayoutManager();
            var parent = new LayoutTargetObject();

            var group = manager.Entry(parent);

            var target = new LayoutTargetObject();
            target.SetParent(parent);

            manager.Entry(target); // <- test point

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    parent
                });
                AssertLayoutManagerGroup(group, parent, 0, null, null, new ILayoutTarget[] {
                    parent, target
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("既存のGroups#Rootのオブジェクト階層にないILayoutTargetを渡したとき")]
        public void Entry_WhenCreateNewGroup_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();
            var otherGroup = manager.Entry(other);

            var target = new LayoutTargetObject();
            var group = manager.Entry(target); // <- test point

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    other, target
                });
                AssertLayoutManagerGroup(otherGroup, other, 0, null, null, new ILayoutTarget[] {
                    other
                });
                AssertLayoutManagerGroup(group, target, 0, null, null, new ILayoutTarget[] {
                    target
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("既存のGroups#Rootのオブジェクト階層にないILayoutTargetを渡したとき(Priorityを指定した版)")]
        public void Entry_WhenCreateNewGroupWithPriority_Passes()
        {
            var manager = new LayoutManager();

            var targets = new (LayoutTargetObject t, int priority)[]
            {
                (new LayoutTargetObject(), -1),
                (new LayoutTargetObject(), 0),
                (new LayoutTargetObject(), 1),
                (new LayoutTargetObject(), 1),
            };

            //test point.
            var groups = new List<LayoutManager.Group>();
            foreach (var (t, priority) in targets)
            {
                var g = manager.Entry(t, priority);
                groups.Add(g);
            }

            {
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    groups[2].Root, // allow index=1 because same priority group[1]!
                    groups[3].Root, // allow index=0 because same priority group[0]!
                    groups[1].Root,
                    groups[0].Root,
                });

                foreach(var (group, targetData) in groups.Zip(targets, (_g, _t) => (_g, _t)))
                {
                    AssertLayoutManagerGroup(group, targetData.t, targetData.priority, null, null, new ILayoutTarget[] {
                        targetData.t
                    });
                }
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("ILayoutTarget#LayoutsにParentFollowLayoutが既にある時")]
        public void Entry_WhenLayoutTargetHasParentFollowLayout_Passes()
        {
            var manager = new LayoutManager();

            var target = new LayoutTargetObject();

            // test point
            target.AddLayout(new ParentFollowLayout());
            manager.Entry(target); // <- not add ParentFollowLayout to tareget!

            {//Check ILayoutTarget side
                Assert.AreEqual(1, target.Layouts.Count());
                Assert.IsTrue(target.Layouts.Any(_l => _l is ParentFollowLayout), "Not auto add ParentFollowLayout to target...");
            }
            Debug.Log($"Success to Set ILayoutTarget!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("後処理型のILayoutを持つILayoutTargetを渡した時のGroup#CaluculationOrderの並び順")]
        public void Entry_WhenLayoutTargetHasPostProcessLayouts_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var layouts = new ILayout[]
            {
                new LayoutCallbackCounter(LayoutKind.Normal),
                new LayoutCallbackCounter(LayoutKind.Delay),
                new LayoutCallbackCounter(LayoutKind.Normal),
                new LayoutCallbackCounter(LayoutKind.Delay),
            };
            target.AddLayout(layouts[0]);
            target.AddLayout(layouts[1]);

            var target2 = new LayoutTargetObject();
            target2.AddLayout(layouts[2]);
            target2.AddLayout(layouts[3]);

            target2.SetParent(target);
            var group = manager.Entry(target);

            AssertionUtils.AssertEnumerable(
                new ILayout[] {
                    //Normal
                    target.Layouts.OfType<ParentFollowLayout>().First(),
                    layouts[0], // in target
                    target2.Layouts.OfType<ParentFollowLayout>().First(),
                    layouts[2], // in target2
                    //Delay
                    layouts[1], // in target
                    layouts[3], // in target2
                }
                , group.CaluculationOrder
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("既に登録されているILayoutTargetを渡した時")]
        public void Entry_ToAlreadyEntryLayoutTarget_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();

            var group = manager.Entry(target);
            manager.Entry(target);// <- test point. LayoutManager do nothing!

            {//Check ILayoutTarget side
                Assert.AreEqual(1, target.Layouts.Count());
                Assert.IsTrue(target.Layouts.Any(_l => _l is ParentFollowLayout), "Not auto add ParentFollowLayout to target...");
            }
            Debug.Log($"Success to Set ILayoutTarget!");

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] { target });
                AssertLayoutManagerGroup(group, target, 0, null, null, new ILayoutTarget[] {
                    target
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// </summary>
        [Test, Order(ENTRY_TEST_ORDER), Description("指定したGroupを親Groupに設定して登録させたい時")]
        public void Entry_ToSpecifyParentGroup_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();

            var target = new LayoutTargetObject();
            var child = new LayoutTargetObject();
            child.SetParent(target);

            var parentGroup = manager.Entry(other);
            var targetGroup = manager.Entry(target, parentGroup);// <- test point.

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    other, target
                });

                {//check other's group
                    var childGroups = new LayoutManager.Group[]
                    {
                        targetGroup
                    };
                    AssertLayoutManagerGroup(parentGroup, other, 0, null, childGroups, new ILayoutTarget[] {
                        other
                    });
                }

                {//check target's group
                    AssertLayoutManagerGroup(targetGroup, target, parentGroup.Priority-1, parentGroup, null, new ILayoutTarget[] {
                        target, child
                    });
                }
            }
            Debug.Log($"Success to LayoutManager!");
        }

        #endregion

        #region Exit
        /// <summary>
        /// <seealso cref="LayoutManager.Exit(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(EXIT_TEST_ORDER), Description("登録されているILayoutTargetを渡した時")]
        public void Exit_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var layout = new LayoutCallbackCounter();

            target.AddLayout(layout);
            var group = manager.Entry(target);

            // check Callbacks
            var ON_DISPOSED = "onDisposed";
            var ON_CHANGED_PARENT = "onChangedParent";
            var ON_CHANGED = "onChanged";
            var ON_CHANGED_OP_PRIORITY = "onChangedOpPriority";
            var targetRegistedDelegateCountDict = new Dictionary<string, int>()
            {
                { ON_DISPOSED, target.OnDisposed.RegistedDelegateCount},
                { ON_CHANGED_PARENT, target.OnChangedParent.RegistedDelegateCount },
            };

            manager.Exit(target);// <- test point

            {//Check ILayoutTarget side
                Assert.AreEqual(2, target.Layouts.Count());
                Assert.IsTrue(target.Layouts.Any(_l => _l is ParentFollowLayout), "Not auto add ParentFollowLayout to target...");
                Assert.AreEqual(targetRegistedDelegateCountDict[ON_DISPOSED] - 1, target.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(targetRegistedDelegateCountDict[ON_CHANGED_PARENT] - 1, target.OnChangedParent.RegistedDelegateCount);
            }
            Debug.Log($"Success to Set ILayoutTarget!");

            {//Check LayoutManager side
                Assert.AreEqual(0, manager.Groups.Count(), $"");
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Exit(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(EXIT_TEST_ORDER), Description("登録を解除された後、Groupに他のILayoutTargetがまだ残っている時(Normal)")]
        public void Exit_RemainOtherLayoutTargetInGroup_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();
            var target = new LayoutTargetObject();

            target.SetParent(other);
            var group = manager.Entry(other);

            manager.Exit(target);// <- test point

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    other
                });

                AssertLayoutManagerGroup(group, other, 0, null, null, new ILayoutTarget[] {
                    other
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Exit(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(EXIT_TEST_ORDER), Description("登録を解除された時に一緒にGroupが削除される場合に、親Groupが指定されている時")]
        public void Exit_AndDeleteGroupSpecifiedParentGroup_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var child = new LayoutTargetObject();
            child.SetParent(target);

            var parentGroup = manager.Entry(other);
            var group = manager.Entry(target, parentGroup); //Groupの親子関係も一緒にテストする

            var cachePriority = parentGroup.Priority; // 削除されるGroupと関係するGroupのpriorityは変更されないようにする
            manager.Exit(target);// <- test point. remove child together!

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    other
                });

                AssertLayoutManagerGroup(parentGroup, other, cachePriority, null, null, new ILayoutTarget[] {
                    other
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Exit(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(EXIT_TEST_ORDER), Description("登録を解除された時に一緒にGroupが削除される場合に、子Groupが指定されている時")]
        public void Exit_AndDeleteGroupSpecifiedChildGroup_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var child = new LayoutTargetObject();
            child.SetParent(target);

            var group = manager.Entry(target);
            var otherGroup = manager.Entry(other, group); //Groupの親子関係も一緒にテストする

            var cachePriority = otherGroup.Priority; // 削除されるGroupと関係するGroupのpriorityは変更されないようにする
            manager.Exit(target);// <- test point. remove child together!

            {//Check LayoutManager side
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    other
                });

                AssertLayoutManagerGroup(otherGroup, other, cachePriority, null, null, new ILayoutTarget[] {
                    other
                });
            }
            Debug.Log($"Success to LayoutManager!");
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Exit(ILayoutTarget)"/>
        /// </summary>
        [Test, Order(EXIT_TEST_ORDER), Description("登録されていないILayoutTargetを渡した時")]
        public void Exit_NotEntryLayoutTarget_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var layout = new LayoutCallbackCounter();

            Assert.DoesNotThrow(() => manager.Exit(target));// <- test point

            {//Check LayoutManager side
                Assert.AreEqual(0, manager.Groups.Count(), $"");
            }
            Debug.Log($"Success to LayoutManager!");
        }

        #endregion

        #region LayoutManager#Group
        /// <summary>
        /// <seealso cref="LayoutManager.Group.Priority"/>
        /// </summary>
        [Test, Order(GROUP_TEST_ORDER), Description("Group#Priorityが変更された時")]
        public void GroupPriority_Changed_Passes()
        {
            var manager = new LayoutManager();
            var other = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var otherPriority = -100;
            var otherGroup = manager.Entry(other, otherPriority);
            var targetGroup = manager.Entry(target);

            //事前チェック
            AssertionUtils.AssertEnumerable(
                new LayoutManager.Group[] {
                    targetGroup,
                    otherGroup,
                }
                , manager.Groups
                , ""
            );

            // test point
            targetGroup.Priority = otherGroup.Priority - 1;

            AssertionUtils.AssertEnumerable(
                new LayoutManager.Group[] {
                    otherGroup,
                    targetGroup,
                }
                , manager.Groups
                , ""
            );
        }
        #endregion

        #region Caluculate Layouts

        /// <summary>
        /// <seealso cref="LayoutManager.CaluculateLayouts()"/>
        /// </summary>
        [Test, Order(CALUCULATE_LAYOUTS_TEST_ORDER), Description("全てのGroupを計算する")]
        public void CaluculateLayouts_Passes()
        {
            var manager = new LayoutManager();

            var targets = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            foreach(var t in targets)
            {
                t.AddLayout(new LayoutCallbackCounter());
                manager.Entry(t);
            }

            // test point
            manager.CaluculateLayouts();

            foreach(var t in targets)
            {
                var callbackCounter = t.Layouts.First(_l => _l is LayoutCallbackCounter) as LayoutCallbackCounter;

                Assert.AreEqual(1, callbackCounter.CallUpdateLayoutCounter);

                callbackCounter.ResetCounter();
            }
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Group.CaluculateLayouts()"/>
        /// </summary>
        [Test, Order(CALUCULATE_LAYOUTS_TEST_ORDER-1), Description("単一のGroupのみを計算する")]
        public void CaluculateLayouts_SingleGroup_Passes()
        {
            var manager = new LayoutManager();

            var targets = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            foreach (var t in targets)
            {
                t.AddLayout(new LayoutCallbackCounter());
                manager.Entry(t);
            }

            // test point
            var group = manager.Groups.First();
            group.CaluculateLayouts();

            foreach (var t in group.Targets)
            {
                var callbackCounter = t.Layouts.First(_l => _l is LayoutCallbackCounter) as LayoutCallbackCounter;

                Assert.AreEqual(1, callbackCounter.CallUpdateLayoutCounter);

                callbackCounter.ResetCounter();
            }
        }

        /// <summary>
        /// <seealso cref="LayoutManager.Group.CaluculateLayouts()"/>
        /// </summary>
        [Test, Order(CALUCULATE_LAYOUTS_TEST_ORDER - 1), Description("更新フラグが立っているもののみを計算する")]
        public void CaluculateLayouts_OnlyDoChangedFlagIsTrue_Passes()
        {
            var manager = new LayoutManager();

            var targets = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            foreach (var t in targets)
            {
                t.AddLayout(new LayoutCallbackCounter());

                var disableLayout = new LayoutCallbackCounter();
                disableLayout.SetDoChanged(false);
                t.AddLayout(disableLayout);

                manager.Entry(t);
            }

            // test point
            manager.CaluculateLayouts();

            foreach (var callbackCounter in manager.Groups
                .SelectMany(_g => _g.Targets.SelectMany(_t => _t.Layouts.Where(_l => _l is LayoutCallbackCounter)))
                .OfType<LayoutCallbackCounter>())
            {
                var correctCount = callbackCounter.DoChanged ? 1 : 0;
                Assert.AreEqual(correctCount, callbackCounter.CallUpdateLayoutCounter);

                callbackCounter.ResetCounter();
            }
        }

        //
        class LayoutCallbackCounterEx : LayoutCallbackCounter
        {
            public static int CallUpdateLayoutSum { get; set; }

            public int CallUpdateLayoutCounter { get; set; }
            #region override LayoutBase

            public override void UpdateLayout()
            {
                base.UpdateLayout();
                CallUpdateLayoutCounter++;
            }
            #endregion
        }

        /// <summary>
        /// <seealso cref="LayoutManager.CaluculateLayouts()"/>
        /// </summary>
        [Test, Order(CALUCULATE_LAYOUTS_TEST_ORDER), Description("Groupsの要素の計算順序のテスト")]
        public void CaluculateLayouts_CaluculateByPriorityOrder_Passes_Passes()
        {
            var manager = new LayoutManager();

            var targets = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            foreach (var t in targets)
            {
                t.AddLayout(new LayoutCallbackCounter());
                manager.Entry(t);
            }

            // test point
            LayoutCallbackCounterEx.CallUpdateLayoutSum = 0;
            manager.CaluculateLayouts();


            //一つ前のGroup#Priorityの方が値が小さくなるようにすること
            var result = manager.Groups
                .Select(_g => (priority: _g.Priority, valid: true))
                .Aggregate((_p, _c) => (_c.priority, _p.valid && (_p.priority <= _c.priority)));
            Assert.IsTrue(result.valid);
        }
        #endregion

        #region ILayoutTarget

        [Test, Order(LAYOUT_TARGET_ORDER), Description("Disposeされた時(Group#Rootの時)")]
        public void LayoutTarget_Dispose_AtRoot_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var child = new LayoutTargetObject();

            child.SetParent(target);
            manager.Entry(target);

            // test point
            target.Dispose();

            Assert.AreEqual(0, manager.Groups.Count);
        }

        [Test, Order(LAYOUT_TARGET_ORDER), Description("Disposeされた時 -- 子LayoutObjectの場合")]
        public void LayoutTarget_Dispose_AtChild_Passes()
        {
            var manager = new LayoutManager();
            var target = new LayoutTargetObject();
            var child = new LayoutTargetObject();

            child.SetParent(target);
            var group = manager.Entry(target);

            // test point
            child.Dispose();

            AssertionUtils.AssertEnumerable(
                new LayoutManager.Group[]
                {
                    group
                }
                , manager.Groups
                , ""
            );
            AssertLayoutManagerGroup(group, target, 0, null, null, new ILayoutTarget[]
            {
                target
            });
        }

        [Test, Order(LAYOUT_TARGET_ORDER), Description("登録されているILayoutTargetのオブジェクト階層が変更された時 -- 既存のGroups#Rootのオブジェクト階層から外れた時")]
        public void LayoutTarget_ChangeHierachy_ExitFromRoot_Passes()
        {
            var manager = new LayoutManager();
            var parent = new LayoutTargetObject();
            var target = new LayoutTargetObject();

            target.SetParent(parent);
            int priority = 10; // Keep priority new group!
            var group = manager.Entry(parent, priority);

            // test point
            target.SetParent(null);

            {
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    parent, target
                });

                AssertLayoutManagerGroup(group, parent, priority, null, null, new ILayoutTarget[] {
                    parent
                });

                var targetGroup = manager.Groups.First(_g => _g.Root == target);
                // priority copy from original group!
                AssertLayoutManagerGroup(targetGroup, target, priority, null, null, new ILayoutTarget[] {
                    target
                });
            }
        }

        [Test, Order(LAYOUT_TARGET_ORDER), Description("登録されているILayoutTargetのオブジェクト階層が変更された時 -- 同じGroups#Root内で移動した時")]
        public void LayoutTarget_ChangeHierachy_MoveInSameGroup_Passes()
        {
            var manager = new LayoutManager();
            var parent = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var other = new LayoutTargetObject();

            target.SetParent(parent);
            other.SetParent(parent);
            var group = manager.Entry(parent);

            // test point
            target.SetParent(other);

            {
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    parent
                });

                AssertLayoutManagerGroup(group, parent, 0, null, null, new ILayoutTarget[] {
                    parent, other, target
                });
            }
        }

        [Test, Order(LAYOUT_TARGET_ORDER), Description("登録されているILayoutTargetのオブジェクト階層が変更された時 -- 他のGroupに移動した時")]
        public void LayoutTarget_ChangeHierachy_MoveToOtherGroup_Passes()
        {
            var manager = new LayoutManager();
            var parent = new LayoutTargetObject();
            var target = new LayoutTargetObject();
            var other = new LayoutTargetObject();

            target.SetParent(parent);
            var group = manager.Entry(parent);
            var otherGroup = manager.Entry(other);

            // test point
            target.SetParent(other);

            AssertionUtils.AssertEnumerable(
                new LayoutManager.Group[]
                {
                    group, otherGroup
                }
                , manager.Groups
                , ""
            );

            {
                AssertRootsInLayoutManagerGroup(manager, new ILayoutTarget[] {
                    parent, other
                });

                AssertLayoutManagerGroup(group, parent, group.Priority, null, null, new ILayoutTarget[] {
                    parent,
                });
                AssertLayoutManagerGroup(otherGroup, other, otherGroup.Priority, null, null, new ILayoutTarget[] {
                    other, target
                });
            }
        }

        /// <summary>
        /// Callbackが正常に動作しているかどうかの確認がメインになります
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, LayoutManager.Group)"/>
        /// </summary>
        [Test, Order(ORDER_CHANGED_LAYOUT_IN_LAYOUT_TARGETS), Description("登録されているILayoutTarget#Layoutsが変更された時 -> 追加/削除")]
        public void LayoutTarget_ChagendLayouts_Passes()
        {
            var manager = new LayoutManager();

            var target = new LayoutTargetObject();
            var group = manager.Entry(target);

            var defaultLayout = target.Layouts.ToArray();

            var addedLayout = new LayoutCallbackCounter();
            {//test point (Add Layout)
                target.AddLayout(addedLayout);

                AssertionUtils.AssertEnumerableByUnordered(
                    defaultLayout
                        .Concat(new ILayout[] { addedLayout })
                    , group.CaluculationOrder
                    , ""
                );
                AssertLayoutManagerGroup(group, target, group.Priority, null, null, new ILayoutTarget[] { target });
            }

            {//test point (Remove Layout)
                target.RemoveLayout(addedLayout);

                AssertionUtils.AssertEnumerableByUnordered(
                    defaultLayout
                    , group.CaluculationOrder
                    , ""
                );
                AssertLayoutManagerGroup(group, target, group.Priority, null, null, new ILayoutTarget[] { target });
            }
        }

        /// <summary>
        /// Callbackが正常に動作しているかどうかの確認がメインになります
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, int)"/>
        /// <seealso cref="LayoutManager.Entry(ILayoutTarget, LayoutManager.Group)"/>
        /// </summary>
        [Test, Order(ORDER_CHANGED_LAYOUT_IN_LAYOUT_TARGETS), Description("登録されているILayoutTarget#Layoutsが変更された時 -> ParentFollowLayoutが追加/削除された時")]
        public void LayoutTarget_ChagendParentFollowLayout_Passes()
        {
            var manager = new LayoutManager();

            var target = new LayoutTargetObject();
            var group = manager.Entry(target);

            var defaultLayout = target.Layouts.ToArray();

            var addParentFollowLayout = new ParentFollowLayout();
            {//test point (Add Layout)
                target.AddLayout(addParentFollowLayout);

                AssertionUtils.AssertEnumerableByUnordered(
                    defaultLayout
                    , group.CaluculationOrder
                    , "ParentFollowLayoutが追加された時は元々あったものだけにしてください。"
                );
                AssertLayoutManagerGroup(group, target, group.Priority, null, null, new ILayoutTarget[] { target });
            }

            {//test point (Remove Layout)
                target.RemoveLayout(addParentFollowLayout);

                AssertionUtils.AssertEnumerableByUnordered(
                    defaultLayout
                    , group.CaluculationOrder
                    , "ParentFollowLayoutが削除された時は元に戻すか再生成してください。"
                );
                AssertLayoutManagerGroup(group, target, group.Priority, null, null, new ILayoutTarget[] { target });
            }
        }
        #endregion

    }
}
