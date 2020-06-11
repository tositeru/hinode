using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests
{
    /// <summary>
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="IViewObject"/>
    /// <seealso cref="IModelViewParamBinder"/>
    /// </summary>
    public class TestModelViewBinder
    {
        [SetUp]
        public void SetUp()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        class ModelClass : Model
        {
            public int Value1 { get; set; }
            public float Value2 { get; set; }
        }

        class IntViewObjClass : EmptyViewObject
        {
            public int IntValue { get; set; }

            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                    var m = model as ModelClass;
                    var v = viewObj as IntViewObjClass;
                    if (v.IntValue != m.Value1)
                    {
                        v.IntValue = m.Value1;
                    }
                }
            }
        }

        class FloatViewObjClass : EmptyViewObject
        {
            public float FloatValue { get; set; }

            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                    var m = model as ModelClass;
                    var v = viewObj as FloatViewObjClass;
                    v.FloatValue = m.Value2;
                }
            }
        }

        [Test]
        public void BasicUsagePasses()
        {
            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var empry = new ModelClass { Name = "empty" };

            //作成のテスト
            //var bindInfoList = ModelViewBinder.CreateBindInfoDict(
            //    (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
            //    (typeof(FloatViewObjClass), new FloatViewObjClass.Binder()));
            //var binder = new ModelViewBinder("apple", bindInfoList);

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binder = new ModelViewBinder("apple", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
            );
            Assert.AreSame(binder.ViewInstaceCreator, viewInstanceCreator);
            { //QueryPathの検証のテスト
                Assert.IsTrue(binder.DoMatch(model));
                Assert.IsFalse(binder.DoMatch(new Model { Name = "orange" }));
            }

            {//ViewObjectの作成のテスト
                var bindInstance = binder.CreateBindInstance(model, null);
                var intViewObj = bindInstance.ViewObjects.FirstOrDefault(_o => _o is IntViewObjClass) as IntViewObjClass;
                var floatViewObj = bindInstance.ViewObjects.FirstOrDefault(_o => _o is FloatViewObjClass) as FloatViewObjClass;
                Assert.IsNotNull(intViewObj);
                Assert.IsNotNull(floatViewObj);

                bindInstance.UpdateViewObjects();
                Assert.AreEqual(model.Value1, intViewObj.IntValue);
                Assert.AreEqual(model.Value2, floatViewObj.FloatValue);

                //対応するModelViewBinderの取得のテスト
                {//IntViewObjClass
                    model.Value1 = -1234;
                    Assert.IsTrue(binder.BindInfos.Any(_i => _i == intViewObj.UseBindInfo));
                    var paramBinder = binder.GetParamBinder(intViewObj.UseBindInfo);
                    Assert.IsNotNull(paramBinder);
                    Assert.AreEqual(typeof(IntViewObjClass.Binder), paramBinder.GetType());
                    paramBinder.Update(model, intViewObj);
                    Assert.AreEqual(model.Value1, intViewObj.IntValue);
                }

                {//FloatViewObjClass
                    model.Value2 = 0.987f;
                    Assert.IsTrue(binder.BindInfos.Any(_i => _i == floatViewObj.UseBindInfo));
                    var paramBinder = binder.GetParamBinder(floatViewObj.UseBindInfo);
                    Assert.IsNotNull(paramBinder);
                    Assert.AreEqual(typeof(FloatViewObjClass.Binder), paramBinder.GetType());
                    paramBinder.Update(model, floatViewObj);
                    Assert.AreEqual(model.Value2, floatViewObj.FloatValue);
                }
            }
        }

        [Test]
        public void AddInfosFromOtherBinderPasses()
        {
            {//
                var binder = new ModelViewBinder("apple", null,
                    new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                    new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
                );
                var otherBinder = new ModelViewBinder("apple", null,
                    new ModelViewBinder.BindInfo("otherInt", typeof(IntViewObjClass)),
                    new ModelViewBinder.BindInfo("otherFloat", typeof(FloatViewObjClass))
                );

                binder.AddInfosFromOtherBinder(otherBinder);
                AssertionUtils.AssertEnumerableByUnordered(
                    binder.BindInfos.Concat(otherBinder.BindInfos).Distinct()
                    , binder.BindInfos, "");
            }

            {//同じBindInfoを持つModelViewBinderの場合
                var intID = "intId";
                var intViewBindInfo = new ModelViewBinder.BindInfo(intID, typeof(IntViewObjClass));
                var floatViewBindInfo = new ModelViewBinder.BindInfo(typeof(FloatViewObjClass));
                var otherFloatViewBindInfo = new ModelViewBinder.BindInfo("otherFloat", typeof(FloatViewObjClass));
                var sameIDViewBindInfo = new ModelViewBinder.BindInfo(intID, typeof(IntViewObjClass));
                var binder = new ModelViewBinder("apple", null,
                    intViewBindInfo, floatViewBindInfo
                );
                var otherBinder = new ModelViewBinder("orange", null,
                    intViewBindInfo, // <- not add
                    otherFloatViewBindInfo
                );
                var otherBinder2 = new ModelViewBinder("grape", null,
                    sameIDViewBindInfo // <- not add
                );

                binder
                    .AddInfosFromOtherBinder(otherBinder)
                    .AddInfosFromOtherBinder(otherBinder2);
                AssertionUtils.AssertEnumerableByUnordered(new ModelViewBinder.BindInfo[] {
                    intViewBindInfo, floatViewBindInfo, otherFloatViewBindInfo,
                   }, binder.BindInfos, "");
            }
        }

        [Test, Description("Model#OnUpdatedと連動しているか確認するテスト")]
        public void ModelOnUpdatedPasses()
        {
            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var empry = new ModelClass { Name = "empty" };

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binder = new ModelViewBinder("apple", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
            );

            {//Model#OnUpdatedと連動しているかテスト
                var obj = new ModelClass() { Name = "apple", Value1 = 456, Value2 = 6.7f };
                var bindInstance = binder.CreateBindInstance(obj, null);
                var intViewObj = bindInstance.ViewObjects.FirstOrDefault(_o => _o is IntViewObjClass) as IntViewObjClass;
                var floatViewObj = bindInstance.ViewObjects.FirstOrDefault(_o => _o is FloatViewObjClass) as FloatViewObjClass;

                {//Model#DoneUpdatedとの連動テスト
                    obj.DoneUpdate();

                    var errorMessage = "ModelViewBindInstaceの生成時には指定したModel#OnUpdatedと連動するようにしてください";
                    obj.Value1 = 9847;
                    obj.DoneUpdate();
                    Assert.AreEqual(obj.Value1, intViewObj.IntValue, errorMessage);
                    Assert.AreEqual(obj.Value2, floatViewObj.FloatValue, errorMessage);

                    obj.Value1 = -1234;
                    obj.DoneUpdate();
                    Assert.AreEqual(obj.Value1, intViewObj.IntValue, errorMessage);

                    //
                    errorMessage = $"ModelViewBindInstace#DettachModelOnUpdated()を呼び出した後はModel#OnUpdatedとの連しないようにしてください。";
                    bindInstance.DettachModelCallback();
                    obj.Value1 = -678;
                    obj.DoneUpdate();
                    Assert.AreNotEqual(obj.Value1, intViewObj.IntValue, errorMessage);

                    errorMessage = "ModelViewBindInstace#AttachModelOnUpdated()を呼び出した後は設定されたModelのModel#OnUpdatedと連動するようにしてください";
                    bindInstance.AttachModelCallback();
                    obj.Value1 = 939753;
                    obj.DoneUpdate();
                    Assert.AreEqual(obj.Value1, intViewObj.IntValue, errorMessage);
                    Assert.AreEqual(obj.Value2, floatViewObj.FloatValue, errorMessage);
                }

                {//ModelViewBindInstance#Disposeテスト
                    bindInstance.Dispose();
                    obj.Value1 = -48593;
                    obj.DoneUpdate();
                    Assert.AreNotEqual(obj.Value1, intViewObj.IntValue, $"ModelViewBindInstance#DisposeのあとはModel#OnUpdatedと連動しないようにしてください。");

                    Assert.IsNull(bindInstance.Model);
                    Assert.IsNull(bindInstance.Binder);
                    Assert.IsNull(bindInstance.UseInstanceMap);
                    Assert.IsFalse(bindInstance.ViewObjects.Any());
                    Assert.IsFalse(bindInstance.AutoLayoutViewObjects.Any());
                    Assert.IsFalse(bindInstance.EventDispatcherHelpObjectsForView.Any());
                }
            }
        }

        [Test, Description("QueryPathと一致しない時のテスト")]
        public void DonotMatchQueryPathFailed()
        {
            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var empry = new ModelClass { Name = "empty" };

            //作成のテスト
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binder = new ModelViewBinder("apple", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
            );

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var empty = new ModelClass { Name = "empty" };
                var bindInstance = binder.CreateBindInstance(empty, null);
            }, "クエリパスが一致しない時は例外を投げて、生成しないようにしてください");
        }

        [Test, Description("有効な型を指定した時のDoMatchのテスト")]
        public void DoMatchWithModelTypePasses()
        {
            //作成のテスト
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var appleBinder = new ModelViewBinder("apple", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
            );
            var matchModel = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var notMatchQueryModel = new ModelClass { Name = "empty" };
            var notMatchTypeModel = new Model { Name = "apple" };

            {// Enabled Model Type => ModelClass
                appleBinder.AddEnabledModelType<ModelClass>();

                Assert.IsTrue(appleBinder.ContainEnabledModelType<ModelClass>());
                Assert.IsFalse(appleBinder.ContainEnabledModelType<Model>());

                Assert.IsTrue(appleBinder.DoMatch(matchModel));
                Assert.IsFalse(appleBinder.DoMatch(notMatchQueryModel));
                Assert.IsFalse(appleBinder.DoMatch(notMatchTypeModel));

                Assert.IsTrue(appleBinder.ContainEnabledModelType(matchModel));
                Assert.IsTrue(appleBinder.ContainEnabledModelType(notMatchQueryModel));
                Assert.IsFalse(appleBinder.ContainEnabledModelType(notMatchTypeModel));
            }

            {// Enabled Model Type => ModelClass, Model
                appleBinder.AddEnabledModelType<Model>();
                Assert.IsTrue(appleBinder.ContainEnabledModelType<ModelClass>());
                Assert.IsTrue(appleBinder.ContainEnabledModelType<Model>());

                Assert.IsTrue(appleBinder.DoMatch(matchModel));
                Assert.IsFalse(appleBinder.DoMatch(notMatchQueryModel));
                Assert.IsTrue(appleBinder.DoMatch(notMatchTypeModel));

                Assert.IsTrue(appleBinder.ContainEnabledModelType(matchModel));
                Assert.IsTrue(appleBinder.ContainEnabledModelType(notMatchQueryModel));
                Assert.IsTrue(appleBinder.ContainEnabledModelType(notMatchTypeModel));
            }

            {// Enabled Model Type => Model
                appleBinder.RemoveEnabledModelType<ModelClass>();
                Assert.IsFalse(appleBinder.ContainEnabledModelType<ModelClass>());
                Assert.IsTrue(appleBinder.ContainEnabledModelType<Model>());

                Assert.IsFalse(appleBinder.DoMatch(matchModel));
                Assert.IsFalse(appleBinder.DoMatch(notMatchQueryModel));
                Assert.IsTrue(appleBinder.DoMatch(notMatchTypeModel));

                Assert.IsFalse(appleBinder.ContainEnabledModelType(matchModel));
                Assert.IsFalse(appleBinder.ContainEnabledModelType(notMatchQueryModel));
                Assert.IsTrue(appleBinder.ContainEnabledModelType(notMatchTypeModel));
            }
        }

        [Test, Description("QueryPathが空のときに有効な型を指定した時のDoMatchのテスト")]
        public void DoMatchWithModelTypeAndEmptyQueryPathPasses()
        {
            //作成のテスト
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var appleBinder = new ModelViewBinder("", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass))
            );
            appleBinder.AddEnabledModelType<ModelClass>();

            var matchModel = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var notMatchQueryModel = new ModelClass { Name = "empty" };
            var notMatchTypeModel = new Model { Name = "apple" };
            Assert.IsTrue(appleBinder.DoMatch(matchModel));
            Assert.IsTrue(appleBinder.DoMatch(notMatchQueryModel));
            Assert.IsFalse(appleBinder.DoMatch(notMatchTypeModel));

            appleBinder.AddEnabledModelType<Model>();
            Assert.IsTrue(appleBinder.DoMatch(matchModel));
            Assert.IsTrue(appleBinder.DoMatch(notMatchQueryModel));
            Assert.IsTrue(appleBinder.DoMatch(notMatchTypeModel));

            appleBinder.RemoveEnabledModelType<ModelClass>();
            Assert.IsFalse(appleBinder.DoMatch(matchModel));
            Assert.IsFalse(appleBinder.DoMatch(notMatchQueryModel));
            Assert.IsTrue(appleBinder.DoMatch(notMatchTypeModel));
        }

        [Test, Description("QueryViewsのテスト")]
        public void QueryViewsPasses()
        {
            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var empry = new ModelClass { Name = "empty" };

            //作成のテスト
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binder = new ModelViewBinder("apple", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)),
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)),
                new ModelViewBinder.BindInfo("Int", typeof(IntViewObjClass))
            );
            var binderInstance = binder.CreateBindInstance(model, null);

            {//query => typeof(IntViewObjClass).FullName
                var ID = ModelViewBinder.BindInfo.ToID(typeof(IntViewObjClass));
                var queryViewResult = binderInstance.QueryViews(ID);
                Assert.AreEqual(1, queryViewResult.Count(), $"Failed to Query Views... ID={ID}");
                var intViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID == ID);
                Assert.IsTrue(intViewObj == queryViewResult.First());
            }

            {//query => typeof(FloatViewObjClass).FullName
                var ID = ModelViewBinder.BindInfo.ToID(typeof(FloatViewObjClass));
                var queryViewResult = binderInstance.QueryViews(ID);
                Assert.AreEqual(1, queryViewResult.Count());
                var floatViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID == ID);
                Assert.IsTrue(floatViewObj == queryViewResult.First());
            }

            {//query => Int
                var queryViewResult = binderInstance.QueryViews("Int");
                Assert.AreEqual(1, queryViewResult.Count());
                var intIdViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID.MainID == "Int");
                Assert.IsTrue(intIdViewObj == queryViewResult.First());
            }

        }

        interface ITestViewLayout : IViewLayout
        {
            int TestViewLayout { get; set; }
        }

        class TestViewLayoutAccessor : IViewLayoutAccessor
        {
            public override Type ViewLayoutType { get => typeof(ITestViewLayout); }
            public override Type ValueType { get => typeof(int); }
            public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

            protected override object GetImpl(object viewLayoutObj)
                => (viewLayoutObj as ITestViewLayout).TestViewLayout;

            protected override void SetImpl(object value, object viewLayoutObj)
                => (viewLayoutObj as ITestViewLayout).TestViewLayout = (int)value;
        }

        class OnViewLayoutViewObj : EmptyViewObject
            , ITestViewLayout
        {
            public int TestViewLayout { get; set; }

            public int OnViewLayoutValue { get; set; }
            public override void OnViewLayouted()
            {
                base.OnViewLayouted();
                OnViewLayoutValue = 100;
            }
        }

        [Test, Description("ModelViewBindInstance#ApplyViewLayout()の時にIViewObject#OnViewLayoutが呼び出されるかテスト")]
        public void OnViewLayoutPasses()
        {
            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f };
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(OnViewLayoutViewObj), new EmptyModelViewParamBinder())
            );
            var binder = new ModelViewBinder("*", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(OnViewLayoutViewObj))
                    .AddViewLayoutValue("testLayout", 5)
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, binder);
            binderMap.UseViewLayouter = new ViewLayouter()
                .AddKeywords(("testLayout", new TestViewLayoutAccessor()));

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.Add(model);

            //Layoutの更新が行われたものだけにIViewObject#OnViewLayoutが呼び出されるようにしています。
            var viewObj = binderInstanceMap[model].ViewObjects.First(_v => _v is OnViewLayoutViewObj)
                as OnViewLayoutViewObj;
            {
                viewObj.OnViewLayoutValue = 0;
                binderInstanceMap[model].ApplyViewLayout(ViewLayoutAccessorUpdateTiming.All);
                Assert.AreEqual(100, viewObj.OnViewLayoutValue);
            }
            Debug.Log($"Success at ViewLayoutAccessorUpdateTiming.All!(Call OnViewLayout)");
            {
                viewObj.OnViewLayoutValue = 0;
                binderInstanceMap[model].ApplyViewLayout(ViewLayoutAccessorUpdateTiming.Always);
                Assert.AreEqual(100, viewObj.OnViewLayoutValue);
            }
            Debug.Log($"Success at ViewLayoutAccessorUpdateTiming.Always!(Not Call OnViewLayout)");
            {
                viewObj.OnViewLayoutValue = 0;
                binderInstanceMap[model].ApplyViewLayout(ViewLayoutAccessorUpdateTiming.AtOnlyModel);
                Assert.AreEqual(100, viewObj.OnViewLayoutValue);
            }
            Debug.Log($"Success at ViewLayoutAccessorUpdateTiming.AtOnlyModel!(Call OnViewLayout)");
        }

        [Test]
        public void InitialViewLayoutStatePasses()
        {
            var styleID = ".styleID";
            var layoutName1 = "testLayout";
            var layoutName2 = "test2";

            var layoutName1Value = 5;
            var layoutName2Value = "testText";
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(OnViewLayoutViewObj), new EmptyModelViewParamBinder())
            );
            var binder = new ModelViewBinder("*", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(OnViewLayoutViewObj))
                    .AddViewLayoutValue(layoutName1, layoutName1Value)
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, binder);
            binderMap.UseViewLayouter = new ViewLayouter()
                .AddKeywords(("testLayout", new TestViewLayoutAccessor()));
            binderMap.UseViewLayoutOverwriter = new ViewLayoutOverwriter()
                .Add(new ViewLayoutSelector(styleID, ""), new ViewLayoutValueDictionary().AddValue(layoutName2, layoutName2Value));

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f }
                .AddStylingID(styleID);
            binderInstanceMap.Add(model);


            var binderInstance = binderInstanceMap[model];
            var viewObject = binderInstance.ViewObjects.First();
            Assert.AreEqual(1, binderInstance.ViewLayoutStateDict.Count);
            Assert.IsTrue(binderInstance.ViewLayoutStateDict.ContainsKey(viewObject));

            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[] {
                    (layoutName1, layoutName1Value),
                    (layoutName2, layoutName2Value)
                }
                , viewObject.GetViewLayoutState()
                , "");
        }

        [Test]
        public void ViewLayoutStateOnChangedValuePasses()
        {
            var styleID = ".styleID";
            var layoutName1 = "testLayout";
            var layoutName2 = "test2";

            var layoutName1Value = 5;
            var layoutName2Value = "testText";
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(OnViewLayoutViewObj), new EmptyModelViewParamBinder())
            );
            var binder = new ModelViewBinder("*", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(OnViewLayoutViewObj))
                    .AddViewLayoutValue(layoutName1, layoutName1Value)
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, binder);
            binderMap.UseViewLayouter = new ViewLayouter()
                .AddKeywords((layoutName1, new TestViewLayoutAccessor()));
            binderMap.UseViewLayoutOverwriter = new ViewLayoutOverwriter()
                .Add(new ViewLayoutSelector(styleID, ""), new ViewLayoutValueDictionary()
                    .AddValue(layoutName2, layoutName2Value));

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f }
                .AddStylingID(styleID);
            binderInstanceMap.Add(model);


            var binderInstance = binderInstanceMap[model];
            Assert.IsNotNull(binderInstance.UseInstanceMap);
            Assert.IsNotNull(binderInstance.UseInstanceMap.UseViewLayouter);

            var viewObject = binderInstance.ViewObjects.OfType<OnViewLayoutViewObj>().First();
            viewObject.GetViewLayoutState().SetRaw(layoutName1, 100);

            Assert.AreEqual(100, viewObject.OnViewLayoutValue, $"Call ViewLayoutState#OnChangedValue Automatic when Change Value in ViewLayoutState of IViewObject if BinderInstance Contains BinderInstanceMap and ViewLayouter.");
        }

        class OnViewLayoutViewObjParamBinder : IModelViewParamBinder
        {
            public string LayoutKey { get; set; }
            public object LayoutValue { get; set; }

            public void Update(Model model, IViewObject viewObj)
            {
                viewObj.GetViewLayoutState()?.SetRaw(LayoutKey, LayoutValue); // <- Test Point
            }
        }

        public class TestAutoViewLayoutObject : EmptyAutoViewLayoutObject
            , ITestViewLayout
        {
            #region ITestViewLayout interface
            public int TestViewLayout { get; set; }
            #endregion

            public class Creator : ViewLayouter.IAutoViewObjectCreator
            {
                public override IEnumerable<Type> GetSupportedIViewLayouts()
                {
                    return new System.Type[] { typeof(ITestViewLayout) };
                }

                protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
                {
                    return new TestAutoViewLayoutObject();
                }
            }
        }
        [Test]
        public void ViewLayoutStateOnChangedValueInIModelViewParamBinderPasses()
        {
            var styleID = ".styleID";
            var layoutName1 = "testLayout";

            var layoutName1Value = 5;
            var paramBinder = new OnViewLayoutViewObjParamBinder() {
                LayoutKey = layoutName1,
                LayoutValue = 100
            };
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(OnViewLayoutViewObj), paramBinder)
            );
            var binder = new ModelViewBinder("*", viewInstanceCreator,
                new ModelViewBinder.BindInfo(typeof(OnViewLayoutViewObj))
                    .AddViewLayoutValue(layoutName1, layoutName1Value)
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, binder)
            {
                UseViewLayouter = new ViewLayouter()
                    .AddKeywords((layoutName1, new TestViewLayoutAccessor()))
                    //.AddAutoCreateViewObject(new TestAutoViewLayoutObject.Creator(), layoutName1)
            };

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new ModelClass { Name = "apple", Value1 = 111, Value2 = 1.234f }
                .AddStylingID(styleID);
            Assert.DoesNotThrow(() => {
                binderInstanceMap.Add(model); // <- Test Point
            });

            var binderInstance = binderInstanceMap[model];
            Assert.IsNotNull(binderInstance.UseInstanceMap);
            Assert.IsNotNull(binderInstance.UseInstanceMap.UseViewLayouter);

            var viewObject = binderInstance.ViewObjects.OfType<OnViewLayoutViewObj>().First();

            var errorMessage = $"Call ViewLayoutState#OnChangedValue Automatic when Change Value in ViewLayoutState of IViewObject if BinderInstance Contains BinderInstanceMap and ViewLayouter.";
            Assert.AreEqual(paramBinder.LayoutValue, viewObject.TestViewLayout, errorMessage);
            Assert.AreEqual(100, viewObject.OnViewLayoutValue, errorMessage);
        }

    }
}
