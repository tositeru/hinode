using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 子GameObjectのComponentにアクセスするための便利クラスになります。
    /// 
    /// <seealso cref="Hinode.Tests.TestChildObject"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChildObject<T>
        where T : Component
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.BasicUsagePasses()"/>
        /// </summary>
        public T Instance { get; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.BasicUsagePasses()"/>
        /// </summary>
        public GameObject GameObject { get => Instance.gameObject; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.BasicUsagePasses()"/>
        /// </summary>
        public Transform Transform { get => Instance.transform; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.BasicUsagePasses()"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objPath"></param>
        public ChildObject(Transform parent, string objPath)
        {
            var obj = parent.Find(objPath);
            Assert.IsNotNull(obj);
            Instance = obj.GetComponent<T>();
            Assert.IsNotNull(Instance);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.BasicUsagePasses()"/>
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetComponent<U>() where U : Component
            => Instance.GetComponent<U>();
        public U[] GetComponents<U>() where U : Component
            => Instance.GetComponents<U>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.TestChildObject.GetOrCreatePasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parent"></param>
        /// <param name="objPath"></param>
        /// <returns></returns>
        public static T GetOrCreate(ref ChildObject<T> target, Transform parent, string objPath)
        {
            if (target != null) return target.Instance;
            target = new ChildObject<T>(parent, objPath);
            return target.Instance;
        }
    }
}
