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
            Assert.IsNotNull(obj);
            Instance = obj.GetComponent<T>();
            Assert.IsNotNull(Instance);
        }

        public U GetComponent<U>() where U : Component
            => Instance.GetComponent<U>();
        public U[] GetComponents<U>() where U : Component
            => Instance.GetComponents<U>();

        public static T GetOrCreate(ref ChildObject<T> target, Transform parent, string objPath)
        {
            if (target != null) return target.Instance;
            target = new ChildObject<T>(parent, objPath);
            return target.Instance;
        }
    }
}
