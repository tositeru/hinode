using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class ChildObject<T>
        where T : Component
    {
        public T Instance { get; }
        public GameObject GameObject { get => Instance.gameObject; }
        public Transform Transform { get => Instance.transform; }

        public ChildObject(Transform parent, string objPath)
        {
            var obj = parent.Find(objPath);
            Instance = obj.GetComponent<T>();
            Assert.IsNotNull(Instance);
        }

        public U GetComponent<U>() where U : Component
            => Instance.GetComponent<U>();
        public U[] GetComponents<U>() where U : Component
            => Instance.GetComponents<U>();
    }
}
