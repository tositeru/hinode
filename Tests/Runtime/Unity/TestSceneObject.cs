using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Unity
{
    /// <summary>
    /// <seealso cref="SceneObject{T}"/>
    /// </summary>
    public class TestSceneObject : TestBase
    {
        /// <summary>
        /// <seealso cref="SceneObject{T}.Instance"/>
        /// <seealso cref="SceneObject{T}.GameObject"/>
        /// <seealso cref="SceneObject{T}.Transform"/>
        /// <seealso cref="SceneObject{T}.GetComponent{U}"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            yield return null;
            var root = new GameObject("root", typeof(BoxCollider));
            var root2 = new GameObject("root2", typeof(BoxCollider));
            var child = new GameObject("child1", typeof(BoxCollider));
            child.transform.parent = root.transform;

            Assert.DoesNotThrow(() => {
                var childCollider = new SceneObject<BoxCollider>("root/child1");

                Assert.IsNotNull(childCollider.Instance);
                Assert.AreSame(child, childCollider.GameObject);
                Assert.AreSame(child.transform, childCollider.Transform);
                Assert.AreSame(child.GetComponent<BoxCollider>(), childCollider.Instance);
                Assert.AreSame(child.GetComponent<BoxCollider>(), childCollider.GetComponent<BoxCollider>());

                Assert.AreSame(root, new SceneObject<BoxCollider>("root").GameObject);
                Assert.AreSame(root2, new SceneObject<BoxCollider>("root2").GameObject);
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var childCollider = new SceneObject<BoxCollider>("___invalidPath/root/child1");
            });
        }

        /// <summary>
        /// <seealso cref="SceneObject{T}.GetOrCreate(ref SceneObject{T}, string)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GetOrCreatePasses()
        {
            yield return null;
            var root = new GameObject("root");
            var child = new GameObject("child1", typeof(BoxCollider));
            child.transform.parent = root.transform;

            Assert.DoesNotThrow(() => {
                SceneObject<BoxCollider> childObject = null;
                var inst = SceneObject<BoxCollider>.GetOrCreate(ref childObject, "root/child1");

                Assert.IsNotNull(childObject);
                Assert.IsNotNull(inst);
                Assert.AreSame(childObject.Instance, inst);

                //Return childObject when it be not null.
                var prevChildObj = childObject;
                var inst2 = SceneObject<BoxCollider>.GetOrCreate(ref childObject, "root/child1");
                Assert.AreSame(inst, inst2);
                Assert.AreSame(prevChildObj, childObject);
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                ChildObject<BoxCollider> childObject = null;
                var inst = ChildObject<BoxCollider>.GetOrCreate(ref childObject, root.transform, "____invalid");
            });
        }

    }
}
