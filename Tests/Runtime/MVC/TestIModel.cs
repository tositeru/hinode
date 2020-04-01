using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="IModel"/>
    /// </summary>
    public class TestIModel : TestBase
    {
        [Test]
        public void PathPasses()
        {
            var root = new Model() { Name = "root" };
            Assert.AreEqual("root", root.Path());

            var rootApple = new Model() { Name = "apple", Parent = root };
            Assert.AreEqual("root/apple", rootApple.Path() );

            var rootOrange = new Model() { Name = "orange", Parent = root };
            Assert.AreEqual("root/orange", rootOrange.Path());

            var rootOrangeApple = new Model() { Name = "apple", Parent = rootOrange };
            Assert.AreEqual("root/orange/apple", rootOrangeApple.Path());
        }

        [Test]
        public void GetChildPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<IModel>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange", },
                    new Model() { Name = "grape", },
                },
            };

            Assert.AreEqual(4, root.ChildCount);
            for(var i=0; i<root.ChildCount; ++i)
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
                Children = new List<IModel>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange", },
                    new Model() { Name = "grape", },
                },
            };

            foreach(var (child, index) in root.Children
                .Zip(Enumerable.Range(0, root.ChildCount), (c, i) => (child: c, index:i)))
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
                Children = new List<IModel>()
                {
                    new Model() { Name = "apple", },
                    new Model() { Name = "apple", },
                    new Model() { Name = "orange",
                        Children = new List<IModel>()
                        {
                            new Model() { Name = "apple" },
                            new Model() { Name = "root",
                                Children = new List<IModel>()
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
            AssertQueryResults(correctModelPathList, root.GetHierarchyEnumerable(), "Failed to Traverse IModel.GetHierarchyEnumerable()...");
        }

        [Test, Description("IModel#QueryParentOrChildrenのテスト")]
        public void QueryParentOrChildrenPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<IModel>()
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
                AssertQueryResults(corrects, root.QueryParentOrChildren("apple"), $"Failed to query(apple)...");
            }
            {//#logical1
                var corrects = new List<string>
                {
                    "root/apple", "root/apple"
                };
                AssertQueryResults(corrects, root.QueryParentOrChildren("#logical1"), $"Failed to query(#logical1)...");
            }
            {//.style1
                var corrects = new List<string>
                {
                    "root/apple", "root/grape", "root/banana",
                };
                AssertQueryResults(corrects, root.QueryParentOrChildren(".style1"), $"Failed to query(.style1)...");
            }
            {//apple .style1
                var corrects = new List<string>
                {
                    "root/apple"
                };
                AssertQueryResults(corrects, root.QueryParentOrChildren("apple .style1"), $"Failed to query(apple .style1)...");
            }
            {//#logical1 .style1
                var corrects = new List<string>
                {
                    "root/apple"
                };
                AssertQueryResults(corrects, root.QueryParentOrChildren("#logical1 .style1"), $"Failed to query(#logical1 .style1)...");
            }
            {//#logical2 .style1
                var corrects = new List<string>
                {
                    "root/banana"
                };
                AssertQueryResults(corrects, root.QueryParentOrChildren("#logical2 .style1"), $"Failed to query(#logical2 .style1)...");
            }

            {//..
                Assert.AreEqual(1, root.QueryParentOrChildren("..").Count(), $"Failed to query(..)...");
                Assert.IsNull(root.QueryParentOrChildren("..").First(), $"Failed to query(..)...");
                var child = root.GetChild(0);
                AssertQueryResults(new List<string> { root.Path() }, child.QueryParentOrChildren(".."), $"Failed to query(..)...");
            }
        }

        // A Test behaves as an ordinary method
        [Test, Description("IModel#Queryのテスト")]
        public void NameQueryPasses()
        {
            var root = new Model()
            {
                Name = "root",
                Children = new List<IModel>()
                {
                    new Model()
                    {
                        Name = "apple",
                    },
                    new Model()
                    {
                        Name = "apple",
                    },
                    new Model()
                    {
                        Name = "orange",
                        Children = new List<IModel>()
                        {
                            new Model()
                            {
                                Name = "apple"
                            },
                            new Model()
                            {
                                Name = "root",
                                Children = new List<IModel>()
                                {
                                    new Model() { Name = "apple" },
                                }
                            }
                        },
                    }
                },
            };
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
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '*'");
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
            {// Parentの名前がrootなModelを検索する
                var results = root.Query("../root");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange",
                    "root/orange/root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '../root'");
            }
            {// Parentの名前がrootなModelを検索する
                var results = root.Query("../root/apple");
                var correctModelPathList = new List<string>
                {
                    "root/apple",
                    "root/apple",
                    "root/orange/root/apple",
                };
                AssertQueryResults(correctModelPathList, results, "Failed to query '../root/apple'");
            }
        }

        void AssertQueryResults(IEnumerable<string> corrects, IEnumerable<IModel> gots, string message)
        {
            var correctList = corrects.ToList();
            Assert.AreEqual(correctList.Count, gots.Count());
            foreach (var r in gots)
            {
                var path = r.Path();
                var index = correctList.IndexOf(r.Path());
                Assert.AreNotEqual(-1, index, $"don't found child(path={path})... msg:{message}");
                correctList.RemoveAt(index);
            }
        }
    }
}
