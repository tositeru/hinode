using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    /// <summary>
    /// 現状のUnityではカスタムInspectorとPropertyDrawerでSerializedObjectとSerializedPropertyの分別ができていないみたいなので、
    /// それらの間で処理を共通化するときに使用することを想定しています。
    /// </summary>
    public class SerializedTarget
    {
        public enum Type
        {
            SerializedObject,
            SerializedProperty,
        }

        Type _type;
        public Type CurrentType { get => _type; }
        object _target;
        public SerializedObject SerializedObject { get => _target as SerializedObject; }
        public SerializedProperty SerializedProperty { get => _target as SerializedProperty; }

        public Object ObjectReference
        {
            get
            {
                switch (_type)
                {
                    case Type.SerializedObject:
                        var SO = _target as SerializedObject;
                        return SO.targetObject;
                    case Type.SerializedProperty:
                        var prop = _target as SerializedProperty;
                        return prop.objectReferenceValue;
                    default:
                        throw new System.NotImplementedException();
                }
            }
            set
            {
                switch (_type)
                {
                    case Type.SerializedObject:
                        throw new System.InvalidOperationException("対応していません");
                    case Type.SerializedProperty:
                        var prop = _target as SerializedProperty;
                        prop.objectReferenceValue = value;
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        public void Set(SerializedObject target) => Set(Type.SerializedObject, target);
        public void Set(SerializedProperty target) => Set(Type.SerializedProperty, target);
        public void Set(Type type, object target)
        {
            _type = type;
            switch(type)
            {
                case Type.SerializedObject: Assert.IsTrue(target is SerializedObject); break;
                case Type.SerializedProperty: Assert.IsTrue(target is SerializedProperty); break;
                default:
                    throw new System.NotImplementedException();
            }
            _target = target;
        }

        public SerializedProperty FindProperty(string name)
        {
            switch (_type)
            {
                case Type.SerializedObject:
                    return SerializedObject.FindProperty(name);
                case Type.SerializedProperty:
                    return SerializedProperty.FindPropertyRelative(name);
                default:
                    throw new System.NotImplementedException();
            }
        }

    }
}
