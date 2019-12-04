using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Runtime.Extensions
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
            AssertionUtils.AssertEnumerable(root.GetChildEnumerable(), corrects.Select(_g => _g.transform), "TransformExtensions#GetChildEnumerableの探索順が想定したものになっていません。");
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

            AssertionUtils.AssertEnumerable(root.transform.GetHierarchyEnumerable(), corrects.Select(_g => _g.transform), "TransformExtensions#GetHierarchyEnumerableの探索順が想定したものになっていません。");
        }
    }
}
