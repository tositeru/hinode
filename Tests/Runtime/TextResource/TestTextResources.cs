using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.TextResource
{
    public class TestTextResources
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            var resource = new TextResources();
            var formattedKey1 = "formattedKey";
            var normalKey = "normalKey";
            resource
                .Add(formattedKey1, "Apple is {0}.")
                .Add(normalKey, "Orange is furits.");

            Assert.AreEqual(2, resource.Count);
            Assert.AreEqual("Apple is 100.", resource.Get(formattedKey1, 100));
            Assert.AreEqual("Orange is furits.", resource.Get(normalKey));
            Assert.IsTrue(resource.Contains(normalKey));
            Assert.IsTrue(resource.Contains(formattedKey1));
            Debug.Log($"Success to Add and Get Methods!");

            {
                resource.Dispose();
                Assert.AreEqual(0, resource.Count);
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                    Assert.AreEqual("Apple is 100.", resource.Get(formattedKey1, 100));
                });
                Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                    Assert.AreEqual("Orange is furits.", resource.Get(normalKey));
                });
                Assert.IsFalse(resource.Contains(normalKey));
                Assert.IsFalse(resource.Contains(formattedKey1));
            }
            Debug.Log($"Success to Dispose!");
        }
    }
}
