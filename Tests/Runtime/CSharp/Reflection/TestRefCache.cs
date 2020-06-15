using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Reflection
{
	/// <summary>
    /// <seealso cref="RefCache"/>
    /// </summary>
    public class TestRefCache
    {
        class InstanceSample
		{
#pragma warning disable CS0414
			public const int literalField = 111;
            int field = 1;
			public int Func(int a, int b) { return a + b; }
            public string Prop { get; set; }

			public int OverloadFunc(int a) { return a; }
			public int OverloadFunc(int a, string str) { return a + str.GetHashCode(); }

			int _getterOnlyProp;
			public int GetterOnlyProp { get => _getterOnlyProp; }
			int _setterOnlyProp;
			public int SetterOnlyProp { set => _setterOnlyProp = value; }
#pragma warning restore CS0414

			public InstanceSample() { }
			public InstanceSample(int field)
            {
				this.field = field;
            }
			public InstanceSample(string prop)
			{
				this.Prop = prop;
			}
			public InstanceSample(int field, string prop)
			{
				this.field = field;
				this.Prop = prop;
			}

			public int GetField() => field;
		}

		/// <summary>
		/// <seealso cref="RefCache.CreateInstance(object[])"/>
		/// </summary>
		[Test]
		public void CreateInstancePasses()
		{
			var refCache = new RefCache(typeof(InstanceSample));
            {
				object instance = refCache.CreateInstance();
				Assert.AreEqual(typeof(InstanceSample), instance.GetType());
            }
			Debug.Log($"Success to CreateInstance()!");
            {
				var instance = refCache.CreateInstance(123) as InstanceSample;
				Assert.AreEqual(123, instance.GetField());
            }
			Debug.Log($"Success to CreateInstance(int)!");
			{
				var instance = refCache.CreateInstance("prop") as InstanceSample;
				Assert.AreEqual("prop", instance.Prop);
			}
			Debug.Log($"Success to CreateInstance(string)!");
			{
				var instance = refCache.CreateInstance(321, "prop") as InstanceSample;
				Assert.AreEqual(321, instance.GetField());
				Assert.AreEqual("prop", instance.Prop);
			}
			Debug.Log($"Success to CreateInstance(int, string)!");
		}

		/// <summary>
        /// <seealso cref="RefCache.ContainsCachedConstructorInfo(string)"/>
        /// <seealso cref="RefCache.FindAndCacheConstructor(string, bool, IEnumerable{System.Type})"/>
        /// <seealso cref="RefCache.GetCachedConstructorInfo(string)"/>
        /// <seealso cref="RefCache.CreateInstanceWithCache(string, object[])"/>
        /// </summary>
		[Test]
		public void ConstructorMethodsPasses()
		{
			var refCache = new RefCache(typeof(InstanceSample));
			var key = "args(int, string)";
			Assert.IsFalse(refCache.ContainsCachedConstructorInfo(key), $"Failed RefCache#ContainsCachedConstructorInfo...");

			var cstor = refCache.FindAndCacheConstructor(key, true, typeof(int), typeof(string));

			Assert.AreEqual(typeof(InstanceSample).GetConstructor(new System.Type[] { typeof(int), typeof(string) }), cstor, $"Failed RefCache#FindAndCacheConstructor...");
			Assert.IsTrue(refCache.ContainsCachedConstructorInfo(key), $"Failed RefCache#ContainsCachedConstructorInfo...");
			Assert.AreEqual(cstor, refCache.GetCachedConstructorInfo(key), $"Failed RefCache#GetConstructorInfo...");

			var inst = refCache.CreateInstanceWithCache(key, 432, "apple") as InstanceSample;
			Assert.IsNotNull(inst, $"Failed RefCache#CreateInstanceWithCache...");
			Assert.AreEqual(432, inst.GetField(), $"Failed RefCache#CreateInstanceWithCache...");
			Assert.AreEqual("apple", inst.Prop, $"Failed RefCache#CreateInstanceWithCache...");
		}

		/// <summary>
		/// <seealso cref="RefCache.FindAndCacheField(string, bool, bool)"/>
		/// <seealso cref="RefCache.GetField(object, string)"/>
		/// <seealso cref="RefCache.SetField(object, string, object)"/>
		/// <seealso cref="RefCache.ContainsCachedField(string)"/>
		/// </summary>
		[Test]
		public void FieldMethodsAtInstancePassess()
		{
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			string fieldName = "field";
			Assert.IsFalse(refCache.ContainsCachedField(fieldName), $"Failed RefCache#ContainsCachedField...Field Name={fieldName}");

			var field = refCache.GetField(instance, fieldName);
			Assert.AreEqual(1, field, $"Failed RefCache#GetField...");
			refCache.SetField(instance, fieldName, 100);
			Assert.AreEqual(100, refCache.GetField(instance, fieldName), $"Failed RefCache#SetField...");
			Assert.IsTrue(refCache.ContainsCachedField(fieldName), $"Failed RefCache#ContainsCachedField...Field Name={fieldName}");

			var bindFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			var fieldInfo = typeof(InstanceSample).GetField(fieldName, bindFlags);
			Assert.AreEqual(fieldInfo, refCache.FindAndCacheField(fieldName, true), $"Failed RefCache#FindField(isInstance=true, isReadOnly=false)...");
			Assert.AreEqual(fieldInfo, refCache.FindAndCacheField(fieldName, true), $"Failed RefCache#FindField(isInstance=true, isReadOnly=true)...");
		}

		/// <summary>
        /// <seealso cref="RefCache.FindAndCacheField(string, bool, bool)"/>
		/// <seealso cref="RefCache.GetField(object, string)"/>
		/// <seealso cref="RefCache.SetField(object, string, object)"/>
		/// <seealso cref="RefCache.ContainsCachedField(string)"/>
		/// </summary>
		[Test]
		public void LiteralFieldPassess()
		{
			var refCache = new RefCache(typeof(InstanceSample));

			string fieldName = "literalField";

			var instance = refCache.CreateInstance() as InstanceSample;
			var field = refCache.GetField(null, fieldName);
			Assert.AreEqual(InstanceSample.literalField, field, $"Failed RefCache#GetField...");

			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
				refCache.SetField(null, fieldName, 100);
			});
		}

		/// <summary>
		/// <seealso cref="RefCache.Invoke(object, string, object[])"/>
		/// <seealso cref="RefCache.ContainsCachedMethod(string, System.Type[])"/>
        /// <seealso cref="RefCache.GetCachedMethodInfo(string, System.Type[])"/>
		/// <seealso cref="RefCache.GetCachedMethodInfo(string, IEnumerable{System.Type})"/>
		/// <seealso cref="RefCache.FindAndCacheMethod(string, bool, System.Type[])"/>
        /// <seealso cref="RefCache.FindAndCacheMethod(string, bool, IEnumerable{System.Type})"/>
		/// </summary>
		[Test]
		public void MethodMethodsAtInstancePassess()
		{
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			string methodName = "Func";
			Assert.IsFalse(refCache.ContainsCachedMethod(methodName), $"Failed RefCache#ContainsCachedMethod... Method Name={methodName}");

			var result = refCache.Invoke(instance, methodName, 2, 3);
			Assert.AreEqual(5, result, $"Failed RefCache#Invoke...");
			var typeArgs = new System.Type[] { typeof(int), typeof(int) };
			Assert.IsTrue(refCache.ContainsCachedMethod(methodName, typeArgs), $"Failed RefCache#ContainsCachedMethod... Method Name={methodName}");

			var methodInfo = typeof(InstanceSample).GetMethod(methodName, typeArgs);
			Assert.AreEqual(
				methodInfo
				, refCache.GetCachedMethodInfo(methodName, typeArgs)
				, "Failed RefCache#GetMethod"); ; ;

			Assert.AreEqual(
				methodInfo
				, refCache.FindAndCacheMethod(methodName, true, typeArgs)
				, $"Failed RefCache#FindAndCacheMethod");
		}

		/// <summary>
        /// <seealso cref="RefCache.GetCachedMethodInfo(string)"/>
        /// <seealso cref="RefCache.GetCachedMethodInfo(string, System.Type[])"/>
        /// <seealso cref="RefCache.GetCachedMethodInfo(string, IEnumerable{System.Type})"/>
        /// <seealso cref="RefCache.FindAndCacheConstructor(string, bool, System.Type[])"/>
        /// <seealso cref="RefCache.FindAndCacheConstructor(string, bool, IEnumerable{System.Type})"/>
        /// <seealso cref="RefCache.ContainsCachedMethod(string, System.Type[])"/>
        /// </summary>
		[Test]
		public void OverloadMethodPasses()
		{
			var refCache = new RefCache(typeof(InstanceSample));

			string funcName = "OverloadFunc";
            {
				var typeArgs1 = new System.Type[] { typeof(int), typeof(string) };
				Assert.IsNull(refCache.GetCachedMethodInfo(funcName));
				Assert.IsNull(refCache.GetCachedMethodInfo(funcName, typeArgs1));
				Assert.IsFalse(refCache.ContainsCachedMethod(funcName, typeArgs1));

				var overloadMethod1 = refCache.FindAndCacheMethod(funcName, true, typeArgs1);
				var methodInfo1 = typeof(InstanceSample).GetMethod(funcName, typeArgs1);
				Assert.IsTrue(refCache.ContainsCachedMethod(funcName, typeArgs1));
				Assert.AreEqual(methodInfo1, overloadMethod1);
				Assert.AreEqual(overloadMethod1, refCache.GetCachedMethodInfo(funcName));
            }
			Debug.Log($"Success to Overload Func1");
            {
				var typeArgs2 = new System.Type[] { typeof(int) };
				Assert.IsNull(refCache.GetCachedMethodInfo(funcName, typeArgs2));
				Assert.IsFalse(refCache.ContainsCachedMethod(funcName, typeArgs2));

				var overloadMethod2 = refCache.FindAndCacheMethod(funcName, true, typeArgs2);
				var methodInfo2 = typeof(InstanceSample).GetMethod(funcName, typeArgs2);
				Assert.IsTrue(refCache.ContainsCachedMethod(funcName, typeArgs2));
				Assert.AreEqual(methodInfo2, overloadMethod2);
				Assert.AreNotEqual(overloadMethod2, refCache.GetCachedMethodInfo(funcName));
				Assert.AreEqual(overloadMethod2, refCache.GetCachedMethodInfo(funcName, typeArgs2));
            }
			Debug.Log($"Success to Overload Func2");
		}

		/// <summary>
        /// <seealso cref="RefCache.ContainsCachedProp(string)"/>
        /// <seealso cref="RefCache.GetProp(object, string)"/>
        /// <seealso cref="RefCache.SetProp(object, string, object)"/>
        /// <seealso cref="RefCache.FindAndCacheProp(string, bool, bool)"/>
        /// </summary>
		[Test]
		public void PropertyMethodsAtInstancePassess()
		{
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			string propName = "Prop";
			Assert.IsFalse(refCache.ContainsCachedProp(propName), $"Failed RefCache#ContainsCachedProp(getter)... Prop Name={propName}");

			refCache.SetProp(instance, propName, "test");
			var prop = (string)refCache.GetProp(instance, propName);
			Assert.AreEqual("test", prop, $"Failed RefCache#GetProp and RefCache#SetProp...");
			Assert.IsTrue(refCache.ContainsCachedProp(propName), $"Failed RefCache#ContainsCachedProp... Prop Name={propName}");

			Assert.AreEqual(typeof(InstanceSample).GetProperty(propName), refCache.FindAndCacheProp(propName, true), $"Failed RefCacheFindAndCacheProp...");
		}

		/// <summary>
		/// <seealso cref="RefCache.SetProp(object, string, object)"/>
		/// <seealso cref="RefCache.FindAndCacheProp(string, bool, bool)"/>
		/// </summary>
		[Test]
		public void GetterOnlyPropertyPasses()
        {
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			var propName = "GetterOnlyProp";
			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
				refCache.SetProp(instance, propName, 100);
			});
		}

		/// <summary>
		/// <seealso cref="RefCache.GetProp(object, string, object)"/>
		/// <seealso cref="RefCache.FindAndCacheProp(string, bool, bool)"/>
		/// </summary>
		[Test]
		public void SetterOnlyPropertyPasses()
		{
			var refCache = new RefCache(typeof(InstanceSample));
			object instance = refCache.CreateInstance();

			var propName = "SetterOnlyProp";
			Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
				refCache.GetProp(instance, propName);
			});
		}

		class StaticSample
		{
#pragma warning disable CS0414
            static int field = 1;
			public static int Func(int a, int b) { return a + b; }
			public static string Prop { get; set; }
#pragma warning restore CS0414
        }
		/// <summary>
		/// <seealso cref="RefCache.FindAndCacheField(string, bool, bool)"/>
		/// <seealso cref="RefCache.GetField(object, string)"/>
		/// <seealso cref="RefCache.SetField(object, string, object)"/>
		/// <seealso cref="RefCache.ContainsCachedField(string)"/>
		/// </summary>
		[Test]
		public void FieldMethodsAtStaticPassess()
		{
			var refCache = new RefCache(typeof(StaticSample));

			string fieldName = "field";
			var field = refCache.GetField(null, fieldName);
			Assert.AreEqual(1, field, $"Failed RefCache#GetField...");
			refCache.SetField(null, fieldName, 100);
			Assert.AreEqual(100, refCache.GetField(null, fieldName), $"Failed RefCache#SetField...");
			Assert.IsTrue(refCache.ContainsCachedField(fieldName), $"Failed RefCache#ContainsField... Field Name={fieldName}");
		}

		/// <summary>
		/// <seealso cref="RefCache.Invoke(object, string, object[])"/>
		/// <seealso cref="RefCache.ContainsCachedMethod(string, System.Type[])"/>
		/// <seealso cref="RefCache.GetCachedMethodInfo(string, System.Type[])"/>
		/// <seealso cref="RefCache.GetCachedMethodInfo(string, IEnumerable{System.Type})"/>
		/// <seealso cref="RefCache.FindAndCacheMethod(string, bool, System.Type[])"/>
		/// <seealso cref="RefCache.FindAndCacheMethod(string, bool, IEnumerable{System.Type})"/>
		/// </summary>
		[Test]
		public void MethodMethodsAtStaticPassess()
		{
			var refCache = new RefCache(typeof(StaticSample));

			string methodName = "Func";
			var result = refCache.Invoke(null, methodName, 2, 3);
			Assert.AreEqual(5, result, $"Failed RefCache#Invoke...");
			Assert.IsTrue(refCache.ContainsCachedMethod(methodName, typeof(int), typeof(int)), $"Failed RefCache#ContainsMethod... Method Name={methodName}");
		}

		/// <summary>
		/// <seealso cref="RefCache.ContainsCachedProp(string)"/>
		/// <seealso cref="RefCache.GetProp(object, string)"/>
		/// <seealso cref="RefCache.SetProp(object, string, object)"/>
		/// <seealso cref="RefCache.FindAndCacheProp(string, bool, bool)"/>
		/// </summary>
		[Test]
		public void PropertyMethodsAtStaticPassess()
		{
			var refCache = new RefCache(typeof(StaticSample));
			string propName = "Prop";
			refCache.SetProp(null, propName, "test");
			var prop = (string)refCache.GetProp(null, propName);
			Assert.AreEqual("test", prop, $"Failed RefCache#GetProp and RefCache#SetProp...");
			Assert.IsTrue(refCache.ContainsCachedProp(propName), $"Failed RefCache#ContainsProp... Method Name={propName}");
		}
	}
}
