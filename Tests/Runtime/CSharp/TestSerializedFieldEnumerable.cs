using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    public class TestSerializedFieldEnumerable
    {
        class BasicPassesClass
        {
#pragma warning disable CS0649, CS0414
            public int pub1;
            int value;
            [SerializeField] int pub2;
#pragma warning restore CS0649, CS0414
            public BasicPassesClass(int pub1, int pub2)
            {
                this.pub1 = pub1;
                this.pub2 = pub2;
            }
        }

        [Test]
        public void BasicPasses()
        {
            var inst = new BasicPassesClass(11, 22);
            
            foreach (var (got, correct) in inst.GetSerializedFieldEnumerable()
                .Zip(new object[] { 11, 22 }, (_e, _i) => (got: _e.Value, correct: _i)))
            {
                Assert.AreEqual(correct, got);
            }
        }

        class HierachyBasicPassesClass
        {
#pragma warning disable CS0649, CS0414
            public int pub1;
            int value;
            [SerializeField] int pub2;
#pragma warning restore CS0649, CS0414
            public HierachyBasicPassesClass2 clazz = new HierachyBasicPassesClass2(-1, -2, "subclass");

            public HierachyBasicPassesClass(int pub1, int pub2)
            {
                this.pub1 = pub1;
                this.pub2 = pub2;
            }
        }

        struct HierachyBasicPassesClass2
        {
#pragma warning disable CS0649, CS0414
            public int pub1;
            int _value;
            [SerializeField] int pri2;
            string _value2;
            public string str1;
#pragma warning restore CS0649, CS0414
            public HierachyBasicPassesClass2(int pub1, int pri2, string str)
            {
                this.pub1 = pub1;
                this.pri2 = pri2;
                str1 = str;

                _value = 0;
                _value2 = "";
            }
        }

        [Test]
        public void HierachyBasicPasses()
        {
            var inst = new HierachyBasicPassesClass(11, 22);
            inst.clazz = new HierachyBasicPassesClass2(-1, -2, "text");
            foreach (var (got, correct) in inst.GetHierarchySerializedFieldEnumerable()
                .Zip(new object[] { 11, 22, inst.clazz, -1, -2, "text" }, (_e, _i) => (got: _e.Value, correct: _i)))
            {
                Assert.AreEqual(correct, got);
            }
        }
    }
}
