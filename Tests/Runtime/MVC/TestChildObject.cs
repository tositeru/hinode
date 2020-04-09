using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="ChildObject{T}"/>
    /// </summary>
    public class TestChildObject : TestBase
    {
        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            yield return null;
            var root = new GameObject("root");
            var child = new GameObject("child1", typeof(BoxCollider));
            child.transform.parent = root.transform;

            Assert.DoesNotThrow(() => {
                var childCollider = new ChildObject<BoxCollider>(root.transform, "child1");

                Assert.IsNotNull(childCollider.Instance);
                Assert.AreSame(child, childCollider.GameObject);
                Assert.AreSame(child.transform, childCollider.Transform);
                Assert.AreSame(child.GetComponent<BoxCollider>(), childCollider.Instance);
                Assert.AreSame(child.GetComponent<BoxCollider>(), childCollider.GetComponent<BoxCollider>());
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var invalidChild = new ChildObject<BoxCollider>(root.transform, "_____invalid");
            });
        }

        [UnityTest]
        public IEnumerator GetOrCreatePasses()
        {
            yield return null;
            var root = new GameObject("root");
            var child = new GameObject("child1", typeof(BoxCollider));
            child.transform.parent = root.transform;

            Assert.DoesNotThrow(() => {
                ChildObject<BoxCollider> childObject = null;
                var inst = ChildObject<BoxCollider>.GetOrCreate(ref childObject, root.transform, "child1");

                Assert.IsNotNull(childObject);
                Assert.IsNotNull(inst);
                Assert.AreSame(childObject.Instance, inst);

                //Return childObject when it be not null.
                var prevChildObj = childObject;
                var inst2 = ChildObject<BoxCollider>.GetOrCreate(ref childObject, root.transform, "child1");
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
