using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestSceneExtensions"/>
    /// </summary>
    public static class SceneExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestSceneExtensions.GetGameObjectEnumerablePasses()"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IEnumerable<GameObject> GetGameObjectEnumerable(this Scene s)
        {
            return new GameObjectEnumerable(s);
        }


        /// <summary>
        /// <seealso cref="GetGameObjectEnumerable(Scene)"/>
        /// </summary>
        public class GameObjectEnumerable : IEnumerable<GameObject>, IEnumerable
        {
            Scene _target;
            public GameObjectEnumerable(Scene target)
            {
                _target = target;
            }

            public IEnumerator<GameObject> GetEnumerator()
            {
                GameObject[] roots;
                try
                {
                    roots = _target.GetRootGameObjects();
                }
                catch(System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Fail to GetRootGameObjects()... {_target.name} {System.Environment.NewLine}{System.Environment.NewLine}{e}");
                    yield break;
                }

                foreach(var root in roots)
                {
                    foreach(var obj in root.transform.GetHierarchyEnumerable())
                    {
                        yield return obj.gameObject;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static IEnumerable<Scene> GetSceneEnumerable()
        {
            return new SceneEnumerable();
        }

        class SceneEnumerable : IEnumerable<Scene>, IEnumerable
        {
            public SceneEnumerable()
            { }

            public IEnumerator<Scene> GetEnumerator()
            {
                for(var i=0; i<SceneManager.sceneCount; ++i)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    yield return scene;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

    }
}
