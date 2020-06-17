using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 現在アクティブになっているSceneにあるGameObjectにアクセスする便利クラスになります。
    /// 
    /// <seealso cref="Hinode.Tests.TestSceneObject"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SceneObject<T>
        where T : Component
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.TestSceneObject.BasicUsagePasses()"/>
        /// </summary>
        public T Instance { get; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestSceneObject.BasicUsagePasses()"/>
        /// </summary>
        public GameObject GameObject { get => Instance.gameObject; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestSceneObject.BasicUsagePasses()"/>
        /// </summary>
        public Transform Transform { get => Instance.transform; }

        public SceneObject(string objPath)
        {
            var scene = SceneManager.GetActiveScene();
            var splitPath = objPath.Split('/');
            Assert.IsTrue(splitPath.Length > 0);
            var rootObjName = splitPath[0];
            var root = scene.GetRootGameObjects().FirstOrDefault(_obj => _obj.name == rootObjName);
            Assert.IsNotNull(root);

            Transform obj = null;
            if(splitPath.Length > 1)
            {
                var childObjPath = objPath.Substring(rootObjName.Length+1);
                obj = root.transform.Find(childObjPath);
            }
            else
            {
                obj = root.transform;
            }
            Assert.IsNotNull(obj);
            Instance = obj.GetComponent<T>();
            Assert.IsNotNull(Instance);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestSceneObject.BasicUsagePasses()"/>
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetComponent<U>() where U : Component
            => Instance.GetComponent<U>();
        public U[] GetComponents<U>() where U : Component
            => Instance.GetComponents<U>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestSceneObject.GetOrCreatePasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="objPath"></param>
        /// <returns></returns>
        public static T GetOrCreate(ref SceneObject<T> target, string objPath)
        {
            if (target != null) return target.Instance;
            target = new SceneObject<T>(objPath);
            return target.Instance;
        }
    }
}
