using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Attributes
{
    public class TestKeyAndTypeDictionaryGetter : TestBase
    {
        [HasKeyAndTypeDictionaryGetter(typeof(BasicUsagePassesClass))]
        class BasicUsagePassesClass
        {
            [KeyAndTypeDictionaryGetter]
            public static IReadOnlyDictionary<string, System.Type> GetDict()
            {
                return new Dictionary<string, System.Type>(){
                    { "A", typeof(string) },
                    { "B", typeof(BasicUsagePassesClass) },
                };
            }
        }

        [HasKeyAndTypeDictionaryGetter(typeof(InheritedBasicUsagePassesClass))]
        class InheritedBasicUsagePassesClass : BasicUsagePassesClass
        {
            [KeyAndTypeDictionaryGetter]
            public static IReadOnlyDictionary<string, System.Type> GetDict2()
            {
                return new Dictionary<string, System.Type>(){
                    { "A", typeof(int) },
                    { "O", typeof(InheritedBasicUsagePassesClass) },
                };
            }
        }

        [Test]
        public void BasicUsagePasses()
        {
            {
                var type = typeof(BasicUsagePassesClass);
                var attr = type.GetCustomAttribute<HasKeyAndTypeDictionaryGetterAttribute>();
                AssertionUtils.AreEqual(BasicUsagePassesClass.GetDict(), attr.GetDictionary(type), "値が取得できていません。");
            }

            {
                var type = typeof(InheritedBasicUsagePassesClass);
                var attr = type.GetCustomAttribute<HasKeyAndTypeDictionaryGetterAttribute>();
                var correct = new Dictionary<string, System.Type>().Merge(false,
                    InheritedBasicUsagePassesClass.GetDict2(),
                    BasicUsagePassesClass.GetDict());
                AssertionUtils.AreEqual(correct, attr.GetDictionary(type), "値が取得できていません。");
            }
        }
    }
}
