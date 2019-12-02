using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    /// <summary>
    /// 強制的にObjectFieldにしたい時に指定するPropertyAttribute
    /// </summary>
    [CustomPropertyDrawer(typeof(ForcedObjectFieldAttribute), false)]
    public class ForcedObjectFieldAttributeDrawProperty : PropertyDrawer
    {
        ForcedObjectFieldAttribute _forcedObjectFieldAttr;
        ForcedObjectFieldAttribute ForcedObjectFieldAttribute
        {
            get
            {
                _forcedObjectFieldAttr = fieldInfo.GetCustomAttributes(true)
                    .OfType<ForcedObjectFieldAttribute>().FirstOrDefault(_t => _t != null);
                Assert.IsNotNull(_forcedObjectFieldAttr);
                return _forcedObjectFieldAttr;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new ObjectField(property.displayName)
            {
                objectType = fieldInfo.FieldType,
                allowSceneObjects = ForcedObjectFieldAttribute.AllowSceneObject,
                value = property.objectReferenceValue,
            };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.ObjectField(position, property, fieldInfo.FieldType);
        }
    }
}
