using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using System.Linq;

namespace Hinode.Tests.Attributes
{
    /// <summary>
    /// <seealso cref="MethodLabelAttribute"/>
    /// </summary>
    public class TestMethodLabelAttribute
    {
        const int ORDER_CONTAINS = 0;
        const int ORDER_GET_METHOD_INFOS = ORDER_CONTAINS + 100;
        const int ORDER_GET_METHOD_INFOS_WITH_RETURN_TYPE_AND_ARG_TYPES = ORDER_GET_METHOD_INFOS + 100;
        const int ORDER_CALL_METHODS = ORDER_GET_METHOD_INFOS_WITH_RETURN_TYPE_AND_ARG_TYPES + 100;

        const string LABEL_APPLE = "Apple";
        const string LABEL_ORANGE = "Orange";

        class A
        {
            public int Value { get; set; }
        }
        class B : A
        { }

        class Fruits
        {
            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod(int n)
            {
                return n + 1;
            }

            [MethodLabel(LABEL_APPLE)]
            public static int StaticMethod2(int n)
            {
                return n + 2;
            }

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method(int n)
            {
                return n + 1;
            }

            [MethodLabel(LABEL_APPLE)]
            public int Method2(int n)
            {
                return n+2;
            }

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            int NonPublicMethod(int n)
            {
                return n + 1;
            }
        }

        #region Contains
        [Test, Order(ORDER_CONTAINS), Description("MethodLabelAttribute#Contains()のテスト")]
        public void Contains_Passes()
        {
            {
                var methodInfo = typeof(Fruits).GetMethod("Method");
                var labelAttr = methodInfo.GetCustomAttribute<MethodLabelAttribute>();
                Assert.IsTrue(labelAttr.Contains(LABEL_APPLE));
                Assert.IsTrue(labelAttr.Contains(LABEL_ORANGE));
                Assert.IsFalse(labelAttr.Contains("__hoge"));
            }

            {
                var methodInfo = typeof(Fruits).GetMethod("Method2");
                var labelAttr = methodInfo.GetCustomAttribute<MethodLabelAttribute>();
                Assert.IsTrue(labelAttr.Contains(LABEL_APPLE));
                Assert.IsFalse(labelAttr.Contains(LABEL_ORANGE));
                Assert.IsFalse(labelAttr.Contains("__hoge"));
            }
        }
        #endregion

