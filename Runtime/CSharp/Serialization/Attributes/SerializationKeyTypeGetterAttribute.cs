using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Serialization
{
    /// <summary>
    /// Hinode.ISerializer内で使用されるKeyに対応するSystem.Typeを返す関数を表すことを示すAttribute
    ///
    /// このAttributeはContainsSerializationKeyTypeGetterAttributeが指定されているクラス内で使用してください。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SerializationKeyTypeGetterAttribute : System.Attribute
    {
        public SerializationKeyTypeGetterAttribute()
        {}

        public static bool IsValid(MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters();
            return methodInfo.IsStatic
                && methodInfo.ReturnType.Equals(typeof(System.Type))
                && args.Length == 1
                && args[0].ParameterType.Equals(typeof(string));
        }
    }

    /// <summary>
    /// <see cref="SerializationKeyTypeGetterAttribute"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ContainsSerializationKeyTypeGetterAttribute : System.Attribute
    {
        readonly static object[] EMPTY_ARGS = { };

        readonly System.Type _targetType;
        readonly MethodInfo _methodInfo;
        public System.Type TargetType { get => _targetType; }
        public MethodInfo MethodInfo { get => _methodInfo; }

        public ContainsSerializationKeyTypeGetterAttribute(System.Type targetType)
        {
            _targetType = targetType;

            var bindflags = BindingFlags.DeclaredOnly
                | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic;
            var getterMethodInfos = targetType.GetMethods(bindflags)
                .Where(_m => null != _m.GetCustomAttribute<SerializationKeyTypeGetterAttribute>());
            Assert.AreEqual(1, getterMethodInfos.Count(), $"SerializationKeyTypeGetterAttributeを持つ関数はクラス内に一つだけにしてください。");
            _methodInfo = getterMethodInfos.First();

            Assert.IsTrue(SerializationKeyTypeGetterAttribute.IsValid(_methodInfo), $"SerializationKeyTypeGetterAttributeに指定された関数が想定された戻り値、引数を持っていません。static System.Type <method name>(string key)にしてください");
        }

        public ISerializationKeyTypeGetter CreateKeyTypeGetter(System.Type type)
        {
            Assert.IsTrue(type.EqualGenericTypeDefinition(TargetType), $"Don't Equal Type... correct={TargetType.FullName}, got={type.FullName}");

            return new AttributeSerializationKeyTypeGetter(type);
        }
    }

    /// <summary>
    /// <see cref="ISerializer"/>
    /// <see cref="ContainsSerializationKeyTypeGetterAttribute"/>
    /// <see cref="SerializationKeyTypeGetterAttribute"/>
    /// </summary>
    public class AttributeSerializationKeyTypeGetter : ISerializationKeyTypeGetter
    {
        IEnumerable<MethodInfo> _getterList;

        public AttributeSerializationKeyTypeGetter(System.Type classType)
        {
            Assert.IsTrue(classType.IsClass, $"Not class type... classType={classType}");

            _getterList = classType.GetClassHierarchyEnumerable()
                .Select(_t => (type: _t, attr: _t.GetCustomAttribute<ContainsSerializationKeyTypeGetterAttribute>()))
                .Where(_t => _t.attr != null)
                .Select(_t => _t.attr.MethodInfo)
                .Where(_m => _m != null && _m.GetCustomAttribute<SerializationKeyTypeGetterAttribute>() != null);
        }

        /// <summary>
        /// keyに対応したSystem.Typeを取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public System.Type Get(string key)
            => _getterList
                .Select(_g => (System.Type)_g.Invoke(null, new object[] { key }))
                .FirstOrDefault(_t => _t != null);
    }

}
