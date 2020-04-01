using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode
{
    public class SceneObject<T>
        where T : Component
    {
        public T Instance { get; }
        public GameObject GameObject { get => Instance.gameObject; }
        public Transform Transform { get => Instance.transform; }

        public SceneObject(string objPath)
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjName = objPath.Split('/')[0];
            var root = scene.GetRootGameObjects().First(_obj => _obj.name == rootObjName);

            var childObjPath = objPath.Substring(rootObjName.Length+1);
            var obj = root.transform.Find(childObjPath);
            Instance = obj.GetComponent<T>();
            Assert.IsNotNull(Instance);
        }

        public U GetComponent<U>() where U : Component
            => Instance.GetComponent<U>();
        public U[] GetComponents<U>() where U : Component
            => Instance.GetComponents<U>();
    }
}
