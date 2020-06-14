﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="TransformExtensions"/>
    /// </summary>
    public class TestTransformExtensions : TestBase
    {
        /// <summary>
        /// <seealso cref="TransformExtensions.GetChildEnumerable(Transform)"/>
        /// </summary>
        [UnityTest]
        public IEnumerator GetChildEnumerablePass()
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
            yield return null;
        }

        /// <summary>
        /// <seealso cref="TransformExtensions.GetHierarchyEnumerable(Transform)"/>
        /// </summary>
        [UnityTest]
        public IEnumerator GetHierarchyEnumerablePass()
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
            yield return null;
        }

        /// <summary>
        /// <seealso cref="TransformExtensions.GetParentEnumerable(Transform)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GetParentEnumerablePass()
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
            yield return null;
        }
    }
}
