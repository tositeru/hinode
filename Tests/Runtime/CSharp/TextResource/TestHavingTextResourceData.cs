using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.TextResource
{
    /// <summary>
	/// <seealso cref="HavingTextResourceData"/>
	/// </summary>
    public class TestHavingTextResourceData
    {
        [Test]
        public void SetParamsPasses()
        {
            var data = new HavingTextResourceData();
            var paramList = new object[]
            {
                100,
                "Apple",
                1.23f,
            };
            data.SetParams(paramList);

            Assert.AreNotSame(paramList, data.GetTextResourceParams());
            Assert.AreEqual(paramList.Length, data.ParamCount);
            AssertionUtils.AssertEnumerable(
                paramList,
                data.GetTextResourceParams(),
                "");
        }

        [Test]
        public void ResizeParamsPasses()
        {
            var data = new HavingTextResourceData()
            {
                HavingTextResourceKey = "key1",
            };
            var paramList = new object[]
            {
                100,
                "Apple",
                1.23f,
            };
            data.SetParams(paramList);

            {
                data.ResizeParams(1);
                Assert.AreEqual(1, data.ParamCount);
                AssertionUtils.AssertEnumerable(
                    new object[] { paramList[0] },
                    data.GetTextResourceParams(),
                    "");
            }
            Debug.Log($"Success to Resize paramList(Reduce ParamCount)");

            {
                data.ResizeParams(3);
                Assert.AreEqual(3, data.ParamCount);
                AssertionUtils.AssertEnumerable(
                    new object[] { paramList[0], null, null },
                    data.GetTextResourceParams(),
                    "");
            }
            Debug.Log($"Success to Resize paramList(Increase ParamCount)");
        }

        [Test]
        public void SetParamByIndexPasses()
        {
            var data = new HavingTextResourceData();
            data.ResizeParams(3);
            data.SetParam(0, 100);
            data.SetParam(1, "Apple");
            data.SetParam(2, 1.23f);

            Assert.AreEqual(3, data.ParamCount);
            AssertionUtils.AssertEnumerable(
                new object[]
                {
                    100,
                    "Apple",
                    1.23f,
                },
                data.GetTextResourceParams(),
                "");

            Debug.Log($"Success to Basic SetParam");

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                data.SetParam(10, 654);
            });
            Debug.Log($"Success to Out of Range Index(over paramCount)");
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                data.SetParam(-1, 654);
            });
            Debug.Log($"Success to Out of Range Index(minus index)");
        }

        [Test]
        public void TextResourceGetPasses()
        {
            var resources = new TextResources()
                .Add("key1", "{0}, {1}, {2}");

            var data = new HavingTextResourceData()
            {
                HavingTextResourceKey = "key1",
            };
            var paramList = new object[]
            {
                100,
                "Apple",
                1.23f,
            };
            data.SetParams(paramList);

            Assert.AreEqual("100, Apple, 1.23", resources.Get(data));
        }

        [Test]
        public void TextResourceContainsPasses()
        {
            var resources = new TextResources()
                .Add("key1", "{0}, {1}, {2}");

            var data = new HavingTextResourceData()
            {
                HavingTextResourceKey = "key1",
            };
            var paramList = new object[]
            {
                100,
                "Apple",
                1.23f,
            };
            data.SetParams(paramList);

            Assert.IsTrue(resources.Contains(data));
        }

        [Test]
        public void CreatePasses()
        {
            var data = HavingTextResourceData.Create("key1", 100, "Apple");
            var resources = new TextResources()
                .Add("key1", "This is {0}, {1}.");

            Assert.AreEqual("This is 100, Apple.", resources.Get(data));
        }
    }
}
