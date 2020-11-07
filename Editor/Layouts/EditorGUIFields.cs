using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Hinode.Editors
{
    public static class EditorGUIFields
    {

        public static bool DrawField(object value, out object newValue, FieldInfo fieldInfo, GUIStyle style, params GUILayoutOption[] options)
        {
            var type = value.GetType();
            if (type.IsSameOrInheritedType<int>())
            {
                var v = DrawIntField((int)value, fieldInfo, style, options);
                newValue = v;
                return v != (int)value;
            }
            else
            {
                Debug.Log($"not implement... {type}");
                newValue = default;
                return false;
            }
        }


        public static int DrawIntField(int value, FieldInfo fieldInfo, GUIStyle style, params GUILayoutOption[] options)
        {
            if(fieldInfo == null)
            {
                return EditorGUILayout.IntField(value, style ?? GUI.skin.textField, options);
            }

            //TODO
            var attrs = fieldInfo.GetCustomAttribute<PropertyAttribute>();
            return EditorGUILayout.IntField(value);
        }
    }
}
