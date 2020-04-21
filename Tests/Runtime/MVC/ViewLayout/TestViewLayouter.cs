using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewLayout
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

            protected override object GetImpl(IViewObject viewObj)
            {
                return (viewObj as ITestColorViewLayout).ColorLayout;
            }

            protected override void SetImpl(object value, IViewObject viewObj)
            {
                (viewObj as ITestColorViewLayout).ColorLayout = (Color)value;
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

            protected override object GetImpl(IViewObject viewObj)
            {
                return (viewObj as ITestShapeViewLayout).ShapeNoLayout;
            }

            protected override void SetImpl(object value, IViewObject viewObj)
            {
                (viewObj as ITestShapeViewLayout).ShapeNoLayout = (int)value;
            }
        }

        class TestAutoViewObj : IViewObject, ITestColorViewLayout
        {
            public Color ColorLayout { get; set; }

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            { }
            public void Unbind() { }

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class AutoTestColorViewLayoutCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IViewObject CreateImpl(IViewObject viewObj)
            {
                return new TestAutoViewObj()
                {
                    UseBinderInstance = viewObj.UseBinderInstance
                };
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return new[] { typeof(ITestColorViewLayout) };
            }
        }

        class TestViewObj : IViewObject
            , ITestColorViewLayout
        {
            public Color ColorLayout { get; set; } = Color.white;

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            { }
            public void Unbind() {}

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class TestView2Obj : IViewObject
        {
            public Color ColorLayout { get; set; } = Color.white;

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            { }
            public void Unbind() { }

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
            Assert.Throws<System.ArgumentException>(() => {
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
            viewLayouter.SetAllMatchLayouts(testViewObj, keyAndValues);
            Assert.AreEqual(setColor, testViewObj.ColorLayout);

            var testView2Obj = new TestView2Obj();
            viewLayouter.SetAllMatchLayouts(testView2Obj, keyAndValues);
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
            var useBindInfo = new ModelViewBinder.BindInfo(typeof(TestViewObj));
            useBindInfo.AddViewLayout("color", Color.blue);
            var useBindInfo2 = new ModelViewBinder.BindInfo(typeof(TestView2Obj))//<- Not include IViewLayout
                    .AddViewLayout("color", Color.green);
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

            Assert.DoesNotThrow(() => {
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
            useBindInfo.AddViewLayout("color", Color.blue);
            var useBindInfo2 = new ModelViewBinder.BindInfo(typeof(TestView2Obj))//<- Not include IViewLayout
                    .AddViewLayout("color", Color.green);
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

            Assert.DoesNotThrow(() => {
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
                Assert.AreSame(view2Obj.UseModel, autoViewObj.UseModel);
                Assert.AreSame(view2Obj.UseBindInfo, autoViewObj.UseBindInfo);
                Assert.AreSame(view2Obj.UseBinderInstance, autoViewObj.UseBinderInstance);
            }
        }

        class NotSupportViewLayoutCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IViewObject CreateImpl(IViewObject viewObj)
            {
                return new ViewObj();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return new[] { typeof(ITestColorViewLayout) };
            }

            public class ViewObj : IViewObject
            {
                public Model UseModel { get; set; }
                public ModelViewBinder.BindInfo UseBindInfo { get; set; }
                public ModelViewBinderInstance UseBinderInstance { get; set; }

                public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
                { }
                public void Unbind() { }

                public class ParamBinder : IModelViewParamBinder
                {
                    public void Update(Model model, IViewObject viewObj)
                    {
                    }
                }
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
                    .AddViewLayout("color", Color.green);
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

            Assert.Throws<System.Exception>(() => {
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
                foreach(var creator in viewLayouter.GetAutoViewObjectCreator(viewObj, useBindInfo.ViewLayouts.Keys))
                {
                    creator.Create(viewObj); // <- Here may be throw Exception...
                }
            });
        }

    }
}
