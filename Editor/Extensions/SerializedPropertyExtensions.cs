using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.Assertions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hinode.Editors
{
    public static class SerializedPropertyExtensions
    {
        public static T GetAssetInstance<T>(this SerializedProperty prop)
            where T : Object
        {
            var assetPath = AssetDatabase.GetAssetOrScenePath(prop.objectReferenceValue);
            if (assetPath == null) return null;
            var assetInstance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return assetInstance;
        }

        public static IEnumerable<System.Attribute> GetFieldAttributes(this SerializedProperty prop)
        {
            return prop.GetFieldAttributes<System.Attribute>();
        }

        public static IEnumerable<T> GetFieldAttributes<T>(this SerializedProperty prop)
            where T : System.Attribute
        {
            var fieldInfo = prop.GetFieldInfo();
            return fieldInfo.GetCustomAttributes<T>();
        }

        public static IEnumerable<System.Attribute> GetFieldAttributes(this SerializedProperty prop, System.Type type)
        {
            Assert.IsTrue(type.IsSubclassOf(typeof(System.Attribute)));
            var fieldInfo = prop.GetFieldInfo();
            return fieldInfo.GetCustomAttributes(type);
        }

        /// <summary>
        /// propのFieldInfoを取得する関数
        /// もしpropが配列およびSystem.Generic.Collection.Listの要素の場合はその要素が所属している配列またはListのFieldInfoを返します。
        /// これはそのフィールドに指定されているAttributeを取得するためです。
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static FieldInfo GetFieldInfo(this SerializedProperty prop)
        {
            return prop.GetPropertyPathEnumerable().Last().fieldInfo;
        }

        public static object GetSelf(this SerializedProperty prop)
        {
            return prop.GetPropertyPathEnumerable().Last().instance;
        }

        public static IEnumerable<PropertyPathEnumerableData> GetPropertyPathEnumerable(this SerializedProperty prop)
        {
            return new PropertyPathEnumerable(prop);
        }

        public static IEnumerable<(SerializedProperty prop, int index)> GetArrayElementEnumerable(this SerializedProperty prop)
        {
            return new ArrayElementEnumerable(prop);
        }

        /// <summary>
        /// 配列要素を探索するEnumerable
        /// </summary>
        class ArrayElementEnumerable : IEnumerable<(SerializedProperty prop, int index)>, IEnumerable
        {
            SerializedProperty _target;
            public ArrayElementEnumerable(SerializedProperty target)
            {
                _target = target;
            }

            public IEnumerator<(SerializedProperty prop, int index)> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<(SerializedProperty prop, int index)>, IEnumerator, System.IDisposable
            {
                SerializedProperty _target;
                IEnumerator<(SerializedProperty, int index)> _e;
                public Enumerator(SerializedProperty target)
                {
                    _target = target;
                    Reset();
                }
                public (SerializedProperty prop, int index) Current => _e.Current;
                object IEnumerator.Current => Current;
                public void Dispose() => _e.Dispose();
                public bool MoveNext() => _e.MoveNext();
                public void Reset() => _e = ForEach();
                IEnumerator<(SerializedProperty prop, int index)> ForEach()
                {
                    for(var i=0; i<_target.arraySize; ++i)
                    {
                        yield return (_target.GetArrayElementAtIndex(i), i);
                    }
                }
            }
        }

        /// <summary>
        /// SerializedProperty#propertyPathの各要素を探索するEnumerable
        /// <seealso cref="PropertyPathEnumerableData"/>
        /// </summary>
        class PropertyPathEnumerable : IEnumerable<PropertyPathEnumerableData>, IEnumerable
        {
            SerializedProperty _target;
            public PropertyPathEnumerable(SerializedProperty target)
            {
                Assert.IsNotNull(target);
                _target = target;
            }

            public IEnumerator<PropertyPathEnumerableData> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<PropertyPathEnumerableData>, IEnumerator, System.IDisposable
            {
                SerializedProperty _target;
                IEnumerator<PropertyPathEnumerableData> _enumerator;
                public Enumerator(SerializedProperty target)
                {
                    Assert.IsNotNull(target);
                    _target = target;
                    Reset();
                }

                public PropertyPathEnumerableData Current { get => _enumerator.Current; }
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset()
                {
                    _enumerator = ForEach();
                }

                IEnumerator<PropertyPathEnumerableData> ForEach()
                {
                    if (_target.serializedObject == null) yield break;
                    //Debug.Log($"debug-- ForEach -- SP => {_target.propertyPath}");
                    PropertyPathEnumerableData prev = new PropertyPathEnumerableData {
                        path = "",
                        type = PropertyPathType.Normal,
                        fieldInfo = null,
                        instance = _target.serializedObject.targetObject,
                    };
                    foreach (var prop in _target.propertyPath.Split('.').AsEnumerable())
                    {
                        PropertyPathType type = GetPropertyPathType(prop, prev.type);
                        //Debug.Log($"debug-- ForEach -- cur={prop},{type} prev={prev.path},{prev.type}");
                        GetFieldInfoAndInstance(out var fieldInfo, out var instance, prev, prop, type);
                        var data = new PropertyPathEnumerableData
                        {
                            path = prop,
                            type = type,
                            fieldInfo = fieldInfo,
                            instance = instance,
                        };
                        yield return data;
                        prev = data;
                    }
                }

                PropertyPathType GetPropertyPathType(string prop, PropertyPathType prevType)
                {
                    if (prevType == PropertyPathType.Array) return PropertyPathType.ArrayElement;
                    if (prop == "Array") return PropertyPathType.Array;
                    return PropertyPathType.Normal;
                }

                void GetFieldInfoAndInstance(out FieldInfo outFieldInfo, out object outInstance, PropertyPathEnumerableData prevData, string prop, PropertyPathType curType)
                {
                    switch(curType)
                    {
                        case PropertyPathType.Array:
                            outFieldInfo = prevData.fieldInfo;
                            outInstance = prevData.instance;
                            break;
                        case PropertyPathType.ArrayElement:
                            // Note: 所属している配列に指定されているAttributeを取得できるように、配列のFieldInfoを返すようにしている
                            outFieldInfo = prevData.fieldInfo;

                            var m = REGEX_ARRAY_ELEMENT.Match(prop);
                            if(m == null || !int.TryParse(m.Groups[1].Value, out int index))
                            {
                                throw new System.InvalidOperationException($"添字の取得に失敗しました。current path={prop}");
                            }
                            if(prevData.instance == null)
                            {
                                outInstance = null;
                            }
                            else if (outFieldInfo.FieldType.IsArray)
                            {
                                var array = (System.Array)prevData.instance;
                                outInstance = 0 <= index && index < array.Length
                                    ? array.GetValue(index)
                                    : null;
                            }
                            else if(outFieldInfo.FieldType.GetGenericTypeDefinition().Equals(typeof(List<>)))
                            {
                                var arrayInfo = prevData.instance.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
                                var array = (System.Array)arrayInfo.GetValue(prevData.instance);
                                outInstance = 0 <= index && index < array.Length
                                    ? array.GetValue(index)
                                    : null;
                            }
                            else
                            {
                                throw new System.NotImplementedException("想定外");
                            }
                            //Debug.Log($"debug-- Get ArrayElement-- path={prop}, {outFieldInfo}, {outInstance}");
                            break;
                        default: // PropertyPathType.Normal
                            System.Type currentType = prevData.fieldInfo != null
                                ? prevData.fieldInfo.FieldType
                                : _target.serializedObject.targetObject.GetType();
                            switch(prevData.type)
                            {
                                case PropertyPathType.ArrayElement:
                                    if(currentType.IsArray)
                                    {
                                        currentType = prevData.fieldInfo.FieldType.GetElementType();
                                    }
                                    else if(currentType.GetGenericTypeDefinition().Equals(typeof(List<>)))
                                    {
                                        currentType = currentType.GenericTypeArguments[0];
                                    }
                                    break;
                            }
                            outFieldInfo = currentType.GetFieldInHierarchy(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            //Debug.Log($"debug-- Get --path={prop}, {outFieldInfo}, {prevData.instance}");
                            outInstance = prevData.instance != null
                                ? outFieldInfo.GetValue(prevData.instance)
                                : null;
                            break;
                    }
                }
                static readonly Regex REGEX_ARRAY_ELEMENT = new Regex(@"^data\[(\d+)\]", RegexOptions.Singleline);

            }
        }
    }

    /// <summary>
    /// SerializedPropertyExtensions.PropertyPathEnumerableで返される値の型
    /// インスタンスが存在しない場合はinstanceはnullになります。
    /// <seealso cref="SerializedPropertyExtensions.PropertyPathEnumerable"/>
    /// </summary>
    public class PropertyPathEnumerableData
    {
        public string path;
        public PropertyPathType type;
        public FieldInfo fieldInfo;
        public object instance;

        public void Deconstruct(out string path, out PropertyPathType type, out FieldInfo info, out object instance)
        {
            path = this.path;
            type = this.type;
            info = fieldInfo;
            instance = this.instance;
        }

        public static implicit operator PropertyPathEnumerableData((string, PropertyPathType, FieldInfo, object) t)
        {
            return new PropertyPathEnumerableData
            {
                path = t.Item1,
                type = t.Item2,
                fieldInfo = t.Item3,
                instance = t.Item4,
            };
        }

        public static implicit operator (string, PropertyPathType, FieldInfo, object)(PropertyPathEnumerableData d)
        {
            return (d.path, d.type, d.fieldInfo, d.instance);
        }
    }
    public enum PropertyPathType
    {
        Normal,
        Array,
        ArrayElement,
    }
}
