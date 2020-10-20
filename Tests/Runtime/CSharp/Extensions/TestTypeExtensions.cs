using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="TypeExtensions"/>
    /// </summary>
    public class TestTypeExtensions
    {
        const int ORDER_EQUAL_GENERIC_TYPE_DEFINITION = 0;

        interface IsSameOrInheritedTypePassesInterface { }
        class IsSameOrInheritedTypePassesBaseClass { }
        class IsSameOrInheritedTypePassesClass
            : IsSameOrInheritedTypePassesBaseClass
            , IsSameOrInheritedTypePassesInterface
        { }
        class IsSameOrInheritedTypePassesOtherClass { }

        /// <summary>
        /// <seealso cref="TypeExtensions.IsSameOrInheritedType(System.Type, System.Type)"/>
        /// <seealso cref="TypeExtensions.IsSameOrInheritedType{T}(System.Type)"/>
        /// </summary>
        [Test]
        public void IsSameOrInheritedTypePasses()
        {
            Assert.IsTrue(typeof(IsSameOrInheritedTypePassesClass).IsSameOrInheritedType<IsSameOrInheritedTypePassesClass>());
            Assert.IsTrue(typeof(IsSameOrInheritedTypePassesClass).IsSameOrInheritedType<IsSameOrInheritedTypePassesBaseClass>());
            Assert.IsTrue(typeof(IsSameOrInheritedTypePassesClass).IsSameOrInheritedType<IsSameOrInheritedTypePassesInterface>());

            Assert.IsFalse(typeof(IsSameOrInheritedTypePassesClass).IsSameOrInheritedType<IsSameOrInheritedTypePassesOtherClass>());
            Assert.IsFalse(typeof(IsSameOrInheritedTypePassesBaseClass).IsSameOrInheritedType<IsSameOrInheritedTypePassesClass>());
            Assert.IsFalse(typeof(IsSameOrInheritedTypePassesInterface).IsSameOrInheritedType<IsSameOrInheritedTypePassesClass>());

        }

        interface HasInterfacePassesInterface
        {
            void Func();
        }

        interface HasInterfacePassesInterface2
        {
            void Func();
        }


        class HasInterfacePassesClass : HasInterfacePassesInterface
        {
            public void Func() { }
        }

        /// <summary>
        /// <seealso cref="TypeExtensions.ContainsInterface(System.Type, System.Type)"/>
        /// <seealso cref="TypeExtensions.ContainsInterface{InterfaceType}(System.Type)"/>
        /// </summary>
        [Test]
        public void ContainsInterfacePasses()
        {
            Assert.IsTrue(typeof(HasInterfacePassesClass).ContainsInterface(typeof(HasInterfacePassesInterface)));
            Assert.IsTrue(typeof(HasInterfacePassesClass).ContainsInterface<HasInterfacePassesInterface>());

            Assert.IsFalse(typeof(HasInterfacePassesClass).ContainsInterface<HasInterfacePassesInterface2>());
            Assert.IsFalse(typeof(HasInterfacePassesClass).ContainsInterface<TestTypeExtensions>());
        }

        class GetFieldInHierarchyBaseClass : ScriptableObject
        {
#pragma warning disable CS0649, CS0414
            int value = 1;
#pragma warning restore CS0649, CS0414
        }
        class GetFieldInHierarchySubClass : GetFieldInHierarchyBaseClass
        {
#pragma warning disable CS0649, CS0414
            public int apple = 10;
#pragma warning restore CS0649, CS0414
        }

        /// <summary>
        /// <seealso cref="TypeExtensions.GetFieldInHierarchy(System.Type, string, BindingFlags)"/>
        /// </summary>
        [Test]
        public void GetFieldInHierarchyPasses()
        {
            Assert.AreEqual(typeof(GetFieldInHierarchyBaseClass).GetField("value", BindingFlags.NonPublic),
                typeof(GetFieldInHierarchySubClass).GetFieldInHierarchy("value", BindingFlags.NonPublic));
            // Use the Assert class to test conditions
        }

        class InheritedList : List<int> {}

        /// <summary>
        /// <seealso cref="TypeExtensions.IsArrayOrList(System.Type)"/>
        /// </summary>
        [Test]
        public void IsArrayOrListPasses()
        {
            Assert.IsTrue(typeof(int[]).IsArrayOrList());
            Assert.IsTrue(typeof(System.Array).IsArrayOrList());
            Assert.IsTrue(typeof(ArrayList).IsArrayOrList());
            Assert.IsTrue(typeof(List<>).IsArrayOrList());
            Assert.IsTrue(typeof(List<int>).IsArrayOrList());
            Assert.IsTrue(typeof(InheritedList).IsArrayOrList());

            Assert.IsFalse(typeof(int).IsArrayOrList());
            Assert.IsFalse(typeof(System.Enum).IsArrayOrList());
            Assert.IsFalse(typeof(Dictionary<string, int>).IsArrayOrList());
        }

        /// <summary>
        /// <seealso cref="TypeExtensions.GetArrayElementType(System.Type)"/>
        /// </summary>
        [Test]
        public void GetArrayElementTypePasses()
        {
            Assert.AreEqual(typeof(int), typeof(int[]).GetArrayElementType());
            Assert.AreEqual(typeof(object), typeof(System.Array).GetArrayElementType());
            Assert.AreEqual(typeof(object), typeof(ArrayList).GetArrayElementType());
            Assert.AreEqual(typeof(List<>).GetGenericArguments()[0], typeof(List<>).GetArrayElementType());
            Assert.AreEqual(typeof(string), typeof(List<string>).GetArrayElementType());
            Assert.AreEqual(typeof(int), typeof(InheritedList).GetArrayElementType());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(int).GetArrayElementType());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(System.Enum).GetArrayElementType());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(Dictionary<string, int>).GetArrayElementType());
        }

        class ListEx : List<int>
        { }
        /// <summary>
        /// <seealso cref="TypeExtensions.EqualGenericTypeDefinition(System.Type, System.Type)"/>
        /// </summary>
        [Test, Order(ORDER_EQUAL_GENERIC_TYPE_DEFINITION), Description("Genericを含む型の判定のテスト")]
        public void EqualGenericTypeDefinition_Passes()
        {
            {
                var baseType = typeof(List<>);
                var type = typeof(List<int>);
                Assert.IsTrue(type.EqualGenericTypeDefinition(baseType));
                Assert.IsTrue(baseType.EqualGenericTypeDefinition(type));

                Assert.IsTrue(typeof(List<string>).EqualGenericTypeDefinition(typeof(List<int>)));
                Assert.IsTrue(typeof(int).EqualGenericTypeDefinition(typeof(int)));

                Assert.IsFalse(typeof(int).EqualGenericTypeDefinition(typeof(double)));
                Assert.IsFalse(typeof(List<int>).EqualGenericTypeDefinition(typeof(HashSet<int>)));
            }

            {//Inherited Type
                var baseType = typeof(List<>);
                var type = typeof(ListEx);
                Assert.IsTrue(type.EqualGenericTypeDefinition(baseType));
                Assert.IsTrue(baseType.EqualGenericTypeDefinition(type));
            }
        }

        interface Interface<T> { }
        interface TestInterface : Interface<int> { }
        interface OtherInterface<T> { }
        /// <summary>
        /// <seealso cref="TypeExtensions.EqualGenericTypeDefinition(System.Type, System.Type)"/>
        /// </summary>
        [Test, Order(ORDER_EQUAL_GENERIC_TYPE_DEFINITION), Description("Interfaceのチェック")]
        public void EqualGenericTypeDefinition_Interface_Passes()
        {
            {
                var baseType = typeof(Interface<>);
                var type = typeof(Interface<int>);
                Assert.IsTrue(type.EqualGenericTypeDefinition(baseType));
                Assert.IsTrue(baseType.EqualGenericTypeDefinition(type));

                Assert.IsTrue(typeof(Interface<string>).EqualGenericTypeDefinition(typeof(Interface<int>)));

                Assert.IsFalse(typeof(Interface<int>).EqualGenericTypeDefinition(typeof(OtherInterface<int>)));
            }

            {//Inherited Type
                var baseType = typeof(Interface<>);
                var type = typeof(TestInterface);
                Assert.IsTrue(type.EqualGenericTypeDefinition(baseType));
                Assert.IsTrue(baseType.EqualGenericTypeDefinition(type));
            }
        }

        /// <summary>
        /// 整数型かどうか確認するための拡張メソッドのテスト
        /// <seealso cref="TypeExtensions.IsInteger(System.Type)"/>
        /// </summary>
        [Test, Description("整数型かどうか確認するための拡張メソッドのテスト")]
        public void IsIntegerPasses()
        {
            Assert.IsTrue(typeof(byte).IsInteger());
            Assert.IsTrue(typeof(sbyte).IsInteger());
            Assert.IsTrue(typeof(short).IsInteger());
            Assert.IsTrue(typeof(ushort).IsInteger());
            Assert.IsTrue(typeof(int).IsInteger());
            Assert.IsTrue(typeof(uint).IsInteger());
            Assert.IsTrue(typeof(long).IsInteger());
            Assert.IsTrue(typeof(ulong).IsInteger());
            Assert.IsTrue(typeof(BigInteger).IsInteger());
            Assert.IsFalse(typeof(float).IsInteger());
            Assert.IsFalse(typeof(double).IsInteger());
            Assert.IsFalse(typeof(decimal).IsInteger());
        }

        /// <summary>
        /// 浮動小数点型かどうか確認するための拡張メソッドのテスト
        /// <seealso cref="TypeExtensions.IsFloat(System.Type)"/>
        /// </summary>
        [Test, Description("浮動小数点型かどうか確認するための拡張メソッドのテスト")]
        public void IsFloatPasses()
        {
            Assert.IsFalse(typeof(byte).IsFloat());
            Assert.IsFalse(typeof(sbyte).IsFloat());
            Assert.IsFalse(typeof(short).IsFloat());
            Assert.IsFalse(typeof(ushort).IsFloat());
            Assert.IsFalse(typeof(int).IsFloat());
            Assert.IsFalse(typeof(uint).IsFloat());
            Assert.IsFalse(typeof(long).IsFloat());
            Assert.IsFalse(typeof(ulong).IsFloat());
            Assert.IsFalse(typeof(BigInteger).IsFloat());
            Assert.IsTrue(typeof(float).IsFloat());
            Assert.IsTrue(typeof(double).IsFloat());
            Assert.IsTrue(typeof(decimal).IsFloat());
        }

        /// <summary>
        /// 数値型かどうか確認するための拡張メソッドのテスト
        /// <seealso cref="TypeExtensions.IsNumeric(System.Type)"/>
        /// </summary>
        [Test, Description("数値型かどうか確認するための拡張メソッドのテスト")]
        public void IsNumericPasses()
        {
            Assert.IsTrue(typeof(byte).IsNumeric());
            Assert.IsTrue(typeof(sbyte).IsNumeric());
            Assert.IsTrue(typeof(short).IsNumeric());
            Assert.IsTrue(typeof(ushort).IsNumeric());
            Assert.IsTrue(typeof(int).IsNumeric());
            Assert.IsTrue(typeof(uint).IsNumeric());
            Assert.IsTrue(typeof(long).IsNumeric());
            Assert.IsTrue(typeof(ulong).IsNumeric());
            Assert.IsTrue(typeof(BigInteger).IsNumeric());
            Assert.IsTrue(typeof(float).IsNumeric());
            Assert.IsTrue(typeof(double).IsNumeric());
            Assert.IsTrue(typeof(decimal).IsNumeric());
        }

        struct IsStructPassesStruct { }
        enum IsStructPassesEnum { }

        /// <summary>
        /// <seealso cref="TypeExtensions.IsStruct(System.Type)"/>
        /// </summary>
        [Test, Description("struct型かどうか確認するための拡張メソッドのテスト")]
        public void IsStructPasses()
        {
            Assert.IsTrue(typeof(IsStructPassesStruct).IsStruct());
            Assert.IsFalse(typeof(int).IsStruct());
            Assert.IsFalse(typeof(string).IsStruct());
            Assert.IsFalse(typeof(IsStructPassesEnum).IsStruct());
            Assert.IsFalse(typeof(System.Action).IsStruct());
        }

        class ParseToNumberPassesClass
        {
        }

        enum ParseToNumberPassesEnum : byte
        {
            A, B, C
        }

        /// <summary>
        /// <seealso cref="TypeExtensions.ParseToNumber(System.Type, string)"/>
        /// </summary>
        [Test, Description("数値を表す文字列を型に合わせた値に変換するメソッドのテスト")]
        public void ParseToNumberPasses()
        {
            Assert.AreEqual(typeof(sbyte), typeof(sbyte).ParseToNumber("-1").GetType());
            Assert.AreEqual(typeof(byte), typeof(byte).ParseToNumber("1").GetType());
            Assert.AreEqual(typeof(short), typeof(short).ParseToNumber("-100").GetType());
            Assert.AreEqual(typeof(ushort), typeof(ushort).ParseToNumber("100").GetType());
            Assert.AreEqual(typeof(int), typeof(int).ParseToNumber("-1000").GetType());
            Assert.AreEqual(typeof(uint), typeof(uint).ParseToNumber("1000").GetType());
            Assert.AreEqual(typeof(long), typeof(long).ParseToNumber("-111").GetType());
            Assert.AreEqual(typeof(ulong), typeof(ulong).ParseToNumber("111").GetType());
            Assert.AreEqual(typeof(BigInteger), typeof(BigInteger).ParseToNumber("123456789").GetType());
            Assert.AreEqual(typeof(float), typeof(float).ParseToNumber("1.23").GetType());
            Assert.AreEqual(typeof(double), typeof(double).ParseToNumber("1.45678").GetType());
            Assert.AreEqual(typeof(decimal), typeof(decimal).ParseToNumber("1.4567891234").GetType());

            Assert.AreEqual(typeof(byte), typeof(ParseToNumberPassesEnum).ParseToNumber("1").GetType());

            Assert.Throws<System.NotSupportedException>(() => { typeof(string).ParseToNumber("1"); });
            Assert.Throws<System.NotSupportedException>(() => { typeof(ParseToNumberPassesClass).ParseToNumber("1"); });
        }

        /// <summary>
        /// <seealso cref="TypeExtensions.TryParseToNumber(System.Type, string, out object)"/>
        /// </summary>
        [Test, Description("数値を表す文字列を型に合わせた値に変換するメソッドのテスト")]
        public void TryParseToNumber()
        {
            Assert.IsTrue(typeof(sbyte).TryParseToNumber("-1", out var _sbyte));
            Assert.AreEqual(typeof(sbyte), _sbyte.GetType());
            Assert.IsTrue(typeof(byte).TryParseToNumber("1", out var _byte));
            Assert.AreEqual(typeof(byte), _byte.GetType());
            Assert.IsTrue(typeof(short).TryParseToNumber("-10", out var _short));
            Assert.AreEqual(typeof(short), _short.GetType());
            Assert.IsTrue(typeof(ushort).TryParseToNumber("10", out var _ushort));
            Assert.AreEqual(typeof(ushort), _ushort.GetType());
            Assert.IsTrue(typeof(int).TryParseToNumber("-100", out var _int));
            Assert.AreEqual(typeof(int), _int.GetType());
            Assert.IsTrue(typeof(uint).TryParseToNumber("100", out var _uint));
            Assert.AreEqual(typeof(uint), _uint.GetType());
            Assert.IsTrue(typeof(long).TryParseToNumber("-1000", out var _long));
            Assert.AreEqual(typeof(long), _long.GetType());
            Assert.IsTrue(typeof(ulong).TryParseToNumber("1000", out var _ulong));
            Assert.AreEqual(typeof(ulong), _ulong.GetType());
            Assert.IsTrue(typeof(BigInteger).TryParseToNumber("123456789", out var _bigInt));
            Assert.AreEqual(typeof(BigInteger), _bigInt.GetType());
            Assert.IsTrue(typeof(float).TryParseToNumber("-1.234", out var _float));
            Assert.AreEqual(typeof(float), _float.GetType());
            Assert.IsTrue(typeof(double).TryParseToNumber("-1.23456789", out var _double));
            Assert.AreEqual(typeof(double), _double.GetType());
            Assert.IsTrue(typeof(decimal).TryParseToNumber("-1.23456789876543", out var _decimal));
            Assert.AreEqual(typeof(decimal), _decimal.GetType());

            Assert.IsTrue(typeof(byte).TryParseToNumber("1", out var _enum));
            Assert.AreEqual(typeof(byte), _enum.GetType());

            Assert.IsFalse(typeof(string).TryParseToNumber("1", out var _str));
            Assert.IsNull(_str);
            Assert.IsFalse(typeof(ParseToNumberPassesClass).TryParseToNumber("1", out var _class));
            Assert.IsNull(_class);
        }

        class ClassHierarchyBaseClass { }
        class ClassHierarchyChildClass : ClassHierarchyBaseClass { }
        class ClassHierarchyChild2Class : ClassHierarchyChildClass { }

        /// <summary>
        /// <seealso cref="TypeExtensions.GetClassHierarchyEnumerable(System.Type)"/>
        /// </summary>
        [Test]
        public void GetClassHierarchyEnumerablePasses()
        {
            AssertionUtils.AssertEnumerable(
                new System.Type[] {
                    typeof(ClassHierarchyChild2Class),
                    typeof(ClassHierarchyChildClass),
                    typeof(ClassHierarchyBaseClass),
                    typeof(System.Object),
                }
                , typeof(ClassHierarchyChild2Class).GetClassHierarchyEnumerable()
                , ""
            );
        }
    }
}
