﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="System.Type"/>
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions"/>
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 同じまたは派生元の型かどうか？
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.IsSameOrInheritedTypePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSameOrInheritedType(this System.Type t, System.Type type)
            => t.Equals(type)
            || t.IsSubclassOf(type)
            || t.ContainsInterface(type);

        /// <summary>
        /// 同じまたは派生元の型かどうか？
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.IsSameOrInheritedTypePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSameOrInheritedType<T>(this System.Type t)
            => t.IsSameOrInheritedType(typeof(T));

        /// <summary>
        /// 指定されたInterfaceを実装しているかどうか?
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.ContainsInterfacePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool ContainsInterface(this System.Type t, System.Type interfaceType)
        {
            return interfaceType.IsInterface && t.GetInterface(interfaceType.Name) != null;
        }

        /// <summary>
        /// 指定されたInterfaceを実装しているかどうか?
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.ContainsInterfacePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool ContainsInterface<InterfaceType>(this System.Type t)
            => t.ContainsInterface(typeof(InterfaceType));

        /// <summary>
        /// 整数型かどうか?
        /// System.Numeric.BigIntegerも含みます
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.IsIntegerPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsInteger(this System.Type t)
            => t.Equals(typeof(sbyte))
            || t.Equals(typeof(short))
            || t.Equals(typeof(int))
            || t.Equals(typeof(long))
            || t.Equals(typeof(byte))
            || t.Equals(typeof(ushort))
            || t.Equals(typeof(uint))
            || t.Equals(typeof(ulong))
            || t.Equals(typeof(BigInteger));

        /// <summary>
        /// 浮動小数点型かどうか？
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.IsFloatPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsFloat(this System.Type t)
            => t.Equals(typeof(float))
            || t.Equals(typeof(double))
            || t.Equals(typeof(decimal));

        /// <summary>
        /// 数値を表す組み込み型か？
        /// System.Numeric.BigIntegerも含みます
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.IsNumericPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNumeric(this System.Type t)
            => t.IsInteger() || t.IsFloat();

        /// <summary>
        /// struct型かどうか?
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.IsStructPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsStruct(this System.Type t)
            => t.IsValueType && !t.IsEnum && !t.IsNumeric();

        /// <summary>
        /// 数値を表すstringをtypeに対応した数値型に変換する
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.ParseToNumberPasses()"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseToNumber(this System.Type type, string str)
        {
            if(type?.IsEnum ?? false)
            {//enumの時は対応する型を使用してパースする(charは未対応)
                type = System.Enum.GetUnderlyingType(type);
            }
            Assert.IsNotNull(type);

            if (type.Equals(typeof(sbyte)))
            {
                return sbyte.Parse(str);
            }
            else if (type.Equals(typeof(byte)))
            {
                return byte.Parse(str);
            }
            else if (type.Equals(typeof(short)))
            {
                return short.Parse(str);
            }
            else if (type.Equals(typeof(ushort)))
            {
                return ushort.Parse(str);
            }
            else if (type.Equals(typeof(int)))
            {
                return int.Parse(str);
            }
            else if (type.Equals(typeof(uint)))
            {
                return uint.Parse(str);
            }
            else if (type.Equals(typeof(long)))
            {
                return long.Parse(str);
            }
            else if (type.Equals(typeof(ulong)))
            {
                return ulong.Parse(str);
            }
            else if (type.Equals(typeof(float)))
            {
                return float.Parse(str);
            }
            else if (type.Equals(typeof(double)))
            {
                return double.Parse(str);
            }
            else if (type.Equals(typeof(decimal)))
            {
                return decimal.Parse(str);
            }
            else if (type.Equals(typeof(BigInteger)))
            {
                return BigInteger.Parse(str);
            }
            else
            {
                throw new System.NotSupportedException($"'type' is not numeric type... type={type}");
            }
        }

        /// <summary>
        /// 数値を表すstringをtypeに対応した数値型に変換する
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TryParseToNumber()"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryParseToNumber(this System.Type type, string str, out object outNum)
        {
            if (type?.IsEnum ?? false)
            {//enumの時は対応する型を使用してパースする(charは未対応)
                type = System.Enum.GetUnderlyingType(type);
            }
            Assert.IsNotNull(type);

            if (type.Equals(typeof(sbyte)))
            {
                var isOK = sbyte.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(byte)))
            {
                var isOK = byte.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(short)))
            {
                var isOK = short.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(ushort)))
            {
                var isOK = ushort.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(int)))
            {
                var isOK = int.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(uint)))
            {
                var isOK = uint.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(long)))
            {
                var isOK = long.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(ulong)))
            {
                var isOK = ulong.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(float)))
            {
                var isOK = float.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(double)))
            {
                var isOK = double.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(decimal)))
            {
                var isOK = decimal.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(BigInteger)))
            {
                var isOK = BigInteger.TryParse(str, out var n);
                outNum = n;
                return isOK;
            }
            else
            {
                Debug.LogWarning($"'type' is not numeric type... Return false and assign null to 'outNum'. type={type}");
                outNum = null;
                return false;
            }
        }

        /// <summary>
        /// 型が等しいかどうか判定する。
        /// もし比較対象の型がGeneric型の時は元になったGeneric型が等しいかどうか判定します。
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.EqualGenericTypeDefinitionPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualGenericTypeDefinition(this System.Type t, System.Type other)
        {
            if (t == null || other == null) return t == other;

            if (InnerEqualGenericTypeDefinition(t, other))
                return true;
            //型を入れ替えて判定する
            return InnerEqualGenericTypeDefinition(other, t);
        }

        static bool InnerEqualGenericTypeDefinition(System.Type t, System.Type other)
        {
            var self = t;
            var o = other.IsGenericType && !other.IsGenericTypeDefinition ? other.GetGenericTypeDefinition() : other;
            while (self != null)
            {
                self = self.IsGenericType && !self.IsGenericTypeDefinition ? self.GetGenericTypeDefinition() : self;
                if (self.Equals(o)) return true;
                self = self.BaseType;
            }

            return t.GetInterfaces().Any(_i => InnerEqualGenericTypeDefinition(_i, other));
        }


        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.GetFieldInHierarchyPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static FieldInfo GetFieldInHierarchy(this System.Type t, string name, BindingFlags flags = BindingFlags.Default)
        {
            while(t != null)
            {
                var info = t.GetField(name, flags);
                if (info != null)
                    return info;
                t = t.BaseType;
            }
            return null;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.IsArrayOrListPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsArrayOrList(this System.Type t)
        {
            if(t.IsArray) return true;
            //IList, IList<>から派生しているか？
            return t.GetInterfaces().Any(_i => _i.Equals(typeof(IList)) || _i.Equals(typeof(IList<>)));
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.GetArrayElementTypePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static System.Type GetArrayElementType(this System.Type t)
        {
            Assert.IsTrue(t.IsArrayOrList());
            if (t.IsArray)
            {
                return t.GetElementType();
            }

            var elementType = t.GetInterfaces()
                .Where(_i => _i.IsGenericType)
                .Where(_i => _i.GetGenericTypeDefinition().Equals(typeof(IList<>)))
                .Select(_i => _i.GetGenericArguments()[0])
                .FirstOrDefault();
            return elementType ?? typeof(object);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestTypeExtensions.GetClassHierarchyEnumerablePasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<System.Type> GetClassHierarchyEnumerable(this System.Type t)
        {
            return new ClassHierarchyEnumerable(t);
        }

        class ClassHierarchyEnumerable : IEnumerable<System.Type>, IEnumerable
        {
            System.Type _target;
            public ClassHierarchyEnumerable(System.Type target)
            {
                _target = target;
            }

            public IEnumerator<System.Type> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<System.Type>, IEnumerator, System.IDisposable
            {
                System.Type _target;
                IEnumerator<System.Type> _enumerator;
                public Enumerator(System.Type target)
                {
                    _target = target;
                    Reset();
                }
                public System.Type Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = GetEnumerator();

                IEnumerator<System.Type> GetEnumerator()
                {
                    var t = _target;
                    while(t != null)
                    {
                        yield return t;
                        t = t.BaseType;
                    }
                }
            }
        }

    }
}
