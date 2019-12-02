using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Extensions
{
    public class TestTransformExtensions : TestBase
    {
        // A Test behaves as an ordinary method
        [Test]
        public void GetChildEnumerablePass()
        {
            var corrects = new List<GameObject>();
            var root = GameObjectExtensions.Create(
                ("root", (Transform)null, new CreateGameObjectParam[] {
                    "A",
                    "B",
                    "C", }
                ),
                corrects
            ).transform;

            corrects.RemoveAt(0);
            AssertionUtils.AssertEnumerable(corrects.Select(_g => _g.transform), root.GetChildEnumerable(), "TransformExtensions#GetChildEnumerableの探索順が想定したものになっていません。");
        }

        // A Test behaves as an ordinary method
        [Test]
        public void GetHierarchyEnumerablePass()
        {
            var corrects = new List<GameObject>();
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
                corrects
            ).transform;

            AssertionUtils.AssertEnumerable(corrects.Select(_g => _g.transform), root.transform.GetHierarchyEnumerable(), "TransformExtensions#GetHierarchyEnumerableの探索順が想定したものになっていません。");
        }

        [Test]
        public void GetParentEnumerablePass()
        {
            var corrects = new List<GameObject>();
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
                corrects
            ).transform;

            var child = root.Find("A/A Child0");
            var A = root.Find("A");
            var errorMessage = $"'root/A/A Child0'の親Transformを正しく取得できていません";
            AssertionUtils.AssertEnumerable(
                new Transform[] { A, root }
                , child.GetParentEnumerable()
                , errorMessage);
        }
    }
}
