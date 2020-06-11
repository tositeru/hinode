using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hinode.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.MVC.Tests.LayoutOverwriter
{
    /// <summary>
    /// <seealso cref="ViewLayoutOverwriter"/>
    /// </summary>
    public class TestViewLayoutOverwriter
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            var query = ".style1";
            var layoutValueDict = new ViewLayoutValueDictionary()
                    .AddValue("key", 100)
                    .AddValue("key2", 200);
            var layoutValueDict2 = new ViewLayoutValueDictionary()
                    .AddValue("apple", -100)
                    .AddValue("orange", -200);

            var layoutOverwriter = new ViewLayoutOverwriter();
            layoutOverwriter
                .Add(new ViewLayoutSelector(query, ""), layoutValueDict)
                .Add(new ViewLayoutSelector($"Model {query}", ""), layoutValueDict2);

            var model = new Model() { Name = "Model" }
                .AddStylingID(query);

            AssertionUtils.AssertEnumerable(
                new ViewLayoutValueDictionary[] {
                    layoutValueDict2,
                    layoutValueDict,
                }
                , layoutOverwriter.MatchLayoutValueDicts(model, null)
                , "");
        }

        [Test]
        public void BasicUsageWithViewObjPasses()
        {
            var query = ".style1";
            var viewID = "view1";
            var viewID2 = "view2";
            var layoutValueDict = new ViewLayoutValueDictionary()
                    .AddValue("key", 100)
                    .AddValue("key2", 200);
            var layoutValueDict2 = new ViewLayoutValueDictionary()
                    .AddValue("apple", -100)
                    .AddValue("orange", -200);

            var layoutValueDict3 = new ViewLayoutValueDictionary()
                .AddValue("grape", -100);

            var layoutOverwriter = new ViewLayoutOverwriter();
            layoutOverwriter
                .Add(new ViewLayoutSelector(query, viewID), layoutValueDict)
                .Add(new ViewLayoutSelector($"Model {query}", viewID), layoutValueDict2)
                .Add(new ViewLayoutSelector($"Model {query}", viewID2), layoutValueDict3);

            var model = new Model() { Name = "Model" }
                .AddStylingID(query);
            var viewObj = new EmptyViewObject();
            viewObj.Bind(model, new ModelViewBinder.BindInfo(viewID, typeof(EmptyViewObject)), null);

            AssertionUtils.AssertEnumerable(
                new ViewLayoutValueDictionary[] {
                    layoutValueDict2,
                    layoutValueDict,
                }
                , layoutOverwriter.MatchLayoutValueDicts(model, viewObj)
                , "");
        }

        interface ITestViewLayout : IViewLayout
        {
            int Value { get; set; }
        }
        class TestViewLayoutAccessor : IViewLayoutAccessor
        {
            public override Type ViewLayoutType { get => typeof(ITestViewLayout); }
            public override Type ValueType { get => typeof(int); }
            public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

            protected override object GetImpl(object viewLayoutObj)
                => (viewLayoutObj as ITestViewLayout).Value;

            protected override void SetImpl(object value, object viewLayoutObj)
            {
                (viewLayoutObj as ITestViewLayout).Value = (int)value;
            }
        }

        class TestViewObject : EmptyViewObject
            , ITestViewLayout
            , IDepthViewLayout
        {
            public int Value { get; set; }
            public float DepthLayout { get; set; }
        }

        [Test]
        public void BinderInstanceApplyViewLayoutPasses()
        {
            var styleID = ".styleID";
            var testViewLayoutName = "test";
            var viewID = "viewID";
            var viewID2 = "viewID2";
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestViewObject), new EmptyModelViewParamBinder())
            );
            var binder = new ModelViewBinder("*", viewInstanceCreator,
                new ModelViewBinder.BindInfo(viewID, typeof(TestViewObject))
                    .AddViewLayoutValue(BasicViewLayoutName.depth, 100)
                    .AddViewLayoutValue(testViewLayoutName, 222),
                new ModelViewBinder.BindInfo(viewID2, typeof(TestViewObject))
                    .AddViewLayoutValue(BasicViewLayoutName.depth, -1)
                    .AddViewLayoutValue(testViewLayoutName, 1)
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator
                , binder
            )
            {
                UseViewLayouter = new ViewLayouter()
                    .AddBasicViewLayouter()
                    .AddKeywords((testViewLayoutName, new TestViewLayoutAccessor())),
                UseViewLayoutOverwriter = new ViewLayoutOverwriter()
                    .Add(new ViewLayoutSelector("*", viewID)
                        , new ViewLayoutValueDictionary()
                            .AddValue(testViewLayoutName, -222)
                    )
                    .Add(new ViewLayoutSelector("*", viewID2)
                        , new ViewLayoutValueDictionary()
                            .AddValue(BasicViewLayoutName.depth, -234f)
                    )
            };
            var bindInstanceMap = binderMap.CreateBinderInstaceMap();
            bindInstanceMap.RootModel =
                new Model() { Name = "Model" }
                    .AddStylingID(styleID);

            bindInstanceMap.ApplyViewLayouts(ViewLayoutAccessorUpdateTiming.All);

            {
                var bindInstance = bindInstanceMap[bindInstanceMap.RootModel];
                var viewObj = bindInstance.QueryViews(viewID).OfType<TestViewObject>().First();
                Assert.AreEqual(viewObj.UseBindInfo.GetViewLayoutValue(BasicViewLayoutName.depth), viewObj.DepthLayout);
                var dicts = bindInstanceMap.UseViewLayoutOverwriter.MatchLayoutValueDicts(viewObj.UseModel, viewObj).First();
                Assert.AreEqual(dicts.GetValue(testViewLayoutName), viewObj.Value);
            }
            Debug.Log($"Success to viewObj({viewID})!");
            {
                var bindInstance = bindInstanceMap[bindInstanceMap.RootModel];
                var viewObj = bindInstance.QueryViews(viewID2).OfType<TestViewObject>().First();
                var dicts = bindInstanceMap.UseViewLayoutOverwriter.MatchLayoutValueDicts(viewObj.UseModel, viewObj).First();
                Assert.AreEqual(dicts.GetValue(BasicViewLayoutName.depth), viewObj.DepthLayout);
                Assert.AreEqual(viewObj.UseBindInfo.GetViewLayoutValue(testViewLayoutName), viewObj.Value);
            }
            Debug.Log($"Success to viewObj({viewID2})!");
        }

    }
}
