using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// test case
    /// ## DoMatchReturnTypeAndArguments
    /// ## GetMatchArgsAndReturnType
    /// ## CallMethods
    /// <seealso cref="MethodInfoExtensions"/>
    /// </summary>
    public class TestMethodInfoExtensions
    {
        const int ORDER_DoMatchReturnTypeAndArguments = 0;
        const int ORDER_GetMatchArgsAndReturnType = ORDER_DoMatchReturnTypeAndArguments + 100;
        const int ORDER_CallMethods = ORDER_GetMatchArgsAndReturnType + 100;

        #region DoMatchReturnTypeAndArguments
        class DoMatchReturnTypeAndArgumentsTest
        {
            public void Func1() { }
            public int Func2() { return 0; }
            public void Func3(int a) { }
            public string Func4(int a) => a.ToString();

            public B Func5(I a) => new B();

            public interface I { }
            public class A : I { }
            public class B : A { }
        }

        /// <summary>
        /// <seealso cref="MethodInfoExtensions.DoMatchReturnTypeAndArguments(MethodInfo, System.Type, IEnumerable{System.Type})"/>
        /// <seealso cref="MethodInfoExtensions.DoMatchReturnTypeAndArguments(MethodInfo, System.Type, System.Type[])"/>
        /// </summary>
        [Test, Order(ORDER_DoMatchReturnTypeAndArguments), Description("")]
        public void DoMatchReturnTypeAndArguments_Passes()
        {
            var type = typeof(DoMatchReturnTypeAndArgumentsTest);

            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(type.GetMethod("Func1").DoMatchReturnTypeAndArguments(typeof(void)));
            Assert.IsTrue(type.GetMethod("Func2").DoMatchReturnTypeAndArguments(typeof(int)));
            Assert.IsTrue(type.GetMethod("Func3").DoMatchReturnTypeAndArguments(typeof(void), typeof(int)));
            Assert.IsTrue(type.GetMethod("Func4").DoMatchReturnTypeAndArguments(typeof(string), typeof(int)));

            Assert.IsTrue(type.GetMethod("Func5").DoMatchReturnTypeAndArguments(typeof(DoMatchReturnTypeAndArgumentsTest.B), typeof(DoMatchReturnTypeAndArgumentsTest. B)));
            Assert.IsTrue(type.GetMethod("Func5").DoMatchReturnTypeAndArguments(typeof(DoMatchReturnTypeAndArgumentsTest.B), typeof(DoMatchReturnTypeAndArgumentsTest.A)));
            Assert.IsTrue(type.GetMethod("Func5").DoMatchReturnTypeAndArguments(typeof(DoMatchReturnTypeAndArgumentsTest.B), typeof(DoMatchReturnTypeAndArgumentsTest.I)));

            Assert.IsFalse(type.GetMethod("Func1").DoMatchReturnTypeAndArguments(typeof(int)));
            Assert.IsFalse(type.GetMethod("Func1").DoMatchReturnTypeAndArguments(typeof(void), typeof(int)));
            Assert.IsFalse(type.GetMethod("Func4").DoMatchReturnTypeAndArguments(typeof(void), typeof(int)));
            Assert.IsFalse(type.GetMethod("Func4").DoMatchReturnTypeAndArguments(typeof(string), typeof(float)));
            Assert.IsFalse(type.GetMethod("Func5").DoMatchReturnTypeAndArguments(typeof(DoMatchReturnTypeAndArgumentsTest.A), typeof(DoMatchReturnTypeAndArgumentsTest.B)));
            Assert.IsFalse(type.GetMethod("Func5").DoMatchReturnTypeAndArguments(typeof(DoMatchReturnTypeAndArgumentsTest.I), typeof(DoMatchReturnTypeAndArgumentsTest.B)));
        }
        #endregion

        #region GetMatchArgsAndReturnType
        class GetMatchArgsAndReturnTypeTest
        {
            public void Func1() { }
            public int Func2() { return 0; }
            public void Func3(int a) { }
            public string Func4(int a) => a.ToString();

            public B Func5(I a) => new B();

            public interface I { }
            public class A : I { }
            public class B : A { }
        }

        /// <summary>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, System.Type[])"/>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, IEnumerable{System.Type})"/>
        /// </summary>
        [Test, Order(ORDER_GetMatchArgsAndReturnType), Description("")]
        public void GetMatchArgsAndReturnType_Passes()
        {
            var type = typeof(GetMatchArgsAndReturnTypeTest);

            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            AssertionUtils.AssertEnumerableByUnordered(
                new MethodInfo[]
                {
                    type.GetMethod("Func1")
                }
                , methods.GetMatchArgsAndReturnType(typeof(void))
                , ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new MethodInfo[]
                {
                    type.GetMethod("Func2")
                }
                , methods.GetMatchArgsAndReturnType(typeof(int))
                , ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new MethodInfo[]
                {
                    type.GetMethod("Func3")
                }
                , methods.GetMatchArgsAndReturnType(typeof(void), typeof(int))
                , ""
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new MethodInfo[]
                {
                    type.GetMethod("Func4")
                }
                , methods.GetMatchArgsAndReturnType(typeof(string), typeof(int))
                , ""
            );

            AssertionUtils.AssertEnumerableByUnordered(
                new MethodInfo[] {}
                , methods.GetMatchArgsAndReturnType(typeof(void), typeof(string), typeof(string))
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, System.Type[])"/>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, IEnumerable{System.Type})"/>
        /// </summary>
        [Test, Order(ORDER_GetMatchArgsAndReturnType), Description("Check Inherited type")]
        public void GetMatchArgsAndReturnType_InheritedType_Passes()
        {
            var type = typeof(GetMatchArgsAndReturnTypeTest);

            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            {
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[]
                    {
                        type.GetMethod("Func5")
                    }
                    , methods.GetMatchArgsAndReturnType(typeof(GetMatchArgsAndReturnTypeTest.B), typeof(GetMatchArgsAndReturnTypeTest.A))
                    , "Fail Full Match Type..."
                );

                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[]
                    {
                        type.GetMethod("Func5")
                    }
                    , methods.GetMatchArgsAndReturnType(typeof(GetMatchArgsAndReturnTypeTest.B), typeof(GetMatchArgsAndReturnTypeTest.B))
                    , "Fail Inherited Type..."
                );

                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[]
                    {
                        type.GetMethod("Func5")
                    }
                    , methods.GetMatchArgsAndReturnType(typeof(GetMatchArgsAndReturnTypeTest.B), typeof(GetMatchArgsAndReturnTypeTest.I))
                    , "Fail Interface Type..."
                );
            }
            Logger.Log(Logger.Priority.High, () => $"Success Enable Case!");

            {
                AssertionUtils.AssertEnumerableByUnordered(
                    new MethodInfo[] { }
                    , methods.GetMatchArgsAndReturnType(typeof(GetMatchArgsAndReturnTypeTest.A), typeof(GetMatchArgsAndReturnTypeTest.A))
                    , "Fail ..."
                );
            }
            Logger.Log(Logger.Priority.High, () => $"Success Disable Case!");
        }

        #endregion

        #region CallMethods
        class CallMethodsTest
        {
            public void Func1() { }
            public int Func2() { return 0; }
            public void Func3(int a) { }
            public string Func4(int a) => a.ToString();
        }

        /// <summary>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, System.Type[])"/>
        /// <seealso cref="MethodInfoExtensions.GetMatchArgsAndReturnType(IEnumerable{MethodInfo}, System.Type, IEnumerable{System.Type})"/>
        /// </summary>
        [Test, Order(ORDER_CallMethods), Description("")]
        public void CallMethods_Passes()
        {
            var inst = new CallMethodsTest();
            var type = inst.GetType();

            var methods = type.GetMethods()
                .CallMethods(inst, typeof(string), 100);
            AssertionUtils.AssertEnumerableByUnordered(
                new object[] { "100" }
                , methods
                , ""
            );
        }
        #endregion
    }
}
