﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.ViewLayout
{
    /// <summary>
	/// <seealso cref="ViewLayouter"/>
	/// </summary>
    public class TestViewLayouter
    {
        interface ITestColorViewLayout : IViewLayout
        {
            Color ColorLayout { get; set; }
        }

        class TestColorViewLayoutAccessor : IViewLayoutAccessor
        {
            public override System.Type ViewLayoutType { get => typeof(ITestColorViewLayout); }
            public override System.Type ValueType { get => typeof(Color); }
            public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

            protected override object GetImpl(object viewLayoutObj)
            {
                return (viewLayoutObj as ITestColorViewLayout).ColorLayout;
            }

            protected override void SetImpl(object value, object viewLayoutObj)
            {
                (viewLayoutObj as ITestColorViewLayout).ColorLayout = (Color)value;
            }
        }

        interface ITestShapeViewLayout : IViewLayout
        {
            int ShapeNoLayout { get; set; }
        }

        class TestShapeViewLayoutAccessor : IViewLayoutAccessor
        {
            public override System.Type ViewLayoutType { get => typeof(ITestShapeViewLayout); }
            public override System.Type ValueType { get => typeof(int); }
            public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

            protected override object GetImpl(object viewLayoutObj)
            {
                return (viewLayoutObj as ITestShapeViewLayout).ShapeNoLayout;
            }

            protected override void SetImpl(object value, object viewLayoutObj)
            {
                (viewLayoutObj as ITestShapeViewLayout).ShapeNoLayout = (int)value;
            }
        }

        interface ITestAlwaysUpdateViewLayout : IViewLayout
        {
            int AlwaysUpdateLayout { get; set; }
        }

        class TestAlwaysUpdateViewLayoutAccessor : IViewLayoutAccessor
        {
            public override System.Type ViewLayoutType { get => typeof(ITestAlwaysUpdateViewLayout); }
            public override System.Type ValueType { get => typeof(int); }
            public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.Always; }

            protected override object GetImpl(object viewLayoutObj)
            {
                return (viewLayoutObj as ITestAlwaysUpdateViewLayout).AlwaysUpdateLayout;
            }

            protected override void SetImpl(object value, object viewLayoutObj)
            {
                (viewLayoutObj as ITestAlwaysUpdateViewLayout).AlwaysUpdateLayout = (int)value;
            }
        }

        class TestAutoViewObj : EmptyAutoViewLayoutObject, ITestColorViewLayout
        {
            public Color ColorLayout { get; set; }
        }

        class AutoTestColorViewLayoutCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
            {
                return new TestAutoViewObj();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return new[] { typeof(ITestColorViewLayout) };
            }
        }

        class TestViewObj : EmptyViewObject
            , ITestColorViewLayout
        {
            public Color ColorLayout { get; set; } = Color.white;

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class TestView2Obj : EmptyViewObject
        {
            public Color ColorLayout { get; set; } = Color.white;

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            //viewLayouter:
            //  - color: TestColorViewLayoutSetter
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(("color", colorLayouterAccessor));

            Assert.IsTrue(viewLayouter.ContainsKeyword("color"));
            Assert.IsFalse(viewLayouter.ContainsKeyword("__invalid"));
            Assert.IsTrue(viewLayouter.IsVaildValue("color", Color.black));
            Assert.IsFalse(viewLayouter.IsVaildValue("color", 100));
            Assert.IsFalse(viewLayouter.IsVaildValue("__invalid", Color.black));

            var testViewObj = new TestViewObj();
            Assert.IsTrue(viewLayouter.IsVaildViewObject("color", testViewObj));
            viewLayouter.Set("color", Color.blue, testViewObj);
            Assert.AreEqual(Color.blue, testViewObj.ColorLayout);

            var testView2Obj = new TestView2Obj();
            Assert.IsFalse(viewLayouter.IsVaildViewObject("color", testView2Obj));
            Assert.Throws<System.ArgumentException>(() =>
            {
                viewLayouter.Set("color", Color.blue, testView2Obj);
            });
        }

        [Test]
        public void AddKeywordsPasses()
        {
            var viewLayouter = new ViewLayouter();

            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var shapeLayoutAccessor = new TestShapeViewLayoutAccessor();
            {
                viewLayouter.AddKeywords(
                    ("color", colorLayouterAccessor),
                    ("shape", shapeLayoutAccessor));

                Assert.IsTrue(viewLayouter.ContainsKeyword("color"));
                Assert.IsTrue(viewLayouter.ContainsKeyword("shape"));
            }
            {
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => viewLayouter.AddKeywords(("color", shapeLayoutAccessor)), "");
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => viewLayouter.AddKeywords(("shape2", shapeLayoutAccessor)), "");
            }
        }

        [Test]
        public void SetAllMatchLayoutsPasses()
        {
            //viewLayouter:
            //  - color: TestColorViewLayoutSetter
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(("color", colorLayouterAccessor));

            var setColor = Color.black;
            var keyAndValues = new Dictionary<string, object>() {
                {"color", setColor }
            };
            var testViewObj = new TestViewObj();
            viewLayouter.SetAllMatchLayouts(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues);
            Assert.AreEqual(setColor, testViewObj.ColorLayout);

            var testView2Obj = new TestView2Obj();
            viewLayouter.SetAllMatchLayouts(ViewLayoutAccessorUpdateTiming.All, testView2Obj, keyAndValues);
            Assert.AreNotEqual(setColor, testView2Obj.ColorLayout);
        }

        [Test, Description("")]
        public void AutoCreateViewObjPasses()
        {
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(("color", colorLayouterAccessor));
            Assert.IsFalse(viewLayouter.DoEnableToAutoCreateViewObject);
            Assert.IsFalse(viewLayouter.ContainAutoViewObjectCreator("color"));

            var autoCreator = new AutoTestColorViewLayoutCreator();
            {//Test Add
                viewLayouter.AddAutoCreateViewObject(autoCreator, "color");
                Assert.IsTrue(viewLayouter.DoEnableToAutoCreateViewObject);
                Assert.IsTrue(viewLayouter.ContainAutoViewObjectCreator("color"));
            }

            {//Test ViewObjet have to match layout keyword.
                var viewObj = new TestViewObj();
                AssertionUtils.AssertEnumerableByUnordered(new ViewLayouter.IAutoViewObjectCreator[] {
                }, viewLayouter.GetAutoViewObjectCreator(viewObj, "color"), "");
            }
            {//Test ViewObjet not to have match layout keyword.
                var viewObj2 = new TestView2Obj();
                AssertionUtils.AssertEnumerableByUnordered(new ViewLayouter.IAutoViewObjectCreator[] {
                    autoCreator,
                }, viewLayouter.GetAutoViewObjectCreator(viewObj2, "color"), "");
            }
        }

        [Test]
        public void AutoSettingPasses()
        {
            //*:
            //  - TestViewObj:
            //    layout(color=(0, 0, 1, 1))
            //  - TestView2Obj:
            //    layout(color=(0, 1, 0, 1))
            var useBindInfo = new ModelViewBinder.BindInfo(typeof(TestViewObj))
                .AddViewLayoutValue("color", Color.blue);
            var useBindInfo2 = new ModelViewBinder.BindInfo(typeof(TestView2Obj))//<- Not include IViewLayout
                .AddViewLayoutValue("color", Color.green);
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestViewObj), new TestViewObj.ParamBinder()),
                (typeof(TestView2Obj), new TestView2Obj.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null, useBindInfo, useBindInfo2);
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var model = new Model() { Name = "model" };
            #endregion

            var viewLayouter = new ViewLayouter(("color", new TestColorViewLayoutAccessor()));
            binderMap.UseViewLayouter = viewLayouter;
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            Assert.AreSame(binderMap.UseViewLayouter, binderInstanceMap.UseViewLayouter);

            Assert.DoesNotThrow(() =>
            {
                binderInstanceMap.RootModel = model;
            });

            var binderInstance = binderInstanceMap.BindInstances[model];
            {//Check TestViewObj
                var viewObj = binderInstance.ViewObjects.OfType<TestViewObj>().First();
                Assert.AreEqual(useBindInfo.GetViewLayoutValue("color"), viewObj.ColorLayout);
            }
            {//Check TestView2Obj
                var view2Obj = binderInstance.ViewObjects.OfType<TestView2Obj>().First();
                Assert.AreNotEqual(useBindInfo2.GetViewLayoutValue("color"), view2Obj.ColorLayout);
            }
        }

        [Test]
        public void AutoSettingAndAutoCreateViewObjPasses()
        {
            //*:
            //  - TestViewObj:
            //    layout(color=(0, 0, 1, 1))
            //  - TestView2Obj:
            //    layout(color=(0, 1, 0, 1))
            var useBindInfo = new ModelViewBinder.BindInfo(typeof(TestViewObj));
            useBindInfo.AddViewLayoutValue("color", Color.blue);
            var useBindInfo2 = new ModelViewBinder.BindInfo(typeof(TestView2Obj))//<- Not include IViewLayout
                    .AddViewLayoutValue("color", Color.green);
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestViewObj), new TestViewObj.ParamBinder()),
                (typeof(TestView2Obj), new TestView2Obj.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null, useBindInfo, useBindInfo2);
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var model = new Model() { Name = "model" };
            #endregion

            var viewLayouter = new ViewLayouter(("color", new TestColorViewLayoutAccessor()));
            viewLayouter.AddAutoCreateViewObject(new AutoTestColorViewLayoutCreator(), "color");
            binderMap.UseViewLayouter = viewLayouter;
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            Assert.AreSame(binderMap.UseViewLayouter, binderInstanceMap.UseViewLayouter);

            Assert.DoesNotThrow(() =>
            {
                binderInstanceMap.RootModel = model;
            });

            var binderInstance = binderInstanceMap.BindInstances[model];
            {//Check TestViewObj
                var viewObj = binderInstance.ViewObjects.OfType<TestViewObj>().First();
                Assert.IsFalse(binderInstance.AutoLayoutViewObjects.ContainsKey(viewObj));
            }
            {//Check TestView2Obj
                var view2Obj = binderInstance.ViewObjects.OfType<TestView2Obj>().First();

                Assert.IsTrue(binderInstance.AutoLayoutViewObjects.ContainsKey(view2Obj));
                var autoViewObjs = binderInstance.AutoLayoutViewObjects[view2Obj];
                Assert.AreEqual(1, autoViewObjs.Count());
                Assert.IsTrue(autoViewObjs.Any(_v => _v is TestAutoViewObj));
                var autoViewObj = autoViewObjs.First(_v => _v is TestAutoViewObj);
                Assert.AreSame(view2Obj, autoViewObj.Target);
            }
        }

        class NotSupportViewLayoutCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
            {
                return new ViewLayoutObj();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return new[] { typeof(ITestColorViewLayout) };
            }

            public class ViewLayoutObj : EmptyAutoViewLayoutObject
            {
            }
        }

        [Test, Description("ViewLayouter.IAutoViewObjectCreatorが対応していないIViewObjectを生成した時のテスト")]
        public void CreateNotSupportedAutoViewObjectFail()
        {
            //*:
            //  - TestViewObj:
            //    layout(color=(0, 0, 1, 1))
            //  - TestView2Obj:
            //    layout(color=(0, 1, 0, 1))
            var useBindInfo = new ModelViewBinder.BindInfo(typeof(TestView2Obj))//<- Not include IViewLayout
                    .AddViewLayoutValue("color", Color.green);
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestViewObj), new TestViewObj.ParamBinder()),
                (typeof(TestView2Obj), new TestView2Obj.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null, useBindInfo);
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var model = new Model() { Name = "model" };
            #endregion

            var viewLayouter = new ViewLayouter(("color", new TestColorViewLayoutAccessor()));

            viewLayouter.AddAutoCreateViewObject(new NotSupportViewLayoutCreator(), "color");
            binderMap.UseViewLayouter = viewLayouter;
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            Assert.AreSame(binderMap.UseViewLayouter, binderInstanceMap.UseViewLayouter);

            Assert.Throws<System.Exception>(() =>
            {
                binderInstanceMap.RootModel = model;
            });

            //ViewLayouter#IAutoViewObjectCreator#Createの確認
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var viewObj = new TestView2Obj()
                {
                    UseModel = model,
                    UseBindInfo = useBindInfo,
                    UseBinderInstance = null
                };
                foreach (var creator in viewLayouter.GetAutoViewObjectCreator(viewObj, useBindInfo.ViewLayoutValues.Layouts.Keys))
                {
                    creator.Create(viewObj); // <- Here may be throw Exception...
                }
            });
        }

        class AllUpdateTimingViewObj : EmptyViewObject
            , ITestColorViewLayout
            , ITestAlwaysUpdateViewLayout
        {
            public Color ColorLayout { get; set; } = Color.white;
            public int AlwaysUpdateLayout { get; set; } = 0;

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class OnlyAlwaysUpdateLayoutViewObj : EmptyViewObject
            , ITestAlwaysUpdateViewLayout
        {
            public int AlwaysUpdateLayout { get; set; } = 0;

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        [Test, Description("IViewObjectが一致しているかどうかのテスト")]
        public void DoIViewObjectMatchAnyLayoutPasses()
        {
            var colorLayoutName = "color";
            var alywaysUpdateLayoutName = "always";
            //viewLayouter:
            //  - color: TestColorViewLayoutSetter
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var alwaysUpdateLayoutAccessor = new TestAlwaysUpdateViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(
                (colorLayoutName, colorLayouterAccessor),
                (alywaysUpdateLayoutName, alwaysUpdateLayoutAccessor));

            {
                var testViewObj = new AllUpdateTimingViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(class AllUpdateTimingViewObj)!");

            {
                var testViewObj = new TestViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsFalse(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(class TestViewObj)!");

            {
                var testViewObj = new OnlyAlwaysUpdateLayoutViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsFalse(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(class OnlyAlwaysUpdateLayoutViewObj)!");
        }

        class AllUpdateTimingAutoViewLayoutObj : EmptyAutoViewLayoutObject
            , ITestColorViewLayout
            , ITestAlwaysUpdateViewLayout
        {
            public Color ColorLayout { get; set; } = Color.white;
            public int AlwaysUpdateLayout { get; set; } = 0;

            #region IAutoViewLayoutObject interface
            #endregion
        }

        class OnlyAlwaysUpdateAutoViewLayoutObj : EmptyAutoViewLayoutObject
            , ITestAlwaysUpdateViewLayout
        {
            public int AlwaysUpdateLayout { get; set; } = 0;
            #region IAutoViewLayoutObject interface
            #endregion
        }

        [Test, Description("IAutoViewLayoutObjectが一致しているかどうかのテスト")]
        public void DoIAutoViewLayoutObjectMatchAnyLayoutPasses()
        {
            var colorLayoutName = "color";
            var alywaysUpdateLayoutName = "always";
            //viewLayouter:
            //  - color: TestColorViewLayoutSetter
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var alwaysUpdateLayoutAccessor = new TestAlwaysUpdateViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(
                (colorLayoutName, colorLayouterAccessor),
                (alywaysUpdateLayoutName, alwaysUpdateLayoutAccessor));

            {
                var testViewLayoutObj = new AllUpdateTimingAutoViewLayoutObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewLayoutObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewLayoutObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewLayoutObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(class AllUpdateTimingAutoViewLayoutObj)!");

            {
                var testViewLayoutObj = new OnlyAlwaysUpdateAutoViewLayoutObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewLayoutObj, keyAndValues));
                Assert.IsFalse(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewLayoutObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewLayoutObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(class OnlyAlwaysUpdateAutoViewLayoutObj)!");
        }

        [Test, Description("KeyAndValueが一致しているかどうかのテスト")]
        public void DoKeyAndValueListMatchAnyLayoutPasses()
        {
            var colorLayoutName = "color";
            var alywaysUpdateLayoutName = "always";
            var colorLayouterAccessor = new TestColorViewLayoutAccessor();
            var alwaysUpdateLayoutAccessor = new TestAlwaysUpdateViewLayoutAccessor();
            var viewLayouter = new ViewLayouter(
                (colorLayoutName, colorLayouterAccessor),
                (alywaysUpdateLayoutName, alwaysUpdateLayoutAccessor));

            {
                var testViewObj = new AllUpdateTimingViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(Both AyOnlyModel and Always)!");

            {
                var testViewObj = new AllUpdateTimingViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    { colorLayoutName, Color.blue },
                    //{ alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsFalse(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(Only AyOnlyModel)!");

            {
                var testViewObj = new AllUpdateTimingViewObj();
                var keyAndValues = new Dictionary<string, object>() {
                    //{ colorLayoutName, Color.blue },
                    { alywaysUpdateLayoutName, 123 },
                };
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.All, testViewObj, keyAndValues));
                Assert.IsFalse(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel, testViewObj, keyAndValues));
                Assert.IsTrue(viewLayouter.DoMatchAnyLayout(ViewLayoutAccessorUpdateTiming.Always, testViewObj, keyAndValues));
            }
            Debug.Log($"Success to DoMatchAnyLayout(Only Always)!");

        }
    }
}
