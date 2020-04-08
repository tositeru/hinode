using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="Model"/>
    /// </summary>
    public class TestModel : TestBase
    {
        [Test, Description("LogicalIDの追加、削除のテスト")]
        public void LogicalIDPasses()
        {
            var model = new Model();
            Assert.IsNotNull(model.LogicalID);
            Assert.IsTrue(!model.LogicalID.Any());

            var list = new string[] { "a", "b", "c" };
            model.AddLogicalID(list);
            AssertIDList(list, model.LogicalID, "IDの追加に失敗しています。");

            model.AddLogicalID("a", "b");
            AssertIDList(list, model.LogicalID, "IDは同名のものを複数持つことができません。");


            model.RemoveLogicalID("b", "c");
            AssertIDList(new string[] { "a" }, model.LogicalID, "IDの削除に失敗しています。");

            model.LogicalID = new HashSet<string>()
            {
                "A", "B",
            };
            AssertIDList(new string[] { "A", "B" }, model.LogicalID, "LogicalIDを直接設定した時の処理が想定したものになっていません。");
        }

        [Test, Description("StyleIDの追加、削除のテスト")]
        public void StyleIDPasses()
        {
            var model = new Model();
            Assert.IsNotNull(model.StylingID);
            Assert.IsTrue(!model.StylingID.Any());

            var list = new string[] { "a", "b", "c" };
            model.AddStylingID(list);
            AssertIDList(list, model.StylingID, "IDの追加に失敗しています。");

            model.AddStylingID("a", "b");
            AssertIDList(list, model.StylingID, "IDは同名のものを複数持つことができません。");


            model.RemoveStylingID("b", "c");
            AssertIDList(new string[] { "a" }, model.StylingID, "IDの削除に失敗しています。");

            model.StylingID = new HashSet<string>()
            {
                "A", "B",
            };
            AssertIDList(new string[] { "A", "B" }, model.StylingID, "StylingIDを直接設定した時の処理が想定したものになっていません。");
        }

        void AssertIDList(IEnumerable<string> corrects, IReadOnlyCollection<string> idList, string message)
        {
            Assert.AreEqual(corrects.Count(), idList.Count, $"Don't equal ID count... {message}");
            foreach(var cor in corrects)
            {
                Assert.IsTrue(idList.Contains(cor), $"Don't found ID({cor}) in IDList... {message}");
            }
        }

        [Test, Description("Name,LogicalID,StyleIDが変更された時のコールバック呼び出しのテスト")]
        public void OnChangedIdentityCallbackPasses()
        {
            var model = new Model();

            //Callbackの設定
            int callbackCounter = 0;
            int callbackCounter2 = 0;
            OnChangedModelIdentitiesCallback incrementCounter = (m) => { callbackCounter++; };
            OnChangedModelIdentitiesCallback incrementCounter2 = (m) => { callbackCounter2++; };
            model.OnChangedModelIdentities.Add(incrementCounter, incrementCounter2);

            {
                callbackCounter = 0;
                callbackCounter2 = 0;
                model.Name = "Apple";
                Assert.AreEqual(1, callbackCounter);
                Assert.AreEqual(1, callbackCounter2);
                model.AddLogicalID("lg");
                Assert.AreEqual(2, callbackCounter);
                Assert.AreEqual(2, callbackCounter2);
                model.AddStylingID("st");
                Assert.AreEqual(3, callbackCounter);
                Assert.AreEqual(3, callbackCounter2);
            }

            {
                callbackCounter = 0;
                callbackCounter2 = 0;
                model.RemoveLogicalID("lg");
                Assert.AreEqual(1, callbackCounter);
                Assert.AreEqual(1, callbackCounter2);
                model.RemoveStylingID("st");
                Assert.AreEqual(2, callbackCounter);
                Assert.AreEqual(2, callbackCounter2);
            }

            {//値を設定するが、変更はされないケース
                callbackCounter = 0;
                callbackCounter2 = 0;
                model.Name = model.Name;
                Assert.AreEqual(0, callbackCounter, "Nameの値が変更された時だけコールバックが呼び出されるようにしてください");
                Assert.AreEqual(0, callbackCounter2);

                model.RemoveLogicalID("lg");
                Assert.AreEqual(0, callbackCounter, "LogicalIDの内容が変更された時だけコールバックが呼び出されるようにしてください");
                Assert.AreEqual(0, callbackCounter2);
                model.RemoveStylingID("st");
                Assert.AreEqual(0, callbackCounter, "StyleIDの内容が変更された時だけコールバックが呼び出されるようにしてください");
                Assert.AreEqual(0, callbackCounter2);

                //何も追加されないケースのテスト
                model.AddLogicalID("lg");
                model.AddStylingID("st");

                callbackCounter = 0;
                callbackCounter2 = 0;
                model.AddLogicalID("lg");
                Assert.AreEqual(0, callbackCounter, "LogicalIDの内容が変更された時だけコールバックが呼び出されるようにしてください");
                Assert.AreEqual(0, callbackCounter2);
                model.AddStylingID("st");
                Assert.AreEqual(0, callbackCounter, "LogicalIDの内容が変更された時だけコールバックが呼び出されるようにしてください");
                Assert.AreEqual(0, callbackCounter2);
            }

            {
                callbackCounter = 0;
                callbackCounter2 = 0;
                model.OnChangedModelIdentities.Remove(incrementCounter);
                model.Name = "Orange";
                Assert.AreEqual(0, callbackCounter, "コールバックの削除に失敗しています");
                Assert.AreEqual(1, callbackCounter2, "指定したコールバック以外も削除しています");
            }
            {//設定しているコールバック全てを削除する
                callbackCounter = 0;
                callbackCounter2 = 0;
                model.OnChangedModelIdentities.Add(incrementCounter);
                model.OnChangedModelIdentities.Clear();
                model.Name += model.Name;
                Assert.AreEqual(0, callbackCounter, "コールバックのClearに失敗しています");
                Assert.AreEqual(0, callbackCounter2, "コールバックのClearに失敗しています");
            }
        }

        [Test, Description("Name,LogicalID,StyleIDが変更された時にParentに設定されたコールバックが呼び出されるかテスト")]
        public void OnChangedIdentityCallbackInParentPasses()
        {
            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "apple" };
            var orange = new Model() { Name = "orange" };

            int callbackCounter = 0;
            OnChangedModelIdentitiesCallback incrementCounter = (m) => { callbackCounter++; };
            root.OnChangedModelIdentities.Add(incrementCounter);
            apple.Parent = root;
            orange.Parent = apple;

            {//子ModelのIdentityを変更した時のテスト
                callbackCounter = 0;
                apple.Name = "APPLE";
                Assert.AreEqual(1, callbackCounter, "子ModelのNameが変更された時にその親のコールバックも呼び出されるようにしてください。");
                apple.AddLogicalID("APPLE");
                Assert.AreEqual(2, callbackCounter, "子ModelのLogicalが変更された時にその親のコールバックも呼び出されるようにしてください。");
                apple.RemoveLogicalID("APPLE");
                Assert.AreEqual(3, callbackCounter, "子ModelのLogicalが変更された時にその親のコールバックも呼び出されるようにしてください。");
                apple.AddStylingID("APPLE");
                Assert.AreEqual(4, callbackCounter, "子ModelのStylingが変更された時にその親のコールバックも呼び出されるようにしてください。");
                apple.RemoveStylingID("APPLE");
                Assert.AreEqual(5, callbackCounter, "子ModelのStylingが変更された時にその親のコールバックも呼び出されるようにしてください。");

                callbackCounter = 0;
                orange.Name = "ORANGE";
                Assert.AreEqual(1, callbackCounter, "孫ModelのIdentityが変更された時にその親のコールバックも呼び出されるようにしてください。");
            }
        }

        class OnChangedHierarchyTestModel : Model
        {
            ChangedModelHierarchyType _changedType;
            Model _changedTarget;
            Model[] _changedModels;

            public OnChangedHierarchyTestModel()
            {
                OnChangedHierarchy.Add(ChangedHierarchy);
            }

            public void AssertCallback(ChangedModelHierarchyType type, Model target, IEnumerable<Model> models, string message)
            {
                Assert.AreEqual(type, _changedType, $"想定されたタイプになっていません... {message}");
                Assert.AreSame(target, _changedTarget, $"想定されたTargetが設定されていません... {message} correct={this?.GetPath() ?? "(null)"}, got={_changedTarget?.GetPath() ?? "(null)"}");
                //string changedModelsLog = "";
                //if(_changedModels != null) foreach(var c in _changedModels) { changedModelsLog += (c?.GetPath() ?? "(null)") + ","; }
                //string modelsLog = "";
                //if(models != null) foreach (var c in models) { modelsLog += (c?.GetPath() ?? "(null)") + ","; }
                //Debug.Log($"correct=>{modelsLog}{System.Environment.NewLine}    gots=>{changedModelsLog}");
                AssertionUtils.AssertEnumerable(_changedModels, models, $"想定されたModelsが設定されていません... {message}");
            }

            public void Reset()
            {
                _changedType = (ChangedModelHierarchyType)(-1);
                _changedTarget = null;
                _changedModels = null;
            }

            void ChangedHierarchy(ChangedModelHierarchyType type, Model target, IEnumerable<Model> models)
            {
                _changedType = type;
                _changedTarget = target;
                _changedModels = models.ToArray();
            }
        }

        [Test, Description("Children,Parentが変更された時のイベントのテスト")]
        public void OnChangedHierarchyPasses()
        {
            //テストのモデル階層
            // root
            //   - apple
            //   - orange
            //     - apple

            // Test. 子Modelが追加
            // root
            //   - apple <- Add
            //
            // 以下のイベントの発生を想定
            //  - rootにてChangedModelHierarchyType.ChildAdd appleでイベント発生
            //  - appleにてParentSet rootでイベント発生
            var root = new OnChangedHierarchyTestModel() { Name = "root" };
            var apple = new OnChangedHierarchyTestModel() { Name = "apple" };

            apple.Parent = root;

            root.AssertCallback(ChangedModelHierarchyType.ChildAdd, root, new Model[] { apple }, "子Modelが追加");
            apple.AssertCallback(ChangedModelHierarchyType.ParentChange, apple, new Model[] { null, root }, "子Modelが追加");

            // Test. 他の子Modelがある時に子Modelが追加
            // root
            //   - apple
            //   - orange <- Add
            // 以下のイベントの発生を想定
            //  - rootにてChangedModelHierarchyType.ChildAdd orangeでイベント発生
            //  - orangeにてParentSet rootでイベント発生
            root.Reset();
            apple.Reset();

            var orange = new OnChangedHierarchyTestModel() { Name = "orange" };

            orange.Parent = root;

            root.AssertCallback(ChangedModelHierarchyType.ChildAdd, root, new Model[]{ orange }, "他の子Modelがある時に子Modelが追加");
            apple.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "他の子Modelがある時に子Modelが追加");
            orange.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { null, root }, "他の子Modelがある時に子Modelが追加");

            // Test. 孫Modelが追加
            // root
            //   - apple
            //   - orange
            //     - grape <- Add
            // 以下のイベントの発生を想定
            //  - rootにてChildAdd grapeでイベント発生
            //  - orangeにてChildAdd grapeでイベント発生
            //  - grapeにてParentSet orangeでイベント発生
            root.Reset();
            apple.Reset();
            orange.Reset();

            var grape = new OnChangedHierarchyTestModel() { Name = "apple" };
            grape.Parent = orange;

            root.AssertCallback(ChangedModelHierarchyType.ChildAdd, orange, new Model[] { grape }, "孫Modelが追加");
            apple.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "孫Modelが追加");
            orange.AssertCallback(ChangedModelHierarchyType.ChildAdd, orange, new Model[] { grape }, "孫Modelが追加");
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, grape, new Model[] { null, orange }, "孫Modelが追加");

            // Test. 子Modelが削除
            // root
            //   - apple <- Remove
            //   - orange
            //     - grape
            // 以下のイベントの発生を想定
            //  - rootにてChildRemove appleでイベント発生
            //  - appleにてParentRemove rootでイベント発生
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            apple.Parent = null;

            root.AssertCallback(ChangedModelHierarchyType.ChildRemove, root, new Model[] { apple }, "子Modelが削除");
            apple.AssertCallback(ChangedModelHierarchyType.ParentChange, apple, new Model[] { root, null }, "子Modelが削除");
            orange.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "子Modelが削除");
            grape.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "子Modelが削除");

            // Test. 孫Modelが削除
            // root
            //   - apple
            //   - orange
            //     - grape <- Remove
            // 以下のイベントの発生を想定
            //  - rootにてChildRemove grapeでイベント発生
            //  - orangeにてChildRemove grapeでイベント発生
            //  - grapeにてParentRemove orangeでイベント発生
            apple.Parent = root;
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            grape.Parent = null;

            root.AssertCallback(ChangedModelHierarchyType.ChildRemove, orange, new Model[] { grape }, "孫Modelが削除");
            apple.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "孫Modelが削除");
            orange.AssertCallback(ChangedModelHierarchyType.ChildRemove, orange, new Model[] { grape }, "孫Modelが削除");
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, grape, new Model[] { orange, null }, "孫Modelが削除");

            // Test. 孫を持つ子Modelが削除
            // root
            //   - apple
            //   - orange <- Remove
            //     - grape
            // 以下のイベントの発生を想定
            //  - rootにてChildRemove orangeでイベント発生
            //  - orangeにてParentRemove rootでイベント発生
            //  - grapeにてParentRemove rootでイベント発生
            grape.Parent = orange;
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            orange.Parent = null;

            root.AssertCallback(ChangedModelHierarchyType.ChildRemove, root, new Model[] { orange }, "孫を持つ子Modelが削除");
            apple.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "孫を持つ子Modelが削除");
            orange.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, null }, "孫を持つ子Modelが削除"); ; ;
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, null }, "孫を持つ子Modelが削除");

            // Test. 孫を持つ子Modelが別の親に移動
            // root
            //   - apple
            //   - orange <- Move to root2
            //     - grape
            // root2
            //   - orange <- Move from root
            // 以下のイベントの発生を想定
            //  - rootにてChildRemove orangeでイベント発生
            //  - orangeにてParentChange rootでイベント発生
            //  - grapeにてParentChange rootでイベント発生
            //  - root2にてChildAdd orangeでイベント発生
            orange.Parent = root;
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            var root2 = new OnChangedHierarchyTestModel() { Name = "root2" };
            orange.Parent = root2;

            root.AssertCallback(ChangedModelHierarchyType.ChildRemove, root, new Model[] { orange }, "孫を持つ子Modelが別の親に移動");
            apple.AssertCallback((ChangedModelHierarchyType)(-1), null, null, "孫を持つ子Modelが別の親に移動");
            orange.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, root2 }, "孫を持つ子Modelが別の親に移動");
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, root2 }, "孫を持つ子Modelが別の親に移動");
            root2.AssertCallback(ChangedModelHierarchyType.ChildAdd, root2, new Model[] { orange }, "孫を持つ子Modelが別の親に移動");

            //Test. 複数の子を同時追加
            // root
            //   - apple <- Add
            //   - orange <- Add
            //     - grape
            // 以下のイベントの発生を想定
            //  - rootにてChildAdd apple,orangeでイベント発生
            //  - appleにてParentChange null,rootでイベント発生
            //  - orangeにてParentChange null,rootでイベント発生
            //  - grapeにてParentChange null,rootでイベント発生
            apple.Parent = null;
            orange.Parent = null;
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            root.AddChildren(apple, orange);

            root.AssertCallback(ChangedModelHierarchyType.ChildAdd, root, new Model[] { apple, orange }, "複数の子Modelを同時追加");
            apple.AssertCallback(ChangedModelHierarchyType.ParentChange, apple, new Model[] { null, root }, "複数の子Modelを同時追加");
            orange.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { null, root }, "複数の子Modelを同時追加");
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { null, root }, "複数の子Modelを同時追加");

            //Test. 複数の子を同時削除
            // root
            //   - apple <- Remove
            //   - orange <- Remove
            //     - grape
            // 以下のイベントの発生を想定
            //  - rootにてChildRemove apple,orangeでイベント発生
            //  - appleにてParentChange root,nullでイベント発生
            //  - orangeにてParentChange root,nullでイベント発生
            //  - grapeにてParentChange root,nullでイベント発生
            apple.Parent = root;
            orange.Parent = root;
            root.Reset();
            apple.Reset();
            orange.Reset();
            grape.Reset();

            root.RemoveChildren(apple, orange);

            root.AssertCallback(ChangedModelHierarchyType.ChildRemove, root, new Model[] { apple, orange }, "複数の子Modelを同時削除");
            apple.AssertCallback(ChangedModelHierarchyType.ParentChange, apple, new Model[] { root, null }, "複数の子Modelを同時削除");
            orange.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, null }, "複数の子Modelを同時削除");
            grape.AssertCallback(ChangedModelHierarchyType.ParentChange, orange, new Model[] { root, null }, "複数の子Modelを同時削除");
        }

        [Test]
        public void PathPasses()
        {
            var root = new Model() { Name = "root" };
            Assert.AreEqual("root", root.GetPath());

            var rootApple = new Model() { Name = "apple", Parent = root };
            Assert.AreEqual("root/apple", rootApple.GetPath());

            var rootOrange = new Model() { Name = "orange", Parent = root };
            Assert.AreEqual("root/orange", rootOrange.GetPath());

            var rootOrangeApple = new Model() { Name = "apple", Parent = rootOrange };
            Assert.AreEqual("root/orange/apple", rootOrangeApple.GetPath());
        }

        [Test]
        public void GetRootPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple",
                        Children = new List<Model>()
                        {
                            new Model() { Name = "orange",
                            },
                        },
                    },
                },
            };

            Assert.AreSame(root, root.GetRoot());
            Assert.AreSame(root, root.Children.First().GetRoot());
            Assert.AreSame(root, root.Children.First().Children.First().GetRoot());
        }

        [Test]
        public void GetTraversedRootEnumerablePasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple",
                        Children = new List<Model>()
                        {
                            new Model() { Name = "orange",
                            },
                        },
                    },
                },
            };

            var apple = root.Children.First();
            var orange = apple.Children.First();
            var correctOrderList = new Model[]
            {
                orange,
                apple,
                root,
            };
            Assert.AreEqual(correctOrderList.Count(), orange.GetTraversedRootEnumerable().Count());
            foreach (var (correct, got) in orange.GetTraversedRootEnumerable()
                .Zip(correctOrderList, (g, c) => (correct: c, got: g)))
            {
                Assert.AreSame(correct, got);
            }
        }

        [Test]
        public void AddAndRemoveChildrenPasses()
        {
            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "apple" };
            var grape = new Model() { Name = "grape" };

            root.AddChildren(apple, grape, null);
            AssertionUtils.AssertEnumerable(root.Children, new Model[] { apple, grape }, "nullを追加しようとした時は、それを除外してください。");
            Assert.AreSame(root, apple.Parent);
            Assert.AreSame(root, grape.Parent);

            root.RemoveChildren(apple, grape, null);
            AssertionUtils.AssertEnumerable(root.Children, new Model[] { }, "nullを削除しようとした時は、それを除外してください。");
            Assert.AreSame(null, apple.Parent);
            Assert.AreSame(null, grape.Parent);
        }

        [Test]
        public void ClearChildrenPasses()
        {
            var root = new Model() { Name = "root" };
            var apple = new Model() { Name = "apple" };
            var grape = new Model() { Name = "grape" };

            root.AddChildren(apple, grape, null);

            root.ClearChildren();
            AssertionUtils.AssertEnumerable(root.Children, new Model[] { }, "");
            Assert.AreSame(null, apple.Parent);
            Assert.AreSame(null, grape.Parent);
        }

        [Test]
        public void GetChildPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange", },
                    new Model() { Name = "grape", },
                },
            };

            Assert.AreEqual(4, root.ChildCount);
            for (var i = 0; i < root.ChildCount; ++i)
            {
                Assert.AreSame(root.Children.ElementAt(i), root.GetChild(i), $"invalid GetChild({i})...");
            }

            Assert.Throws<System.ArgumentOutOfRangeException>(() => root.GetChild(0).GetChild(0));
        }

        [Test]
        public void GetSiblingIndexPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange", },
                    new Model() { Name = "grape", },
                },
            };

            foreach (var (child, index) in root.Children
                .Zip(Enumerable.Range(0, root.ChildCount), (c, i) => (child: c, index: i)))
            {
                Assert.AreEqual(index, child.GetSiblingIndex());
            }

            Assert.AreEqual(-1, root.GetSiblingIndex());
        }

        [Test]
        public void GetHierarchyEnumerablePasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange",
                        Children = new List<Model>()
                        {
                            new Model() { Name = "apple" },
                            new Model() { Name = "root",
                                Children = new List<Model>()
                                {
                                    new Model() { Name = "apple" },
                                }
                            },
                        },
                    },
                },
            };

            var correctModelPathList = new List<string>
            {
                "root",
                "root/apple",
                "root/apple",
                "root/orange",
                "root/orange/apple",
                "root/orange/root",
                "root/orange/root/apple",
            };
            AssertQueryResults(correctModelPathList, root.GetHierarchyEnumerable(), "Failed to Traverse Model.GetHierarchyEnumerable()...");
        }

        [Test, Description("Model#DoMatchQueryのテスト")]
        public void DoMatchQueryPasses()
        {
            var root = new Model() { Name = "root", LogicalID = new ModelIDList("lg1"), StylingID = new ModelIDList("sl1") };
            string query;
            query = "root"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = "#lg1"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = ".sl1"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = "root #lg1"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = ".sl1 #lg1"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = "#lg1 root .sl1"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
            query = "*"; Assert.IsTrue(root.DoMatchQuery(query), $"Miss match Query({query})...");
        }

        [Test, Description("Model#QueryChildrenのテスト")]
        public void QueryChildrenPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1") },
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1"), StylingID = new ModelIDList("style1") },
                    new Model() { Name = "orange", LogicalID = new ModelIDList("logical2"), },
                    new Model() { Name = "grape", StylingID = new ModelIDList("style1")},
                    new Model() { Name = "banana", LogicalID = new ModelIDList("logical2"), StylingID = new ModelIDList("style1")},
                },
            };

            {//apple
                var corrects = new List<string>
                {
                    "root/apple", "root/apple"
                };
                AssertQueryResults(corrects, root.QueryChildren("apple"), $"Failed to query(apple)...");
            }
            {//#logical1
                var corrects = new List<string>
                {
                    "root/apple", "root/apple"
                };
                AssertQueryResults(corrects, root.QueryChildren("#logical1"), $"Failed to query(#logical1)...");
            }
            {//.style1
                var corrects = new List<string>
                {
                    "root/apple", "root/grape", "root/banana",
                };
                AssertQueryResults(corrects, root.QueryChildren(".style1"), $"Failed to query(.style1)...");
            }
            {//apple .style1
                var corrects = new List<string>
                {
                    "root/apple"
                };
                AssertQueryResults(corrects, root.QueryChildren("apple .style1"), $"Failed to query(apple .style1)...");
            }
            {//#logical1 .style1
                var corrects = new List<string>
                {
                    "root/apple"
                };
                AssertQueryResults(corrects, root.QueryChildren("#logical1 .style1"), $"Failed to query(#logical1 .style1)...");
            }
            {//#logical2 .style1
                var corrects = new List<string>
                {
                    "root/banana"
                };
                AssertQueryResults(corrects, root.QueryChildren("#logical2 .style1"), $"Failed to query(#logical2 .style1)...");
            }

            {//*
                var corrects = root.Children.Select(_c => _c.GetPath()).ToList();
                AssertQueryResults(corrects, root.QueryChildren("*"), $"Failed to query(*)...");
            }
        }

        [Test, Description("Model#DoMatchQueryPathのテスト")]
        public void DoMatchQueryPathPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1") },
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1"), StylingID = new ModelIDList("style1") },
                    new Model() { Name = "orange", LogicalID = new ModelIDList("logical2"),
                        Children = new List<Model>(){
                            new Model() { Name = "apple", LogicalID = new ModelIDList("logical1") },
                            new Model() { Name = "root", StylingID = new ModelIDList("style1"),
                                Children = new List<Model>(){
                                    new Model() { Name = "apple" },
                                },
                            },
                        },
                    },
                    new Model() { Name = "grape", StylingID = new ModelIDList("style1")},
                    new Model() { Name = "banana", LogicalID = new ModelIDList("logical2"), StylingID = new ModelIDList("style1")},
                },
            };

            var apple1 = root.Children.First();
            Assert.IsTrue(apple1.DoMatchQueryPath("apple"), "Don't Match queryPath(apple)...");
            Assert.IsTrue(apple1.DoMatchQueryPath("root/apple"), "Don't Match queryPath(root/apple)...");
            Assert.IsTrue(apple1.DoMatchQueryPath("#logical1"), "Don't Match queryPath(#logical1)...");
            Assert.IsTrue(apple1.DoMatchQueryPath("apple #logical1"), "Don't Match queryPath(apple #logical1)...");
            Assert.IsTrue(apple1.DoMatchQueryPath("root/apple #logical1"), "Don't Match queryPath(root/apple #logical1)...");
            Assert.IsTrue(apple1.DoMatchQueryPath("/root/apple"), "Don't Match queryPath(/root/apple)...");

            Assert.IsFalse(apple1.DoMatchQueryPath("orange"), "Miss to Match queryPath(orange)...");
            Assert.IsFalse(apple1.DoMatchQueryPath("apple .style1"), "Miss to Match queryPath(apple .style1)...");
            Assert.IsFalse(apple1.DoMatchQueryPath("root/apple .style1"), "Miss to Match queryPath(root/apple .style1)...");
            Assert.IsFalse(apple1.DoMatchQueryPath("~root/apple"), "Miss to Match queryPath(~root/apple)...");

            Assert.IsFalse(root.DoMatchQueryPath("apple"), "Miss to Match queryPath(apple) at root...");
        }

        [Test, Description("Model#GetQueryPathPriorityのテスト(Model#DoMatchQueryPathも含んでいます)")]
        public void QueryPathPriorityPasses()
        {
            var root = new Model()
            {
                Name = "root",
                LogicalID = new ModelIDList("logical1"),
                StylingID = new ModelIDList("style1"),
                Children = new List<Model>()
                {
                    new Model() { Name = "apple",
                        LogicalID = new ModelIDList("logical1", "logical2"),
                        StylingID = new ModelIDList("style1", "style2") },
                },
            };

            var apple = root.Query("apple").First();
            var noneParentQueryPathList = new (string path, bool isSamePrev)[] {
                //親パスの指定無しのクエリパス(優先順位が高い順)
                ("apple #logical1 #logical2 .style1 .style2", false),
                ("apple #logical1 #logical2 .style1", false),
                ("apple #logical1 #logical2 .style2", true),
                ("apple #logical1 #logical2", false),
                ("apple #logical1 .style1 .style2", false),
                ("apple #logical2 .style1 .style2", true),
                ("apple #logical1 .style1", false),
                ("apple #logical1 .style2", true),
                ("apple #logical2 .style1", true),
                ("apple #logical2 .style2", true),
                ("apple #logical1", false),
                ("apple #logical2", true),
                ("apple .style1 .style2", false),
                ("apple .style1", false),
                ("apple .style2", true),
                ("apple", false),
                ("#logical1 #logical2 .style1 .style2", false),
                ("* #logical1 #logical2 .style1 .style2", true),
                ("#logical1 #logical2 .style1", false),
                ("#logical1 #logical2 .style2", true),
                ("* #logical1 #logical2 .style1", true),
                ("* #logical1 #logical2 .style2", true),
                ("#logical1 #logical2", false),
                ("* #logical1 #logical2", true),
                ("#logical1 .style1 .style2", false),
                ("#logical2 .style1 .style2", true),
                ("* #logical1 .style1 .style2", true),
                ("* #logical2 .style1 .style2", true),
                ("#logical1 .style1", false),
                ("#logical1 .style2", true),
                ("#logical2 .style1", true),
                ("#logical2 .style2", true),
                ("* #logical1 .style1", true),
                ("* #logical1 .style2", true),
                ("* #logical2 .style1", true),
                ("* #logical2 .style2", true),
                ("#logical1", false),
                ("#logical2", true),
                ("* #logical1", true),
                ("* #logical2", true),
                (".style1 .style2", false),
                ("* .style1 .style2", true),
                (".style1", false),
                (".style2", true),
                ("* .style1", true),
                ("* .style2", true),
                ("*", false),
            };

            System.Func<string, (string path, bool isSamePrev)[], (string path, bool isSamePrev)[]> appendParentPath= (string parentPath, (string path, bool isSamePrev)[] seedQueries) => {
                return seedQueries
                .Select(_seed => (path: $"{parentPath}/{_seed.path}", isSamePrev: _seed.isSamePrev))
                .ToArray();
            };

            var queryPathes = appendParentPath("root #logical1 .style1", noneParentQueryPathList)
                .Concat(appendParentPath("root #logical1", noneParentQueryPathList))
                .Concat(appendParentPath("root .style1", noneParentQueryPathList))
                .Concat(appendParentPath("root", noneParentQueryPathList))
                .Concat(appendParentPath("*", noneParentQueryPathList))
                .Concat(noneParentQueryPathList);

            //priorityの辞書を作成する
            var applePriorityDict = new Dictionary<string, ModelViewQueryPathPriority>();
            foreach (var (queryPath, _) in queryPathes)
            {
                var priority = apple.GetQueryPathPriority(queryPath);
                Assert.IsFalse(priority.IsEmpty, $"Don't match quertPath... queryPath={queryPath}");
                applePriorityDict.Add(queryPath, priority);
            }

            //テスト実行
            var prevPriority = applePriorityDict[queryPathes.First().path];
            Assert.IsFalse(prevPriority.IsEmpty);
            foreach(var (queryPath, isSamePrev) in queryPathes.Skip(1))
            {
                var priority = applePriorityDict[queryPath];
                if(isSamePrev)
                {
                    Assert.AreEqual(prevPriority, priority, $"Not be same prev priority... queryPath={queryPath}, prevPath={prevPriority.QueryPath}");
                }
                else
                {
                    Assert.Less(priority, prevPriority, $"Current priority don't less than prev priority... queryPath={queryPath}, prevPath={prevPriority.QueryPath}");
                }
                prevPriority = priority;
            }
        }

        [Test, Description("Model#Queryのテスト")]
        public void QueryPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<Model>()
                {
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1") },
                    new Model() { Name = "apple", LogicalID = new ModelIDList("logical1"), StylingID = new ModelIDList("style1") },
                    new Model() { Name = "orange", LogicalID = new ModelIDList("logical2"),
                        Children = new List<Model>(){
                            new Model() { Name = "apple", LogicalID = new ModelIDList("logical1") },
                            new Model() { Name = "root", StylingID = new ModelIDList("style1"),
                                Children = new List<Model>(){
                                    new Model() { Name = "apple" },
                                },
                            },
                        },
                    },
                    new Model() { Name = "grape", StylingID = new ModelIDList("style1")},
                    new Model() { Name = "banana", LogicalID = new ModelIDList("logical2"), StylingID = new ModelIDList("style1")},
                },
            };

            {// LogicalID=logical1なModelを検索する
                var results = root.Query("#logical1");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '#logical1'");
            }
            {// 親Modelの名前がrootでLogicalID=logical1なModelを検索する
                var results = root.Query("root/#logical1");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query 'root/#logical1'");
            }
            {// StyleID=style1なModelを検索する
                var results = root.Query(".style1");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/orange/root",
                    "root/grape",
                    "root/banana",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '.style1'");
            }
            {// 親がorangeでStyleID=style1なModelを検索する
                var results = root.Query("orange/.style1");
                var correctModelPathList = new List<string>
                {
                    "root/orange/root",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query 'orange/.style1'");
            }
            {// LogicalID=logical1でStyleID=style1なModelを検索する
                var results = root.Query("#logical1 .style1");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '#logical1 .style1'");
            }
            {// Name=appleなModelを検索する
                var results = root.Query("apple");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange/apple",
                    "root/orange/root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query 'apple'");
            }
            {// Pathにroot/appleを含むものを検索する
                var results = root.Query("root/apple");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange/root/apple"
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query 'root/apple'");
            }
            {// 検索ルートを基準にしたPathがroot/appleなModelを検索する
                var results = root.Query("/root/apple");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '/root/apple'");
            }
            {// 検索ルートを基準にしたPathがroot/appleなModel以外のModelを検索する
                var results = root.Query("~root/apple");
                var correctModelPathList = new List<string>
                {
                    "root/orange/root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '~root/apple'");
            }
            {// 全てのModelを検索する
                var results = root.Query("*");
                var correctModelPathList = new List<string>
                {
                    "root",
                    "root/apple",
                    "root/apple",
                    "root/orange",
                    "root/orange/apple",
                    "root/orange/root",
                    "root/orange/root/apple",
                    "root/grape",
                    "root/banana",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '*'");
            }
            {// 全てのModelを検索する
                var results = root.Query("*/*/*");
                var correctModelPathList = new List<string>
                {
                    "root/orange/apple",
                    "root/orange/root",
                    "root/orange/root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '*'");
            }
            {// RootModelを検索する
                var results = root.Query("/*");
                var correctModelPathList = new List<string>
                {
                    "root",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '/*'");
            }
            {// RootModel以外のModelを検索する
                var results = root.Query("~*");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange",
                    "root/orange/apple",
                    "root/orange/root",
                    "root/orange/root/apple",
                    "root/grape",
                    "root/banana",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '~*'");
            }
            {// Pathがroot/orangeの子Modelを検索する
                var results = root.Query("root/orange/*");
                var correctModelPathList = new List<string>
                {
                    "root/orange/apple",
                    "root/orange/root",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query 'root/orange/*'");
            }
        }

        void AssertQueryResults(IEnumerable<string> corrects, IEnumerable<Model> gots, string message)
        {
            var correctList = corrects.ToList();
            Assert.AreEqual(correctList.Count, gots.Count(), $"{message}");
            foreach (var r in gots)
            {
                var path = r.GetPath();
                var index = correctList.IndexOf(r.GetPath());
                Assert.AreNotEqual(-1, index, $"don't found child(path={path})... msg:{message}");
                correctList.RemoveAt(index);
            }
        }
    }
}
