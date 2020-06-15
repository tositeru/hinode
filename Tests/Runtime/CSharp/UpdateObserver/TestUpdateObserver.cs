using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.IUpdateObserver
{
    /// <summary>
	/// <seealso cref="UpdateObserver"/>
	/// </summary>
    public class TestUpdateObserver
    {
        /// <summary>
        /// <seealso cref="UpdateObserver{T}.DidUpdated"/>
        /// <seealso cref="UpdateObserver{T}.Value"/>
        /// <seealso cref="UpdateObserver{T}.Reset"/>
        /// </summary>
        [Test]
        public void BasicUsagePasses()
        {
            var v = new UpdateObserver<int>();
            Assert.IsFalse(v.DidUpdated);

            v.Value = 100;
            Assert.AreEqual(100, v.Value);
            Assert.IsTrue(v.DidUpdated);

            v.Value = -100;
            Assert.AreEqual(-100, v.Value);
            Assert.IsTrue(v.DidUpdated);

            var prevValue = v.Value;
            v.Reset();
            Assert.AreEqual(prevValue, v.Value);
            Assert.IsFalse(v.DidUpdated);
        }

        [Test]
        public void OnChangedValuePasses()
        {
            var v = new UpdateObserver<int>(0);
            var counter = 0;
            var recievedValue = 0;
            v.OnChangedValue.Add((i) => {
                counter++;
                recievedValue = i;
            });

            {
                v.Value = 100;
                Assert.AreEqual(1, counter);
                Assert.AreEqual(v.Value, recievedValue);
            }
            Debug.Log($"Success to Call OnChangedValue Callback!");

            {
                v.Value = v.Value;
                Assert.AreEqual(1, counter);
                Assert.AreEqual(v.Value, recievedValue);
            }
            Debug.Log($"Success not to Call OnChangedValue Callback when not update UpdateObserver#Value!");
        }

    }
}
