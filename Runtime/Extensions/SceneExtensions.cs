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
                foreach(var root in _target.GetRootGameObjects())
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
    }
}
