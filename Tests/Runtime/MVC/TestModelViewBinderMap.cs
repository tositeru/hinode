using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
	/// <seealso cref="ModelViewBinderMap"/>
	/// </summary>
    public class TestModelViewBinderMap
    {
        class ModelClass : Model
        {
            public int IntValue { get; set; }
            public float FloatValue { get; set; }
        }

        class IntViewObjClass : IViewObject
        {
            public int IntValue { get; set; }
            public void Dispose() { }

            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                    var m = model as ModelClass;
                    var v = viewObj as IntViewObjClass;
                    v.IntValue = m.IntValue;
                }
            }
        }

        class FloatViewObjClass : IViewObject
        {
            public float FloatValue { get; set; }
            public void Dispose() { }

            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                    var m = model as ModelClass;
                    var v = viewObj as FloatViewObjClass;
                    v.FloatValue = m.FloatValue;
                }
            }
        }

        [Test]
        public void BasicUsagePasses()
        {
            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple" };
            var grape = new ModelClass() { Name = "grape" };
            root.AddChildren(apple, grape);
            var orange = new ModelClass() { Name = "orange" };
            grape.AddChildren(orange);

            var appleBinder = new ModelViewBinder("apple",
                ModelViewBinder.CreateBindInfoDict((typeof(IntViewObjClass), new IntViewObjClass.Binder())));
            var orangeBinder = new ModelViewBinder("orange",
                ModelViewBinder.CreateBindInfoDict((typeof(FloatViewObjClass), new FloatViewObjClass.Binder())));
            Assert.IsFalse(root.DoMatchQueryPath(appleBinder.QueryPath));
            var binderMap = new ModelViewBinderMap(appleBinder, orangeBinder);
            {//Constructorのテスト
                Assert.AreEqual(2, binderMap.Binders.Count());
                Assert.IsTrue(binderMap.Binders.Any(_b => _b == appleBinder), "指定したBinderがBinderMapの中にありません");
                Assert.IsTrue(binderMap.Binders.Any(_b => _b == orangeBinder), "指定したBinderがBinderMapの中にありません");
            }

            {//ModelViewBindMap#CreateBindInstanceのテスト
                var appleBindInstance = binderMap.CreateBindInstance(apple);
                var orangeBindInstance = binderMap.CreateBindInstance(orange);
                Assert.IsNotNull(appleBindInstance);
                Assert.IsNotNull(orangeBindInstance);
                Assert.IsNull(binderMap.CreateBindInstance(root));
                Assert.IsNull(binderMap.CreateBindInstance(grape));

                Assert.AreSame(apple, appleBindInstance.Model);
                Assert.AreSame(appleBinder, appleBindInstance.Binder);
                Assert.AreEqual(1, appleBindInstance.ViewObjects.Count());
                Assert.AreEqual(typeof(IntViewObjClass), appleBindInstance.ViewObjects.First().GetType());

                Assert.AreSame(orange, orangeBindInstance.Model);
                Assert.AreSame(orangeBinder, orangeBindInstance.Binder);
                Assert.AreEqual(1, orangeBindInstance.ViewObjects.Count());
                Assert.AreEqual(typeof(FloatViewObjClass), orangeBindInstance.ViewObjects.First().GetType());
            }

            {//BinderInstanceMapのテスト
                var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

                {//BindInstanceMap#Addのテスト
                    Assert.AreEqual(0, bindInstanceMap.BindInstances.Count());
                    bindInstanceMap.Add(apple, orange);
                    Assert.AreEqual(2, bindInstanceMap.BindInstances.Count());

                    //追加された時は合わせてViewのパラメータもModelのものに更新する
                    var appleViewObj = bindInstanceMap[apple].ViewObjects.First(_v => _v is IntViewObjClass) as IntViewObjClass;
                    Assert.AreEqual(apple.IntValue, appleViewObj.IntValue);
                    var orangeViewObj = bindInstanceMap[orange].ViewObjects.First(_v => _v is FloatViewObjClass) as FloatViewObjClass;
                    Assert.AreEqual(orange.FloatValue, orangeViewObj.FloatValue);

                    //既に追加されていたら追加しない
                    bindInstanceMap.Add(apple, orange);
                    Assert.AreEqual(2, bindInstanceMap.BindInstances.Count(), "同じModelが追加できないようにしてください");
                }

                var appleBindInstance = bindInstanceMap[apple];
                var orangeBindInstance = bindInstanceMap[orange];
                {//BindInstanceMap#[]のテスト
                    Assert.IsNotNull(appleBindInstance);
                    Assert.AreEqual(apple, appleBindInstance.Model);

                    Assert.IsNotNull(orangeBindInstance);
                    Assert.AreEqual(orange, orangeBindInstance.Model);
                }

                {//BindInstanceMap#UpdateViewObjectsのテスト
                    apple.IntValue = 234;
                    orange.FloatValue = 2.5432f;
                    bindInstanceMap.UpdateViewObjects();
                    var appleViewObj = appleBindInstance.ViewObjects.First(_v => _v is IntViewObjClass) as IntViewObjClass;
                    Assert.AreEqual(apple.IntValue, appleViewObj.IntValue);
                    var orangeViewObj = orangeBindInstance.ViewObjects.First(_v => _v is FloatViewObjClass) as FloatViewObjClass;
                    Assert.AreEqual(orange.FloatValue, orangeViewObj.FloatValue);
                }

                {//BindInstanceMap#Removeのテスト
                    bindInstanceMap.Remove(appleBindInstance.Model, orangeBindInstance.Model);
                    Assert.AreEqual(0, bindInstanceMap.BindInstances.Count());
                }
            }
        }

        [Test, Description("クエリパスの優先順位で生成するModelViewBinderInstaceの切り替えのテスト")]
        public void QueryPathPriorityPasses()
        {
            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple" };
            var grape = new ModelClass() { Name = "grape" };
            root.AddChildren(apple, grape);
            var orange = new ModelClass() { Name = "orange" };
            grape.AddChildren(orange);

            var allBinder = new ModelViewBinder("*",
                ModelViewBinder.CreateBindInfoDict((typeof(IntViewObjClass), new IntViewObjClass.Binder())));
            var appleBinder = new ModelViewBinder("apple",
                ModelViewBinder.CreateBindInfoDict((typeof(IntViewObjClass), new IntViewObjClass.Binder())));
            var orangeBinder = new ModelViewBinder("orange",
                ModelViewBinder.CreateBindInfoDict((typeof(FloatViewObjClass), new FloatViewObjClass.Binder())));
            var grapeOrangeBinder = new ModelViewBinder("grape/orange",
                ModelViewBinder.CreateBindInfoDict((typeof(FloatViewObjClass), new FloatViewObjClass.Binder())));
            Assert.IsFalse(root.DoMatchQueryPath(appleBinder.QueryPath));

            var binderMap = new ModelViewBinderMap(
                allBinder,
                appleBinder,
                orangeBinder,
                grapeOrangeBinder);
            var bindInstanceMap = binderMap.CreateBinderInstaceMap();
            bindInstanceMap.Add(root.GetHierarchyEnumerable());

            var rootBinderInstance = bindInstanceMap.BindInstances[root];
            Assert.AreSame(allBinder, rootBinderInstance.Binder);

            var appleBinderInstance = bindInstanceMap.BindInstances[apple];
            Assert.AreSame(appleBinder, appleBinderInstance.Binder);

            // grape/orangeのものを使用すること
            var orangeBinderInstance = bindInstanceMap.BindInstances[orange];
            Assert.AreSame(grapeOrangeBinder, orangeBinderInstance.Binder);

            var grapeBinderInstance = bindInstanceMap.BindInstances[grape];
            Assert.AreSame(allBinder, grapeBinderInstance.Binder);
        }

        [Test, Description("BindInstanceMapの遅延操作のテスト")]
        public void DelayOperationPasses()
        {
            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple" };
            var grape = new ModelClass() { Name = "grape" };
            root.AddChildren(apple, grape);
            var orange = new ModelClass() { Name = "orange" };
            grape.AddChildren(orange);

            var allBinder = new ModelViewBinder("*",
                ModelViewBinder.CreateBindInfoDict((typeof(IntViewObjClass), new IntViewObjClass.Binder())));

            var binderMap = new ModelViewBinderMap(allBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

            Assert.IsFalse(bindInstanceMap.EnabledDelayOperation);
            bindInstanceMap.EnabledDelayOperation = true;

            {//追加
                bindInstanceMap.Add(root.GetHierarchyEnumerable());
                Assert.IsFalse(bindInstanceMap.BindInstances.Any(), "遅延操作が有効になっている場合はBindInstanceMap#DoDelayOperation()を呼び出されるまで、追加処理を実行しないでください。");

                bindInstanceMap.DoDelayOperations();
                Assert.IsTrue(bindInstanceMap.BindInstances.Any());

                {//一度操作が実行された後は同じ操作を繰り返し実行されないようにする
                    var e = bindInstanceMap.GetDoDelayOperationsEnumerator();
                    var opCount = 0;
                    while(e.MoveNext() && e.Current != null)
                    {
                        opCount++;
                    }
                    Assert.AreEqual(0, opCount, "一度操作を実行した後はそれを削除するようにしてください");
                }
            }

            {//更新は対象外
                //bindInstanceMap.UpdateViewObjects();
                //bindInstanceMap.DoDelayOperations();
            }

            {//削除
                var instanceCount = bindInstanceMap.BindInstances.Count();
                bindInstanceMap.Remove(bindInstanceMap.BindInstances.Keys);
                Assert.AreEqual(instanceCount, bindInstanceMap.BindInstances.Count()
                    , "遅延操作が有効になっている場合はBindInstanceMap#DoDelayOperation()を呼び出されるまで、削除処理を実行しないでください。");

                bindInstanceMap.DoDelayOperations();
                Assert.IsFalse(bindInstanceMap.BindInstances.Any());
            }

            {//追加と削除が合わせて遅延処理に登録されていたら、何もしないようにする
                bindInstanceMap.Add(root.GetHierarchyEnumerable());
                bindInstanceMap.Remove(root.GetHierarchyEnumerable());
                Assert.IsFalse(bindInstanceMap.BindInstances.Any(), "遅延操作が有効になっている場合はBindInstanceMap#DoDelayOperation()を呼び出されるまで、追加・削除処理を実行しないでください。");

                bindInstanceMap.DoDelayOperations();
                Assert.IsFalse(bindInstanceMap.BindInstances.Any(), "同じModelの追加・削除を遅延処理で同時に実行される場合は何もしないようにしてください");
            }
        }

        [Test, Description("BindInstanceMapの遅延操作のテスト(GetEnumerator)")]
        public void GetDoDelayOperationsEnumeratorPasses()
        {
            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple" };
            var grape = new ModelClass() { Name = "grape" };
            root.AddChildren(apple, grape);
            var orange = new ModelClass() { Name = "orange" };
            grape.AddChildren(orange);

            var allBinder = new ModelViewBinder("*",
                ModelViewBinder.CreateBindInfoDict((typeof(IntViewObjClass), new IntViewObjClass.Binder())));

            var binderMap = new ModelViewBinderMap(allBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

            Assert.IsFalse(bindInstanceMap.EnabledDelayOperation);
            bindInstanceMap.EnabledDelayOperation = true;

            {//操作は対象になっているIModel分だけ生成されるようにする
                bindInstanceMap.Add(root.GetHierarchyEnumerable());
                bindInstanceMap.Remove(root.GetHierarchyEnumerable());

                var correctOpCount = root.GetHierarchyEnumerable().Count();
                var enumerator = bindInstanceMap.GetDoDelayOperationsEnumerator();

                var opCount = 0;
                while(enumerator.MoveNext() && enumerator.Current != null)
                {
                    opCount++;
                }

                Assert.AreEqual(correctOpCount, opCount, "登録された操作の個数が想定されたものになっていません。操作対象となっているIModelの数だけ操作を生成してくください。");

                bindInstanceMap.Add(root.GetHierarchyEnumerable());
                correctOpCount = root.GetHierarchyEnumerable().Count();
                opCount = 0;
                while(enumerator.MoveNext() && enumerator.Current != null)
                {
                    opCount++;
                }
                Assert.AreEqual(correctOpCount, opCount, "再び操作が追加された後でも、ModelViewBinderInstanceMap#GetDoDelayOperationsEnumerator関数の返り値のIEnumeratorが使用できるようにしてください。");
            }
        }

    }
}
