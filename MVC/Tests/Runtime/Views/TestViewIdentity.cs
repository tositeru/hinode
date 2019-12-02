using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Views
{
    /// <summary>
	/// <seealso cref="ViewIdentity"/>
	/// </summary>
    public class TestViewIdentity
    {
        [Test]
        public void CreateOnlyMainIdPasses()
        {
            var viewID = ViewIdentity.Create("apple");
            Assert.AreEqual("apple", viewID.MainID);
            Assert.IsFalse(viewID.HasChildIDs);
            AssertionUtils.AssertEnumerable(
                new string[] { }
                , viewID.ChildIDs
                , "");
        }

        [Test]
        public void CreateIncludingChildIDsPasses()
        {
            var viewID = ViewIdentity.Create("apple", "orange", "grape");
            Assert.AreEqual("apple", viewID.MainID);
            Assert.IsTrue(viewID.HasChildIDs);
            AssertionUtils.AssertEnumerable(
                new string[] { "orange", "grape" }
                , viewID.ChildIDs
                , "");
        }

        [Test]
        public void ParsePasses()
        {
            var viewID = ViewIdentity.Parse("apple.orange.grape");
            Assert.AreEqual("apple", viewID.MainID);
            Assert.IsTrue(viewID.HasChildIDs);
            AssertionUtils.AssertEnumerable(
                new string[] { "orange", "grape" }
                , viewID.ChildIDs
                , "");
        }

        [Test]
        public void IsEmptyPasse()
        {
            Assert.IsTrue(ViewIdentity.Create().IsEmpty);
            Assert.IsTrue(ViewIdentity.Parse("").IsEmpty);
        }

        [Test]
        public void ToStringPasses()
        {
            Assert.AreEqual("apple", ViewIdentity.Create("apple").ToString());
            Assert.AreEqual("apple.orange.grape", ViewIdentity.Create("apple", "orange", "grape").ToString());
        }

        [Test]
        public void EqualsPasses()
        {
            Assert.AreEqual(ViewIdentity.Create("apple"), ViewIdentity.Create("apple"));
            Assert.AreEqual(ViewIdentity.Create("apple", "orange", "grape"), ViewIdentity.Create("apple", "orange", "grape"));

            Assert.AreNotEqual(ViewIdentity.Create("apple", "grape", "orange"), ViewIdentity.Create("apple", "orange", "grape"));
        }

        [Test]
        public void EqualOperatorPasses()
        {
            Assert.IsTrue(ViewIdentity.Create("apple") == ViewIdentity.Create("apple"));
            Assert.IsTrue(ViewIdentity.Create("apple", "orange", "grape") ==  ViewIdentity.Create("apple", "orange", "grape"));

            Assert.IsFalse(ViewIdentity.Create("apple", "grape", "orange") == ViewIdentity.Create("apple", "orange", "grape"));
        }

        [Test]
        public void ValidTermPasses()
        {
            Assert.DoesNotThrow(() => {
                ViewIdentity.Create("apple");
                ViewIdentity.Create("APPLE");
                ViewIdentity.Create("ApPLE");
                ViewIdentity.Create("apple_");
                ViewIdentity.Create("123");
                ViewIdentity.Create("_ap2123Ple");
                ViewIdentity.Create("ap!");
                ViewIdentity.Create("ap?");
                ViewIdentity.Create("__");
                ViewIdentity.Create("!");
                ViewIdentity.Create("?");
            });
        }

        [Test]
        public void CreateInvalidTermFail()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Create($"app*le");
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Create("root", $"app/le");
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Create("root", "parent", $"@apple");
            });
        }

        [Test]
        public void ParseInvalidTermFail()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Parse($"app@le");
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Parse("root.app{le");
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Parse("root.parent.app^le");
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var viewID = ViewIdentity.Parse(null);
            });
        }

        [Test]
        public void OnlyMainIDViewIdentityPasses()
        {
            Assert.IsTrue(typeof(OnlyMainIDViewIdentity).IsSubclassOf(typeof(ViewIdentity)));

            var viewID = OnlyMainIDViewIdentity.Create("apple");
            Assert.AreEqual("apple", viewID.MainID);
            Assert.IsFalse(viewID.HasChildIDs);
            AssertionUtils.AssertEnumerable(
                new string[] { }
                , viewID.ChildIDs
                , "");
        }
    }
}
