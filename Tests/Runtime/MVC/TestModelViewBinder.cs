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
    public class TestModelViewBinder : TestBase
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

            public void OnCreated(Model targetModel, ModelViewBinderInstanceMap binderInstanceMap)
            {
                UseModel = targetModel;
            }

            public void Dispose() { }

            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                    var m = model as ModelClass;
                    var v = viewObj as IntViewObjClass;
                    if(v.IntValue != m.Value1)
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

            public void OnCreated(Model targetModel, ModelViewBinderInstanceMap binderInstanceMap)
            {
                UseModel = targetModel;
            }

            public void Dispose() { }

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
            var bindInfoList = ModelViewBinder.CreateBindInfoDict(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder()));
            var binder = new ModelViewBinder("apple", bindInfoList);

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
                    var paramBinder = binder.GetParamBinder(intViewObj);
                    Assert.IsNotNull(paramBinder);
                    paramBinder.Update(model, intViewObj);
                    Assert.AreEqual(model.Value1, intViewObj.IntValue);
                }

                {//FloatViewObjClass
                    model.Value2 = 0.987f;
                    var paramBinder = binder.GetParamBinder(floatViewObj);
                    Assert.IsNotNull(paramBinder);
                    paramBinder.Update(model, floatViewObj);
                    Assert.AreEqual(model.Value2, floatViewObj.FloatValue);
                }
            }

            {//
                //Modelの状態が変更された結果クエリパスと一致するようになるかもしれないので、生成自体は行うようにしています。
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                    var empty = new ModelClass { Name = "empty" };
                    var bindInstance = binder.CreateBindInstance(empty, null);
                });
            }
        }
    }
}
