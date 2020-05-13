using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    public class TestGameObjectExtensions : TestBase
    {
        [UnityTest]
        public IEnumerator GetOrAddComponentPasses()
        {
            yield return null;

            var obj = new GameObject("test");

            Assert.IsNull(obj.GetComponent<BoxCollider>());
            var box = obj.GetOrAddComponent<BoxCollider>();
            Assert.IsNotNull(obj.GetComponent<BoxCollider>());
            Assert.AreSame(obj.GetComponent<BoxCollider>(), box);

            var box2 = obj.GetOrAddComponent<BoxCollider>();
            Assert.AreSame(box2, box);
        }

        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator CreateGameObjectPasses()
        {
            yield return null;

            var createdList = new List<GameObject>();
            var root = GameObjectExtensions.Create(
                ("root", (Transform)null, new CreateGameObjectParam[] {
                    ("A", new CreateGameObjectParam[] {
                        "A Child0",
                        "A Child1",
                        "A Child2", }
                    ),
                    "B",
                    ("C", new CreateGameObjectParam[] {
                        "C Child0", }
                    ), }
                ),
                createdList
            );

            var A = root.transform.GetChild(0).gameObject;
            var B = root.transform.GetChild(1).gameObject;
            var C = root.transform.GetChild(2).gameObject;
            var correctList = new List<GameObject> {
                root,
                A,
                A.transform.GetChild(0).gameObject,
                A.transform.GetChild(1).gameObject,
                A.transform.GetChild(2).gameObject,
                B,
                C,
                C.transform.GetChild(0).gameObject,
            };
            AssertionUtils.AssertEnumerable(createdList, correctList, "想定した順にリストへ追加されていません");

            AssertGameObject(root, "root", new GameObject[]{ A, B, C }, "invalid root...");
            AssertGameObject(A, "A", new GameObject[] { A.transform.GetChild(0).gameObject, A.transform.GetChild(1).gameObject, A.transform.GetChild(2).gameObject }, "invalid A");
            AssertGameObject(A.transform.GetChild(0).gameObject, "A Child0", new GameObject[] { }, "invalid A Child0");
            AssertGameObject(A.transform.GetChild(1).gameObject, "A Child1", new GameObject[] { }, "invalid A Child1");
            AssertGameObject(A.transform.GetChild(2).gameObject, "A Child2", new GameObject[] { }, "invalid A Child2");
            AssertGameObject(B, "B", new GameObject[] { }, "invalid B");
            AssertGameObject(C, "C", new GameObject[] { C.transform.GetChild(0).gameObject }, "invalid C");
            AssertGameObject(C.transform.GetChild(0).gameObject, "C Child0", new GameObject[] { }, "invalid C Child0");
        }

        void AssertGameObject(GameObject got, string name, IEnumerable<GameObject> children, string message)
        {
            Assert.AreEqual(name, got.name, $"{message}: Don't equal name...");
            Assert.AreEqual(children.Count(), got.transform.childCount, $"{message}: Don't equal child count...");
            int index = 0;
            foreach(var pair in children.Zip(got.transform.GetChildEnumerable(), (child, _got) => (child, got: _got.gameObject)))
            {
                Assert.AreSame(pair.child, pair.got, $"{message}: Don't equal child at index={index}");
            }
        }

        [UnityTest]
        public IEnumerator CreateGameObjectCallbackPasses()
        {
            yield return null;

            int counter = 0;
            System.Action<GameObject> onCreated = (obj) => {
                counter++;
            };
            System.Action<GameObject> validateChildren = (obj) => {
                onCreated(obj);
                AssertionUtils.AssertEnumerable(
                    obj.transform.GetHierarchyEnumerable().Select(_o => _o.name),
                    new string[]{ "root", "A", "B", "C" }, "onCreatedが子要素の生成後に呼び出されていません。");
            };
            var createdList = new List<GameObject>();
            var root = GameObjectExtensions.Create(
                ("root", validateChildren, new CreateGameObjectParam[] {
                    ("A", onCreated),
                    ("B", onCreated),
                    ("C", onCreated)}
                ),
                createdList
            );

            Assert.AreEqual(4, counter, "onCreatedが想定した回数呼び出されていません。");
        }
    }
}
