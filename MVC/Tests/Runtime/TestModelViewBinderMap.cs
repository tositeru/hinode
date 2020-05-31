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
	/// <seealso cref="ModelViewBinderMap"/>
	/// </summary>
    public class TestModelViewBinderMap
    {
        [SetUp]
        public void Setup()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        class ModelClass : Model
        {
            public int IntValue { get; set; }
            public float FloatValue { get; set; }
        }

        /// <summary>
        /// 何もしないViewオブジェクト
        /// </summary>
        class EmptyViewObjClass : EmptyViewObject
        {
            public class Binder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
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
                    v.IntValue = m.IntValue;
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

            var appleBinder = new ModelViewBinder("apple", null,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var orangeBinder = new ModelViewBinder("orange", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            var rebindBinder = new ModelViewBinder("Rebind", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            Assert.IsFalse(root.DoMatchQueryPath(appleBinder.Query));
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, appleBinder, orangeBinder, rebindBinder);
            {//Constructorのテスト
                Assert.AreEqual(3, binderMap.Binders.Count());
                Assert.IsTrue(binderMap.Binders.Any(_b => _b == appleBinder), "指定したBinderがBinderMapの中にありません");
                Assert.IsTrue(binderMap.Binders.Any(_b => _b == orangeBinder), "指定したBinderがBinderMapの中にありません");
                Assert.IsTrue(binderMap.Binders.Any(_b => _b == rebindBinder), "指定したBinderがBinderMapの中にありません");

                Assert.AreSame(viewInstanceCreator, binderMap.ViewInstanceCreator);
                string errorMessage = "ModelViewBinderMapに設定されたModelViewBinder#ViewInstaceCreatorはModelViewBinderMap#ViewInstanceCreatorと同じものになるようにしてください。";
                Assert.AreSame(binderMap.ViewInstanceCreator, appleBinder.ViewInstaceCreator, errorMessage);
                Assert.AreSame(binderMap.ViewInstanceCreator, orangeBinder.ViewInstaceCreator, errorMessage);
                Assert.AreSame(binderMap.ViewInstanceCreator, rebindBinder.ViewInstaceCreator, errorMessage);
            }

            {//ModelViewBindMap#CreateBindInstanceのテスト
                var appleBindInstance = binderMap.CreateBindInstance(apple, null);
                var orangeBindInstance = binderMap.CreateBindInstance(orange, null);
                Assert.IsNotNull(appleBindInstance);
                Assert.IsNotNull(orangeBindInstance);
                Assert.IsNull(binderMap.CreateBindInstance(root, null));
                Assert.IsNull(binderMap.CreateBindInstance(grape, null));

                Assert.AreSame(apple, appleBindInstance.Model);
                Assert.AreSame(appleBinder, appleBindInstance.Binder);
                Assert.AreEqual(1, appleBindInstance.ViewObjects.Count());
                Assert.AreEqual(typeof(IntViewObjClass), appleBindInstance.ViewObjects.First().GetType());

                Assert.AreSame(orange, orangeBindInstance.Model);
                Assert.AreSame(orangeBinder, orangeBindInstance.Binder);
                Assert.AreEqual(1, orangeBindInstance.ViewObjects.Count());
                Assert.AreEqual(typeof(FloatViewObjClass), orangeBindInstance.ViewObjects.First().GetType());
            }
        }

        [Test]
        public void BinderInstanceMapBasicUsagePasses()
        {
            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple" };
            var noneBinderModel = new ModelClass() { Name = "grape" };
            root.AddChildren(apple, noneBinderModel);
            var orange = new ModelClass() { Name = "orange" };
            noneBinderModel.AddChildren(orange);

            var appleBinder = new ModelViewBinder("apple", null,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var orangeBinder = new ModelViewBinder("orange", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            var rebindBinder = new ModelViewBinder("Rebind", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            Assert.IsFalse(root.DoMatchQueryPath(appleBinder.Query));
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, appleBinder, orangeBinder, rebindBinder);

            {//BinderInstanceMapのテスト
                var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);
                Assert.AreSame(binderMap, bindInstanceMap.BinderMap);

                {//BindInstanceMap#Addのテスト
                    Assert.AreEqual(0, bindInstanceMap.BindInstances.Count());
                    bindInstanceMap.Add(false, null, apple, noneBinderModel);
                    //grapeはQueryPathと一致しないので追加されない
                    AssertionUtils.AssertEnumerableByUnordered(new Model[] {
                            apple, orange
                        }, bindInstanceMap.BindInstances.Select(_b => _b.Key), "");

                    //追加された時は合わせてViewのパラメータもModelのものに更新する
                    var appleViewObj = bindInstanceMap[apple].ViewObjects.First(_v => _v is IntViewObjClass) as IntViewObjClass;
                    Assert.AreEqual(apple.IntValue, appleViewObj.IntValue);
                    foreach (var viewObj in bindInstanceMap[apple].ViewObjects)
                    {
                        Assert.AreEqual(bindInstanceMap[apple], viewObj.UseBinderInstance);
                    }
                    Assert.AreSame(bindInstanceMap, bindInstanceMap[apple].UseInstanceMap);

                    var orangeViewObj = bindInstanceMap[orange].ViewObjects.First(_v => _v is FloatViewObjClass) as FloatViewObjClass;
                    Assert.AreEqual(orange.FloatValue, orangeViewObj.FloatValue);
                    foreach (var viewObj in bindInstanceMap[orange].ViewObjects)
                    {
                        Assert.AreEqual(bindInstanceMap[orange], viewObj.UseBinderInstance);
                    }

                    //既に追加されていたら追加しない
                    bindInstanceMap.Add(false, null, apple, orange);
                    AssertionUtils.AssertEnumerableByUnordered(new Model[] {
                            apple, orange
                        }, bindInstanceMap.BindInstances.Select(_b => _b.Key), "同じModelが追加できないようにしてください");
                }

                {//マッチしないModelを追加した時のテスト
                    var empty = new Model() { Name = "__empty" };
                    Assert.DoesNotThrow(() =>
                    {
                        bindInstanceMap.Add(empty);
                        Assert.IsFalse(bindInstanceMap.BindInstances.ContainsKey(empty), "マッチしないModelを追加した時はModelViewBinderInstanceを生成しないようにしてください");
                    }, "マッチしないModelを追加した時でもModelViewBinderInstanceMapから例外を発生させないようにしてください。");
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

                {//BindInstanceMap#Rebindのテスト
                    apple.Name = "Rebind";
                    var isSuccess = bindInstanceMap.Rebind(apple);
                    Assert.IsTrue(isSuccess, "Rebindに失敗しています");
                    Assert.AreSame(rebindBinder, bindInstanceMap.BindInstances[apple].Binder);
                    Assert.AreSame(bindInstanceMap, bindInstanceMap.BindInstances[apple].UseInstanceMap);
                }
                {
                    // 追加されていないものをRebindした時は何もしない
                    var recordedBindInstances = bindInstanceMap.BindInstances.ToArray();
                    var model = new ModelClass() { Name = "Tmp" };
                    var isSuccess = bindInstanceMap.Rebind(model);
                    Assert.IsFalse(isSuccess, "登録されていないModelの場合はRebindしないようにしてください");
                    AssertionUtils.AssertEnumerable(recordedBindInstances, bindInstanceMap.BindInstances, "ModelViewBinderInstanceMapに追加されていないModelをRebindした時は何もしないようにしてください。");
                }

                {//BindInstanceMap#Removeのテスト
                    bindInstanceMap.Remove(apple, noneBinderModel);
                    Assert.AreEqual(0, bindInstanceMap.BindInstances.Count());
                }

                {//BindInstanceMap#ClearBindInstancesのテスト
                    bindInstanceMap.Add(false, null, apple, orange);
                    bindInstanceMap.ClearBindInstances();
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
            var orange = new ModelClass() { Name = "orange", LogicalID = new ModelIDList("child") };
            grape.AddChildren(orange);

            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var appleBinder = new ModelViewBinder("apple", null,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var orangeBinder = new ModelViewBinder("orange", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            var childOrangeBinder = new ModelViewBinder("orange #child", null,
                new ModelViewBinder.BindInfo(typeof(FloatViewObjClass)));
            Assert.IsFalse(root.DoMatchQueryPath(appleBinder.Query));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(
                viewInstanceCreator,
                allBinder,
                appleBinder,
                orangeBinder,
                childOrangeBinder);
            var bindInstanceMap = binderMap.CreateBinderInstaceMap();
            bindInstanceMap.Add(false, null, root.GetHierarchyEnumerable());

            var rootBinderInstance = bindInstanceMap.BindInstances[root];
            Assert.AreSame(allBinder, rootBinderInstance.Binder);

            var appleBinderInstance = bindInstanceMap.BindInstances[apple];
            Assert.AreSame(appleBinder, appleBinderInstance.Binder);

            // grape/orangeのものを使用すること
            var orangeBinderInstance = bindInstanceMap.BindInstances[orange];
            Assert.AreSame(childOrangeBinder, orangeBinderInstance.Binder);

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

            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

            Assert.IsFalse(bindInstanceMap.EnabledDelayOperation);
            bindInstanceMap.EnabledDelayOperation = true;

            {//追加
                bindInstanceMap.Add(false, null, root.GetHierarchyEnumerable());
                Assert.IsFalse(bindInstanceMap.BindInstances.Any(), "遅延操作が有効になっている場合はBindInstanceMap#DoDelayOperation()を呼び出されるまで、追加処理を実行しないでください。");

                bindInstanceMap.DoDelayOperations();
                Assert.IsTrue(bindInstanceMap.BindInstances.Any());

                {//一度操作が実行された後は同じ操作を繰り返し実行されないようにする
                    var e = bindInstanceMap.GetDoDelayOperationsEnumerator();
                    var opCount = 0;
                    while (e.MoveNext() && e.Current != null)
                    {
                        opCount++;
                    }
                    Assert.AreEqual(0, opCount, "一度操作を実行した後はそれを削除するようにしてください");
                }
            }

            {//更新 Model#OnUpdatedが呼び出されるかどうかでテストしている
                var viewObj = bindInstanceMap.BindInstances[root].ViewObjects.First() as IntViewObjClass;
                {//ModelViewBindInstanceMap#UpdateViewObjects()
                    root.IntValue = 9874;
                    bindInstanceMap.UpdateViewObjects();
                    Assert.AreNotEqual(viewObj.IntValue, root.IntValue, $"遅延操作が有効な時は、ModelViewBindInstanceMap#DoDelayOperation()が呼び出されるまで更新処理を実行しないでください。");

                    bindInstanceMap.DoDelayOperations();
                    Assert.AreEqual(viewObj.IntValue, root.IntValue, $"遅延操作が更新に対応していません。");
                }

                {//Model#DoneUpdate()
                    root.IntValue++;
                    root.DoneUpdate();
                    Assert.AreNotEqual(viewObj.IntValue, root.IntValue, $"遅延操作が有効な時は、ModelViewBindInstanceMap#DoDelayOperation()が呼び出されるまで更新処理を実行しないでください。");

                    bindInstanceMap.DoDelayOperations();
                    Assert.AreEqual(viewObj.IntValue, root.IntValue, $"遅延操作が更新に対応していません。");
                }
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
                bindInstanceMap.Add(false, null, root.GetHierarchyEnumerable());
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

            var allBinder = new ModelViewBinder("*", null, new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(IntViewObjClass), new IntViewObjClass.Binder()),
                (typeof(FloatViewObjClass), new FloatViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

            Assert.IsFalse(bindInstanceMap.EnabledDelayOperation);
            bindInstanceMap.EnabledDelayOperation = true;

            {//操作は対象になっているIModel分だけ生成されるようにする
                bindInstanceMap.Add(false, null, root.GetHierarchyEnumerable());
                bindInstanceMap.Remove(root.GetHierarchyEnumerable());

                var correctOpCount = root.GetHierarchyEnumerable().Count();
                var enumerator = bindInstanceMap.GetDoDelayOperationsEnumerator();

                var opCount = 0;
                while (enumerator.MoveNext() && enumerator.Current != null)
                {
                    opCount++;
                }

                Assert.AreEqual(correctOpCount, opCount, "登録された操作の個数が想定されたものになっていません。操作対象となっているIModelの数だけ操作を生成してくください。");

                bindInstanceMap.Add(false, null, root.GetHierarchyEnumerable());
                correctOpCount = root.GetHierarchyEnumerable().Count();
                opCount = 0;
                while (enumerator.MoveNext() && enumerator.Current != null)
                {
                    opCount++;
                }
                Assert.AreEqual(correctOpCount, opCount, "再び操作が追加された後でも、ModelViewBinderInstanceMap#GetDoDelayOperationsEnumerator関数の返り値のIEnumeratorが使用できるようにしてください。");
            }
        }

        [Test, Description("Model階層の変更に合わせて、自動的にバインドを行う機能のテスト")]
        public void AutoBindOnChangedModelHierarchyPasses()
        {
            var allBinder = new ModelViewBinder("*", null, new ModelViewBinder.BindInfo(typeof(EmptyViewObjClass)));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(EmptyViewObjClass), new EmptyViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);
            Assert.IsFalse(bindInstanceMap.EnableAutoBind);

            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "apple" };
            var orange = new Model() { Name = "orange" };
            var grape = new Model() { Name = "grape" };

            {//自動バインドのルートモデルの設定のテスト
                //RootModelの設定
                // root <- set to RootModel
                bindInstanceMap.RootModel = root;
                Assert.IsTrue(bindInstanceMap.EnableAutoBind, "ModelViewBinderInstaneMap#RootModelを設定した時はEnableAutoBindはtrueになるようにしてください。");
                var errorMessage = "ModelViewBinderInstaneMap#RootModelを設定した時はバインドも行うようにしてください。";
                Assert.AreEqual(1, bindInstanceMap.BindInstances.Count, errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(root), errorMessage);

                //RootModelをnullにした時
                // root <- unset to RootModel
                bindInstanceMap.RootModel = null;
                Assert.IsFalse(bindInstanceMap.EnableAutoBind, "ModelViewBinderInstaneMap#RootModelをnullにした時はEnableAutoBindはfalseになるようにしてください。");
                errorMessage = "ModelViewBinderInstaneMap#RootModelをnullにした時は既存のバインドを全て削除してください。";
                Assert.AreEqual(0, bindInstanceMap.BindInstances.Count, errorMessage);

                //子を持つModelをRootModelにした時
                // root <- set to RootModel
                //   - apple <- Auto Add
                //     - orange <- Auto Add
                apple.AddChildren(orange);
                root.AddChildren(apple);
                bindInstanceMap.RootModel = root;
                Assert.IsTrue(bindInstanceMap.EnableAutoBind, "ModelViewBinderInstaneMap#RootModelを設定した時はEnableAutoBindはtrueになるようにしてください。");
                errorMessage = "子を持つModelをModelViewBinderInstaneMap#RootModelに設定した時はバインドも行うようにしてください。";
                Assert.AreEqual(3, bindInstanceMap.BindInstances.Count, errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(root), errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(apple), errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(orange), errorMessage);

                bindInstanceMap.RootModel = null;
                root.Parent = null;
                apple.Parent = null;
                orange.Parent = null;
                grape.Parent = null;
            }

            {//自動生成のテスト
                root.ClearChildren();
                apple.ClearChildren();
                orange.ClearChildren();
                grape.ClearChildren();

                // 子Modelの追加
                //   Appleをrootの子に追加
                // root(RootModel)
                //   - apple <- Add
                bindInstanceMap.RootModel = root;
                var count = bindInstanceMap.BindInstances.Count;
                var errorMessage = "子を追加した時は、それに対してのバインドを自動的に追加してください";

                root.AddChildren(apple);

                Assert.AreEqual(count + 1, bindInstanceMap.BindInstances.Count, errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(apple), errorMessage);

                // 親子構造を持つModelの追加
                //   grapeを子に持つorangeをrootの子に追加
                // root(RootModel)
                //   - apple
                //   - orange <- Add
                //     - grape <- Auto Add
                count = bindInstanceMap.BindInstances.Count;
                errorMessage = "親子構造を持つModelを追加した時は、その全てのModelに対してバインドを行うようにしてください";

                grape.Parent = orange;
                orange.Parent = root;

                Assert.AreEqual(count + 2, bindInstanceMap.BindInstances.Count, errorMessage);
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(orange), $"Model(orange)がバインドに追加されていません。{errorMessage}");
                Assert.IsTrue(bindInstanceMap.BindInstances.ContainsKey(grape), $"Model(grape)がバインドに追加されていません。{errorMessage}");
            }

            {//自動削除のテスト
                root.ClearChildren();
                apple.ClearChildren();
                orange.ClearChildren();
                grape.ClearChildren();

                // 子Modelの削除
                // BindInstanceMapからappleを削除する
                // root(RootModel)
                //   - apple <- Remove
                //   - orange
                //     - grape
                apple.Parent = root;
                orange.Parent = root;
                grape.Parent = orange;
                bindInstanceMap.RootModel = root;

                var count = bindInstanceMap.BindInstances.Count;
                var errorMessage = "子が削除された時は、それに関連するバインドも削除してください。";

                root.RemoveChildren(apple);

                Assert.AreEqual(count - 1, bindInstanceMap.BindInstances.Count, errorMessage);
                Assert.IsFalse(bindInstanceMap.BindInstances.ContainsKey(apple), errorMessage);

                //孫を持つModelの削除
                // BindInstanceMapからorangeを削除する
                // root(RootModel)
                //   - apple
                //   - orange <- Remove
                //     - grape <- Auto Remove
                apple.Parent = root;
                count = bindInstanceMap.BindInstances.Count;
                errorMessage = "孫を持つModelを削除した時はそのModel階層以下のModel全てのバインドも削除してください";
                orange.Parent = null;
                Assert.AreEqual(count - 2, bindInstanceMap.BindInstances.Count, $"想定した個数のModelが削除されていません。{errorMessage}");
                Assert.IsFalse(bindInstanceMap.BindInstances.ContainsKey(orange), $"Model(orange)のバインドが削除されていません。{errorMessage}");
                Assert.IsFalse(bindInstanceMap.BindInstances.ContainsKey(grape), $"Model(grape)のバインドが削除されていません。{errorMessage}");

                orange.Parent = root;//後のテストのために元に戻す
            }

            {//ルートModelに親を追加した時のテスト
                // ルートModelに親を追加
                // - rootParent <- Add
                //   - root(RootModel)
                //     - apple
                //     - orange
                //       - grape
                var count = bindInstanceMap.BindInstances.Count;
                var errorMessage = "ルートModelに親を設定した時はその親に対してはバインドを行いません。";
                var saveBindInstances = bindInstanceMap.BindInstances.ToArray();
                var rootParent = new Model() { Name = "rootParent" };
                root.Parent = rootParent;
                Assert.AreEqual(count, bindInstanceMap.BindInstances.Count, errorMessage);
                AssertionUtils.AssertEnumerable(saveBindInstances, bindInstanceMap.BindInstances, errorMessage);
            }
        }

        [Test, Description("ModelのName,LogicalID,StylingIDの変更に合わせて、自動的にバインドを行う機能のテスト")]
        public void AutoBindOnChangedModelIdentitiesPasses()
        {
            var nameBinderQueryPath = "name";
            var logicalBinderQueryPath = "#log";
            var styleBinderQueryPath = ".stl";

            var allBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(typeof(EmptyViewObjClass)));
            var nameBinder = new ModelViewBinder(nameBinderQueryPath, null
                , new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var logicalBinder = new ModelViewBinder(logicalBinderQueryPath, null
                , new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));
            var styleBinder = new ModelViewBinder(styleBinderQueryPath, null
                , new ModelViewBinder.BindInfo(typeof(IntViewObjClass)));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(EmptyViewObjClass), new EmptyViewObjClass.Binder()),
                (typeof(IntViewObjClass), new IntViewObjClass.Binder())
            );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder,
                nameBinder,
                logicalBinder,
                styleBinder);
            var bindInstanceMap = new ModelViewBinderInstanceMap(binderMap);

            var root = new ModelClass() { Name = "root" };
            var apple = new ModelClass() { Name = "apple", Parent = root };
            var orange = new ModelClass() { Name = "orange", Parent = root };
            var grape = new ModelClass() { Name = "grape", Parent = orange };

            bindInstanceMap.RootModel = root;

            {//Nameを切り替えた時のテスト
                // root.Name => "name"
                // - rootのBinder -> nameBinderに変更される
                root.Name = nameBinderQueryPath;
                Assert.AreSame(nameBinder, bindInstanceMap.BindInstances[root].Binder);

                // root.Name => "root"
                // - rootのBinder -> allBinderに変更される
                root.Name = "root";
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[root].Binder);

                // apple.Name => "name"
                // - appleのBinder -> nameBinderに変更される
                apple.Name = nameBinderQueryPath;
                Assert.AreSame(nameBinder, bindInstanceMap.BindInstances[apple].Binder, "ルートModelの階層内のModelのNameが変更された時も、バインドを切り替えるようにしてください");

                // apple.Name => "apple"
                // - appleのBinder -> allBinderに変更される
                apple.Name = "apple";
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[apple].Binder, "ルートModelの階層内のModelのNameが変更された時も、バインドを切り替えるようにしてください");

                // grape.Name => "name"
                // - grapeのBinder -> nameBinderに変更される
                grape.Name = nameBinderQueryPath;
                Assert.AreSame(nameBinder, bindInstanceMap.BindInstances[grape].Binder, "ルートModelの階層内のModelのNameが変更された時も、バインドを切り替えるようにしてください");

                // grape.Name => "root"
                // - grapeのBinder -> allBinderに変更される
                grape.Name = "grape";
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[grape].Binder, "ルートModelの階層内のModelのNameが変更された時も、バインドを切り替えるようにしてください");
            }

            {//LogicalIDを切り替えた時のテスト
                // root.LogicalID => "log"
                // - rootのBinder -> logicalBinderに変更される
                root.AddLogicalID(logicalBinderQueryPath);
                Assert.AreSame(logicalBinder, bindInstanceMap.BindInstances[root].Binder);

                // root.LogicalID => ""
                // - rootのBinder -> allBinderに変更される
                root.RemoveLogicalID(logicalBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[root].Binder);

                var errorMessage = "ルートModelの階層内のModelのLogicalIDが変更された時も、バインドを切り替えるようにしてください";
                // orange.LogicalID => "log"
                // - orangeのBinder -> logicalBinderに変更される
                orange.AddLogicalID(logicalBinderQueryPath);
                Assert.AreSame(logicalBinder, bindInstanceMap.BindInstances[orange].Binder, errorMessage);

                // orange.LogicalID => ""
                // - orangeのBinder -> allBinderに変更される
                orange.RemoveLogicalID(logicalBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[orange].Binder, errorMessage);

                // grape.LogicalID => "log"
                // - grapeのBinder -> logicalBinderに変更される
                grape.AddLogicalID(logicalBinderQueryPath);
                Assert.AreSame(logicalBinder, bindInstanceMap.BindInstances[grape].Binder, errorMessage);

                // grape.LogicalID => ""
                // - grapeのBinder -> allBinderに変更される
                grape.RemoveLogicalID(logicalBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[grape].Binder, errorMessage);
            }

            {//StylingIDを切り替えた時のテスト
                // root.StylingID => "stl"
                // - rootのBinder -> styleBinderに変更される
                root.AddStylingID(styleBinderQueryPath);
                Assert.AreSame(styleBinder, bindInstanceMap.BindInstances[root].Binder);

                // root.StylingID => ""
                // - rootのBinder -> allBinderに変更される
                root.RemoveStylingID(styleBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[root].Binder);

                var errorMessage = "ルートModelの階層内のModelのStylingIDが変更された時も、バインドを切り替えるようにしてください";
                // orange.StylingID => "log"
                // - orangeのBinder -> styleBinderに変更される
                orange.AddStylingID(styleBinderQueryPath);
                Assert.AreSame(styleBinder, bindInstanceMap.BindInstances[orange].Binder, errorMessage);

                // orange.StylingID => ""
                // - orangeのBinder -> allBinderに変更される
                orange.RemoveStylingID(styleBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[orange].Binder, errorMessage);

                // grape.StylingID => "log"
                // - grapeのBinder -> styleBinderに変更される
                grape.AddStylingID(styleBinderQueryPath);
                Assert.AreSame(styleBinder, bindInstanceMap.BindInstances[grape].Binder, errorMessage);

                // grape.StylingID => ""
                // - grapeのBinder -> allBinderに変更される
                grape.RemoveStylingID(styleBinderQueryPath);
                Assert.AreSame(allBinder, bindInstanceMap.BindInstances[grape].Binder, errorMessage);
            }

        }


        /// <summary>
        /// 何もしないViewオブジェクト
        /// </summary>
        class TestOnCreatedViewObjClass : EmptyViewObject
        {
            public ModelViewBinderInstanceMap UsedBinderInstanceMap { get; set; }

            protected override void OnBind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
                UsedBinderInstanceMap = binderInstanceMap;
            }

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        [Test, Description("IViewObject#OnCreatedのテスト")]
        public void IViewObjectOnCreatedPasses()
        {
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestOnCreatedViewObjClass)));

            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestOnCreatedViewObjClass), new TestOnCreatedViewObjClass.ParamBinder())
            );

            var errorMessageUseModel = $"ModeViewBinder#CreateViewObjectsに渡したModelがIViewObject#UseModelに設定されるようにしてください";
            var errorMessageUseBinderInstance = $"ModeViewBinder#CreateViewObjectsに渡したModelViewBinderInstanceがIViewObject#UseBinderInstanceに設定されるようにしてください";
            var errorMessageUsedBinderInstanceMap = $"ModeViewBinder#CreateViewObjectsに渡したModelViewBinderInstanceMapがIViewObject#OnCreatedに渡されるようにしてください";
            {//ModeViewBinder#CreateViewObjectsのテスト
                var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
                var binderInstanceMap = new ModelViewBinderInstanceMap(binderMap);
                var root = new Model() { Name = "root" };

                var viewObjs = allBinder.CreateViewObjects(root, null, binderInstanceMap);
                Assert.AreEqual(1, viewObjs.Count);

                var viewObj = viewObjs[0];
                Assert.AreSame(root, viewObj.UseModel, errorMessageUseModel);
                Assert.AreSame(null, viewObj.UseBinderInstance, errorMessageUseBinderInstance);

                var onCreatedViewObj = viewObjs[0] as TestOnCreatedViewObjClass;
                Assert.AreSame(binderInstanceMap, onCreatedViewObj.UsedBinderInstanceMap, errorMessageUsedBinderInstanceMap);
            }
            {
                //ModelViewBinderInstanceMapを指定しなかった時のテスト
                var root = new Model() { Name = "root" };
                var viewObjs = allBinder.CreateViewObjects(root, null, null);
                Assert.AreEqual(1, viewObjs.Count);

                var viewObj = viewObjs[0];
                Assert.AreSame(root, viewObj.UseModel, errorMessageUseModel);

                var onCreatedViewObj = viewObj as TestOnCreatedViewObjClass;
                Assert.AreSame(null, onCreatedViewObj.UsedBinderInstanceMap, errorMessageUsedBinderInstanceMap);
            }

            {//ModelViewBinderInstanceMap#CreateBindInstanceのテスト
                var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
                var binderInstanceMap = new ModelViewBinderInstanceMap(binderMap);
                var root = new Model() { Name = "root" };

                var bindInstance = binderMap.CreateBindInstance(root, binderInstanceMap);

                Assert.AreEqual(1, bindInstance.ViewObjects.Count());

                var viewObj = bindInstance.ViewObjects.First();
                Assert.AreSame(root, viewObj.UseModel, errorMessageUseModel);
                Assert.AreSame(bindInstance, viewObj.UseBinderInstance, errorMessageUseBinderInstance);

                var onCreatedViewObj = viewObj as TestOnCreatedViewObjClass;
                Assert.AreSame(binderInstanceMap, onCreatedViewObj.UsedBinderInstanceMap, errorMessageUsedBinderInstanceMap);
            }

            {//ModelViewBinderInstanceMap#Addのテスト
                var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder);
                var binderInstanceMap = new ModelViewBinderInstanceMap(binderMap);
                var root = new Model() { Name = "root" };

                binderInstanceMap.Add(root);
                var bindInstance = binderInstanceMap.BindInstances[root];
                Assert.AreEqual(1, bindInstance.ViewObjects.Count());

                var viewObj = bindInstance.ViewObjects.First();
                Assert.AreSame(root, viewObj.UseModel, errorMessageUseModel);

                var onCreatedViewObj = viewObj as TestOnCreatedViewObjClass;
                Assert.AreSame(binderInstanceMap, onCreatedViewObj.UsedBinderInstanceMap, errorMessageUsedBinderInstanceMap);
            }
        }

        [Test]
        public void OnAddedCallbackPasses()
        {
            var allBinder = new ModelViewBinder("*", null,
                new ModelViewBinder.BindInfo(typeof(TestOnCreatedViewObjClass)));
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestOnCreatedViewObjClass), new TestOnCreatedViewObjClass.ParamBinder())
            );

            {//ModeViewBinder#CreateViewObjectsのテスト
                var counter = 0;
                List<ModelViewBinderInstance> onAddedBinderInstanceList = new List<ModelViewBinderInstance>();
                ModelViewBinderMap.OnAddedCallback onAddedCallback = (binderInstance) =>
                {
                    counter++;
                    onAddedBinderInstanceList.Add(binderInstance);
                };
                var binderMap = new ModelViewBinderMap(viewInstanceCreator, allBinder)
                {
                    DefaultOnAddedCallback = onAddedCallback
                };

                Assert.AreSame(onAddedCallback, binderMap.DefaultOnAddedCallback);

                var binderInstanceMap = new ModelViewBinderInstanceMap(binderMap);
                var root = new Model() { Name = "root" };

                {//
                    counter = 0;
                    onAddedBinderInstanceList.Clear();
                    binderInstanceMap.RootModel = root;
                    Assert.AreEqual(1, counter);
                    AssertionUtils.AssertEnumerableByUnordered(
                        new ModelViewBinderInstance[] { binderInstanceMap.BindInstances[root] }
                        , onAddedBinderInstanceList
                        , "");
                }

                {//親子階層を持つModelを追加した時のテスト
                    var model = new Model() { Name = "model1" };
                    var child = new Model() { Name = "child1" };
                    child.Parent = model;

                    counter = 0;
                    onAddedBinderInstanceList.Clear();
                    binderInstanceMap.Add(model, false, null);

                    Assert.AreEqual(2, counter);
                    AssertionUtils.AssertEnumerableByUnordered(
                        new ModelViewBinderInstance[] {
                            binderInstanceMap.BindInstances[model],
                            binderInstanceMap.BindInstances[child],
                        }
                        , onAddedBinderInstanceList
                        , "");
                }

                {//Default意外にもカールバックを設定した時のテスト.
                    var model = new Model() { Name = "model1" };

                    counter = 0;
                    onAddedBinderInstanceList.Clear();

                    var tmpCounter = 0;
                    binderInstanceMap.Add(model, false, (binderInstance) => {
                        tmpCounter += 100;
                        onAddedBinderInstanceList.Add(binderInstance);
                    });

                    Assert.AreEqual(1, counter);
                    Assert.AreEqual(100, tmpCounter);
                    AssertionUtils.AssertEnumerableByUnordered(
                        new ModelViewBinderInstance[] {
                            binderInstanceMap.BindInstances[model],
                            binderInstanceMap.BindInstances[model],
                        }
                        , onAddedBinderInstanceList
                        , "引数に渡したコールバックとDefaultのコールバック両方とも呼び出すようにしてください");
                }

            }
        }

        [Test]
        public void ContainsPasses()
        {
            var binderMap = new ModelViewBinderMap(new DefaultViewInstanceCreator(),
                new ModelViewBinder("*", null)
                );

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new Model();
            Assert.IsFalse(binderInstanceMap.Contains(model));
            binderInstanceMap.Add(model);
            Assert.IsTrue(binderInstanceMap.Contains(model));
        }

        [Test]
        public void ContainsAtDelayPasses()
        {
            var binderMap = new ModelViewBinderMap(new DefaultViewInstanceCreator(),
                new ModelViewBinder("*", null)
                );

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.EnabledDelayOperation = true;

            var model = new Model();
            Assert.IsFalse(binderInstanceMap.Contains(model));
            binderInstanceMap.Add(model);
            Assert.IsFalse(binderInstanceMap.Contains(model));

            binderInstanceMap.DoDelayOperations();
            Assert.IsTrue(binderInstanceMap.Contains(model));

        }

    }
}
