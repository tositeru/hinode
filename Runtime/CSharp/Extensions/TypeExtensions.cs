using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 同じまたは派生元の型かどうか？
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSameOrInheritedType(this System.Type t, System.Type type)
            => t.Equals(type)
            || t.IsSubclassOf(type)
            || t.HasInterface(type);

        /// <summary>
        /// 同じまたは派生元の型かどうか？
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSameOrInheritedType<T>(this System.Type t)
            => t.IsSameOrInheritedType(typeof(T));

        /// <summary>
        /// 指定されたInterfaceを実装しているかどうか?
        /// </summary>
        /// <param name="t"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool HasInterface(this System.Type t, System.Type interfaceType)
        {
            return interfaceType.IsInterface && t.GetInterfaces().Any(_i => _i == interfaceType);
        }

        /// <summary>
        /// 指定されたInterfaceを実装しているかどうか?
        /// </summary>
        /// <param name="t"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool HasInterface<InterfaceType>(this System.Type t)
            => t.HasInterface(typeof(InterfaceType));

        /// <summary>
        /// 整数型かどうか?
        /// System.Numeric.BigIntegerも含みます
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
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNumeric(this System.Type t)
            => t.IsInteger() || t.IsFloat();

        /// <summary>
        /// struct型かどうか?
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsStruct(this System.Type t)
            => t.IsValueType && !t.IsEnum && !t.IsNumeric();

        /// <summary>
        /// 数値を表すstringをtypeに対応した数値型に変換する
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseToNumber(this System.Type type, string keyword)
        {
            if(type?.IsEnum ?? false)
            {//enumの時は対応する型を使用してパースする(charは未対応)
                type = System.Enum.GetUnderlyingType(type);
            }
            Assert.IsNotNull(type);

            if (type.Equals(typeof(sbyte)))
            {
                return sbyte.Parse(keyword);
            }
            else if (type.Equals(typeof(byte)))
            {
                return byte.Parse(keyword);
            }
            else if (type.Equals(typeof(short)))
            {
                return short.Parse(keyword);
            }
            else if (type.Equals(typeof(ushort)))
            {
                return ushort.Parse(keyword);
            }
            else if (type.Equals(typeof(int)))
            {
                return int.Parse(keyword);
            }
            else if (type.Equals(typeof(uint)))
            {
                return uint.Parse(keyword);
            }
            else if (type.Equals(typeof(long)))
            {
                return long.Parse(keyword);
            }
            else if (type.Equals(typeof(ulong)))
            {
                return ulong.Parse(keyword);
            }
            else if (type.Equals(typeof(float)))
            {
                return float.Parse(keyword);
            }
            else if (type.Equals(typeof(double)))
            {
                return double.Parse(keyword);
            }
            else if (type.Equals(typeof(decimal)))
            {
                return decimal.Parse(keyword);
            }
            else if (type.Equals(typeof(BigInteger)))
            {
                return BigInteger.Parse(keyword);
            }
            else
            {
                throw new System.NotSupportedException($"'type' is not numeric type... type={type}");
            }
        }

        /// <summary>
        /// 数値を表すstringをtypeに対応した数値型に変換する
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryParseToNumber(this System.Type type, string keyword, out object outNum)
        {
            if (type?.IsEnum ?? false)
            {//enumの時は対応する型を使用してパースする(charは未対応)
                type = System.Enum.GetUnderlyingType(type);
            }
            Assert.IsNotNull(type);

            if (type.Equals(typeof(sbyte)))
            {
                var isOK = sbyte.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(byte)))
            {
                var isOK = byte.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(short)))
            {
                var isOK = short.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(ushort)))
            {
                var isOK = ushort.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(int)))
            {
                var isOK = int.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(uint)))
            {
                var isOK = uint.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(long)))
            {
                var isOK = long.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(ulong)))
            {
                var isOK = ulong.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(float)))
            {
                var isOK = float.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(double)))
            {
                var isOK = double.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(decimal)))
            {
                var isOK = decimal.TryParse(keyword, out var n);
                outNum = n;
                return isOK;
            }
            else if (type.Equals(typeof(BigInteger)))
            {
                var isOK = BigInteger.TryParse(keyword, out var n);
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
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualGenericTypeDefinition(this System.Type t, System.Type other)
        {
            var self = t.IsGenericType && !t.IsGenericTypeDefinition ? t.GetGenericTypeDefinition() : t;
            other = other.IsGenericType && !other.IsGenericTypeDefinition ? other.GetGenericTypeDefinition() : other;
            return self.Equals(other);
        }

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

        public static bool IsArrayOrList(this System.Type t)
        {
            if(t.IsArray) return true;
            //IList, IList<>から派生しているか？
            return t.GetInterfaces().Any(_i => _i.Equals(typeof(IList)) || _i.Equals(typeof(IList<>)));
        }

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
