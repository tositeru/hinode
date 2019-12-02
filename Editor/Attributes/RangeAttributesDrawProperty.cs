using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hinode.Editors
{
    /// <summary>
    /// KeyIntObjectに対応したRangeAttributeのPropertyDrawer
    /// <seealso cref="KeyIntObject"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(RangeIntAttribute))]
    public class RangeIntAttributeDrawProperty : PropertyDrawer
    {
        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    return base.CreatePropertyGUI(property);
        //}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    var range = attribute as RangeIntAttribute;
                    EditorGUI.IntSlider(position, property, range.Min, range.Max, label);
                    break;
                default:
                    if(fieldInfo.FieldType.Equals(typeof(KeyIntObject)))
                    {
                        var drawer = new KeyIntObjectPropertyDrawer();
                        drawer.OnGUI(position, property, label);
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label, new GUIContent($"Not support property type({property.type},{property.propertyType})..."));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// KeyFloatObjectに対応したRangeAttributeのPropertyDrawer
    /// 標準のRangeAttributeを拡張できなかったので作成したクラス
    /// <seealso cref="KeyFloatObject"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(RangeNumberAttribute))]
    public class RangeFloatAttributeDrawProperty : PropertyDrawer
    {
        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    return base.CreatePropertyGUI(property);
        //}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    var range = attribute as RangeNumberAttribute;
                    EditorGUI.Slider(position, property, range.Min, range.Max, label);
                    break;
                default:
                    if (fieldInfo.FieldType.Equals(typeof(KeyFloatObject))
                        || fieldInfo.FieldType.Equals(typeof(KeyDoubleObject)))
                    {
                        var drawer = new KeyNumberObjectPropertyDrawer();
                        drawer.OnGUI(position, property, label);
                    }
                    else if(fieldInfo.FieldType.Equals(typeof(double)))
                    {
                        range = attribute as RangeNumberAttribute;
                        EditorGUI.Slider(position, property, range.Min, range.Max, label);
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label, new GUIContent($"Not support property type({property.type},{property.propertyType})..."));
                    }
                    break;
            }
        }
    }
}