        #region GetMethodInfos
        /// <summary>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos(System.Type, string, bool)"/>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos{T}(string, bool)"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS), Description("GetMethodInfos()のInstanceメソッドの取得のテスト")]
        public void GetMethodInfos_Instance_Passes()
        {
            {//Apple
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>(LABEL_APPLE, false);
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        typeof(Fruits).GetMethod("Method"),
                        typeof(Fruits).GetMethod("Method2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//Orange
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>(LABEL_ORANGE, false);
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        typeof(Fruits).GetMethod("Method"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {// other labels...
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>("__hoge__", false);
                Assert.AreEqual(0, methods.Count());
            }
        }

        /// <summary>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos(System.Type, string, bool)"/>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos{T}(string, bool)"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS), Description("GetMethodInfos()のStaticメソッドの取得のテスト")]
        public void GetMethodInfos_Static_Passes()
        {
            {//Apple
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>(LABEL_APPLE, true);
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        typeof(Fruits).GetMethod("StaticMethod"),
                        typeof(Fruits).GetMethod("StaticMethod2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//Orange
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>(LABEL_ORANGE, true);
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        typeof(Fruits).GetMethod("StaticMethod"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {// other labels...
                var methods = MethodLabelAttribute.GetMethodInfos<Fruits>("__hoge__", true);
                Assert.AreEqual(0, methods.Count());
            }
        }
        #endregion

        #region GetMethodInfosWithArgsAndReturnType

        class Fruits2
        {
            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod(int n) => n + 1;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static A StaticMethod_1(int n) => new A() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static B StaticMethod_2(int n) => new B() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod_3(int n, int m) => n + m;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod_4(string n) => n.Length;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method(int n) => n + 1;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public A Method_1(int n) => new A() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public B Method_2(int n) => new B() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method_3(int n, int m) => n + m;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method_4(string n) => n.Length;
        }

        /// <summary>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType(System.Type, string, bool, System.Type, IEnumerable{System.Type})"/>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType{T, TReturnType}(string, bool, IEnumerable{System.Type})"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS_WITH_RETURN_TYPE_AND_ARG_TYPES), Description("GetMethodInfos()のInstanceメソッドの取得のテスト")]
        public void GetMethodInfosWithArgsAndReturnType_Instance_Passes()
        {
            var classType = typeof(Fruits2);
            var onlyInstance = false;
            {//int XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyInstance, new System.Type[] {
                    typeof(int)
                });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("Method"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//A XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, A>(LABEL_APPLE, onlyInstance, new System.Type[] { typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("Method_1"),
                        classType.GetMethod("Method_2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//B XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, B>(LABEL_APPLE, onlyInstance, new System.Type[] { typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("Method_2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//int XXX(int, int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyInstance, new System.Type[] { typeof(int), typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("Method_3"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//int XXX(string)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyInstance, new System.Type[] { typeof(string) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("Method_4"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {// other labels...
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>("__hoge__", onlyInstance, new System.Type[] { typeof(int) });
                Assert.AreEqual(0, methods.Count());
            }
        }

        /// <summary>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos(System.Type, string, bool)"/>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos{T}(string, bool)"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS_WITH_RETURN_TYPE_AND_ARG_TYPES), Description("GetMethodInfos()のStaticメソッドの取得のテスト")]
        public void GetMethodInfosWithArgsAndReturnType_Static_Passes()
        {
            var classType = typeof(Fruits2);
            var onlyStatic = true;
            {//int XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyStatic, new System.Type[] {
                    typeof(int)
                });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("StaticMethod"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//A XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, A>(LABEL_APPLE, onlyStatic, new System.Type[] { typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("StaticMethod_1"),
                        classType.GetMethod("StaticMethod_2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//B XXX(int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, B>(LABEL_APPLE, onlyStatic, new System.Type[] { typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("StaticMethod_2"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//int XXX(int, int)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyStatic, new System.Type[] { typeof(int), typeof(int) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("StaticMethod_3"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {//int XXX(string)
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>(LABEL_APPLE, onlyStatic, new System.Type[] { typeof(string) });
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] {
                        classType.GetMethod("StaticMethod_4"),
                    }
                    , methods.Select(_t => _t.methodInfo)
                    , ""
                );
            }

            {// other labels...
                var methods = MethodLabelAttribute.GetMethodInfosWithArgsAndReturnType<Fruits2, int>("__hoge__", onlyStatic, new System.Type[] { typeof(int) });
                Assert.AreEqual(0, methods.Count());
            }
        }
        #endregion


        #region CallMethods
        class Fruits3
        {
            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod(int n) => n + 1;

            [MethodLabel(LABEL_ORANGE)]
            public static int StaticMethod2(int n) => n * 10;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static A StaticMethod_1(int n) => new A() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static B StaticMethod_2(int n) => new B() { Value = n * -1 };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod_3(int n, int m) => n + m;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public static int StaticMethod_4(string n) => n.Length;



            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method(int n) => n + 1;

            [MethodLabel(LABEL_ORANGE)]
            public int Method2(int n) => n * 10;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public A Method_1(int n) => new A() { Value = n };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public B Method_2(int n) => new B() { Value = n * -1 };

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method_3(int n, int m) => n + m;

            [MethodLabel(LABEL_APPLE, LABEL_ORANGE)]
            public int Method_4(string n) => n.Length;
        }

        /// <summary>
        /// <seealso cref="MethodLabelAttribute.CallMethods{TReturnType}(object, string, bool, object[])"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS), Description("GetMethodInfos()のInstanceメソッドの取得のテスト")]
        public void CallMethods_Instance_Passes()
        {
            bool isStatic = false;
            {//label Apple: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        inst.Method(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {//label Orange: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_ORANGE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        inst.Method(arg),
                        inst.Method2(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {// other labels...: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, "__hoge__", isStatic, arg);
                Assert.AreEqual(0, returnValues.Count());
            }

            {//label Apple: A XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<A>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new A[] {
                        inst.Method_1(arg),
                        inst.Method_2(arg),
                    }
                    , returnValues.OfType<A>()
                    , ""
                    , (_c, _g) => _c.Value == _g.Value
                );
            }

            {//label Apple: B XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<B>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new B[] {
                        inst.Method_2(arg),
                    }
                    , returnValues.OfType<B>()
                    , ""
                    , (_c, _g) => _c.Value == _g.Value
                );
            }

            {//label Apple: int XXX(int, int)
                var inst = new Fruits3();
                var arg = 100;
                var arg2 = 200;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg, arg2);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        inst.Method_3(arg, arg2),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {//label Apple: int XXX(string)
                var inst = new Fruits3();
                var arg = "apple";
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        inst.Method_4(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }
        }

        /// <summary>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos(System.Type, string, bool)"/>
        /// <seealso cref="MethodLabelAttribute.GetMethodInfos{T}(string, bool)"/>
        /// </summary>
        [Test, Order(ORDER_GET_METHOD_INFOS), Description("GetMethodInfos()のStaticメソッドの取得のテスト")]
        public void CallMethods_Static_Passes()
        {
            bool isStatic = true;
            {//label Apple: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        Fruits3.StaticMethod(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {//label Orange: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_ORANGE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        Fruits3.StaticMethod(arg),
                        Fruits3.StaticMethod2(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {// other labels...: int XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, "__hoge__", isStatic, arg);
                Assert.AreEqual(0, returnValues.Count());
            }

            {//label Apple: A XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<A>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new A[] {
                        Fruits3.StaticMethod_1(arg),
                        Fruits3.StaticMethod_2(arg),
                    }
                    , returnValues.OfType<A>()
                    , ""
                    , (_c, _g) => _c.Value == _g.Value
                );
            }

            {//label Apple: B XXX(int)
                var inst = new Fruits3();
                var arg = 100;
                var returnValues = MethodLabelAttribute.CallMethods<B>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new B[] {
                        Fruits3.StaticMethod_2(arg),
                    }
                    , returnValues.OfType<B>()
                    , ""
                    , (_c, _g) => _c.Value == _g.Value
                );
            }

            {//label Apple: int XXX(int, int)
                var inst = new Fruits3();
                var arg = 100;
                var arg2 = 200;
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg, arg2);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        Fruits3.StaticMethod_3(arg, arg2),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }

            {//label Apple: int XXX(string)
                var inst = new Fruits3();
                var arg = "apple";
                var returnValues = MethodLabelAttribute.CallMethods<int>(inst, LABEL_APPLE, isStatic, arg);
                AssertionUtils.AssertEnumerableByUnordered(
                    new int[] {
                        Fruits3.StaticMethod_4(arg),
                    }
                    , returnValues.OfType<int>()
                    , ""
                );
            }
        }
        #endregion

    }
}
