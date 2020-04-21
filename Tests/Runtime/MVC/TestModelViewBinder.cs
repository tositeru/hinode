using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="IViewObject"/>
    /// <seealso cref="IModelViewParamBinder"/>
    /// </summary>
    public class TestModelViewBinder
    {
        class ModelClass : Model
        {
            public int Value1 { get; set; }
            public float Value2 { get; set; }
        }

        class IntViewObjClass : IViewObject
        {
            public int IntValue { get; set; }

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
            }
            public void Unbind() { }

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

        class FloatViewObjClass : IViewObject
        {
            public float FloatValue { get; set; }

            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }
            public ModelViewBinderInstance UseBinderInstance { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
            }

            public void Unbind() { }

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

                    var errorMessage = "ModelViewBindInstace#AttachModelOnUpdated()を呼び出すまでModel#OnUpdatedと連動しないようにしてください";
                    Assert.AreNotEqual(obj.Value1, intViewObj.IntValue, errorMessage);
                    Assert.AreNotEqual(obj.Value2, floatViewObj.FloatValue, errorMessage);

                    errorMessage = "ModelViewBindInstace#AttachModelOnUpdated()を呼び出した後はModel#OnUpdatedと連動するようにしてください";
                    bindInstance.AttachModelOnUpdated();
                    obj.DoneUpdate();
                    Assert.AreEqual(obj.Value1, intViewObj.IntValue, errorMessage);
                    Assert.AreEqual(obj.Value2, floatViewObj.FloatValue, errorMessage);

                    obj.Value1 = -1234;
                    obj.DoneUpdate();
                    Assert.AreEqual(obj.Value1, intViewObj.IntValue, errorMessage);

                    //
                    errorMessage = $"ModelViewBindInstace#DettachModelOnUpdated()を呼び出した後はModel#OnUpdatedとの連しないようにしてください。";
                    bindInstance.DettachModelOnUpdated();
                    obj.Value1 = -678;
                    obj.DoneUpdate();
                    Assert.AreNotEqual(obj.Value1, intViewObj.IntValue, errorMessage);
                }

                {//ModelViewBindInstance#Disposeテスト
                    bindInstance.Dispose();
                    obj.Value1 = -48593;
                    obj.DoneUpdate();
                    Assert.AreNotEqual(obj.Value1, intViewObj.IntValue, $"ModelViewBindInstance#DisposeのあとはModel#OnUpdatedと連動しないようにしてください。");
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
                var queryViewResult = binderInstance.QueryViews(typeof(IntViewObjClass).FullName);
                Assert.AreEqual(1, queryViewResult.Count());
                var intViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID == typeof(IntViewObjClass).FullName);
                Assert.IsTrue(intViewObj == queryViewResult.First());
            }

            {//query => typeof(FloatViewObjClass).FullName
                var queryViewResult = binderInstance.QueryViews(typeof(FloatViewObjClass).FullName);
                Assert.AreEqual(1, queryViewResult.Count());
                var floatViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID == typeof(FloatViewObjClass).FullName);
                Assert.IsTrue(floatViewObj == queryViewResult.First());
            }

            {//query => Int
                var queryViewResult = binderInstance.QueryViews("Int");
                Assert.AreEqual(1, queryViewResult.Count());
                var intIdViewObj = binderInstance.ViewObjects.First(_v => _v.UseBindInfo.ID == "Int");
                Assert.IsTrue(intIdViewObj == queryViewResult.First());
            }

        }

    }
}
