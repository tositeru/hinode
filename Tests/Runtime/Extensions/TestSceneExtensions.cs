using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="SceneExtensions"/>
    /// </summary>
    public class TestSceneExtensions : TestBase
    {
        /// <summary>
        /// <seealso cref="SceneExtensions.GetGameObjectEnumerable(Scene)"/>
        /// </summary>
        [UnityTest]
        public IEnumerator GetGameObjectEnumerablePasses()
        {
            var scene = SceneManager.GetActiveScene();
            var corrects = new List<GameObject>();
            corrects.AddRange(scene.GetRootGameObjects());

            GameObjectExtensions.Create(
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
            );

            GameObjectExtensions.Create(
                ("root2", (Transform)null, new CreateGameObjectParam[] {
                    "A2",
                    ("B2", new CreateGameObjectParam[] {
                        "B2 Child0"
                    })
                }),
                corrects);

            AssertionUtils.AssertEnumerable(corrects, scene.GetGameObjectEnumerable(), "SceneExtensions#GetGameObjectEnumerableの探索順が想定したものになっていません。");
            yield return null;
        }
    }
}
