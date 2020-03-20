using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class KeyAndTypeDictionaryGetterAttribute : System.Attribute
    {
    }

    /// <summary>
    /// キーに対応するSystem.TypeのDictionaryを取得するためのAttribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class HasKeyAndTypeDictionaryGetterAttribute : System.Attribute
    {
        readonly static object[] EMPTY_ARGS = { };

        readonly System.Type _targetType;
        readonly MethodInfo _methodInfo;
        public System.Type TargetType { get => _targetType; }
        public MethodInfo MethodInfo { get => _methodInfo; }

        public HasKeyAndTypeDictionaryGetterAttribute(System.Type targetType)
        {
            _targetType = targetType;

            var bindflags = BindingFlags.DeclaredOnly
                | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic;
            var getterMethodInfos = targetType.GetMethods(bindflags)
                .Where(_m => null != _m.GetCustomAttribute<KeyAndTypeDictionaryGetterAttribute>());
            Assert.AreEqual(1, getterMethodInfos.Count(), $"KeyAndTypeDictionaryGetterAttributeを持つ関数がクラス内に一つだけにしてください。");
            _methodInfo = getterMethodInfos.First();
            Assert.AreEqual(typeof(IReadOnlyDictionary<string, System.Type>), _methodInfo.ReturnType, $"KeyAndTypeDictionaryGetterAttributeを持つ関数の戻り値はIReadOnlyDictionary<string, string>にしてください。");
        }

        public IReadOnlyDictionary<string, System.Type> GetDictionary(System.Type type)
        {
            Assert.IsTrue(type.EqualGenericTypeDefinition(TargetType), $"Don't Equal Type... correct={TargetType.FullName}, got={type.FullName}");

            var srcDicts = type.GetClassHierarchyEnumerable()
                .Select(_t => (type: _t, attr: _t.GetCustomAttribute<HasKeyAndTypeDictionaryGetterAttribute>()))
                .Where(_t => _t.attr != null)
                .Select(_t => _t.attr.MethodInfo)
                .Where(_m => _m != null)
                .SelectMany(_m => {
                    //Debug.Log($"debug--pass type={_m.DeclaringType.Name} method={_m.Name}");
                    return (IReadOnlyDictionary<string, System.Type>)_m.Invoke(null, EMPTY_ARGS);
                });

            var dict = new Dictionary<string, System.Type>();
            dict.Merge(false, srcDicts);
            return dict;
        }
    }
}
