using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Reflection
{
    public class TestRefCache
    {
        class InstanceSample
		{
#pragma warning disable CS0414
            int field = 1;
			public int Func(int a, int b) { return a + b; }
            public string Prop { get; set; }
#pragma warning restore CS0414
        }

        [Test]
        public void BasicUsageAtInstancePassess()
        {
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			string fieldName = "field";
			var field = refCache.GetField(instance, fieldName);
			Assert.AreEqual(1, field);
			refCache.SetField(instance, fieldName, 100);
			Assert.AreEqual(100, refCache.GetField(instance, fieldName));
			Assert.IsTrue(refCache.HasField(fieldName), $"キャッシュ情報に保存されていません。Field Name={fieldName}");

			string methodName = "Func";
			var result = refCache.Invoke(instance, methodName, 2, 3);
			Assert.AreEqual(5, result);
			Assert.IsTrue(refCache.HasMethod(methodName), $"キャッシュ情報に保存されていません。Method Name={methodName}");

			string propName = "Prop";
			refCache.SetProp(instance, propName, "test");
			var prop = (string)refCache.GetProp(instance, propName);
			Assert.AreEqual("test", prop);
			Assert.IsTrue(refCache.HasProp(propName), $"キャッシュ情報に保存されていません。Method Name={propName}");
		}

		class StaticSample
		{
#pragma warning disable CS0414
            static int field = 1;
			public static int Func(int a, int b) { return a + b; }
			public static string Prop { get; set; }
#pragma warning restore CS0414
        }
        [Test]
		public void BasicUsageAtStaticPassess()
		{
			var refCache = new RefCache(typeof(StaticSample));

			string fieldName = "field";
			var field = refCache.GetField(null, fieldName);
			Assert.AreEqual(1, field);
			refCache.SetField(null, fieldName, 100);
			Assert.AreEqual(100, refCache.GetField(null, fieldName));
			Assert.IsTrue(refCache.HasField(fieldName), $"キャッシュ情報に保存されていません。Field Name={fieldName}");

			string methodName = "Func";
			var result = refCache.Invoke(null, methodName, 2, 3);
			Assert.AreEqual(5, result);
			Assert.IsTrue(refCache.HasMethod(methodName), $"キャッシュ情報に保存されていません。Method Name={methodName}");

			string propName = "Prop";
			refCache.SetProp(null, propName, "test");
			var prop = (string)refCache.GetProp(null, propName);
			Assert.AreEqual("test", prop);
			Assert.IsTrue(refCache.HasProp(propName), $"キャッシュ情報に保存されていません。Method Name={propName}");
		}
	}
}
