using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    [CustomPropertyDrawer(typeof(KeyBoolDictionary))]
    public class KeyBoolDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryEditorUtils<KeyBoolDictionary, KeyBoolObject, bool> _utils = new KeyValueDictionaryEditorUtils<KeyBoolDictionary, KeyBoolObject, bool>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    [CustomPropertyDrawer(typeof(KeyIntDictionary))]
    public class KeyIntDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryEditorUtils<KeyIntDictionary, KeyIntObject, int> _utils = new KeyValueDictionaryEditorUtils<KeyIntDictionary, KeyIntObject, int>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    [CustomPropertyDrawer(typeof(KeyFloatDictionary))]
    public class KeyFloatDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryEditorUtils<KeyFloatDictionary, KeyFloatObject, float> _utils = new KeyValueDictionaryEditorUtils<KeyFloatDictionary, KeyFloatObject, float>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    [CustomPropertyDrawer(typeof(KeyDoubleDictionary))]
    public class KeyDoubleDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryEditorUtils<KeyDoubleDictionary, KeyDoubleObject, double> _utils = new KeyValueDictionaryEditorUtils<KeyDoubleDictionary, KeyDoubleObject, double>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    [CustomPropertyDrawer(typeof(KeyStringDictionary))]
    public class KeyStringDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryEditorUtils<KeyStringDictionary, KeyStringObject, string> _utils = new KeyValueDictionaryEditorUtils<KeyStringDictionary, KeyStringObject, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    [CustomPropertyDrawer(typeof(KeyEnumDictionary))]
    public class KeyEnumDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryWithTypeNameEditorUtils<KeyEnumDictionary, KeyEnumObject, int, System.Enum> _utils = new KeyValueDictionaryWithTypeNameEditorUtils<KeyEnumDictionary, KeyEnumObject, int, System.Enum>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }

    /// <summary>
    /// Unityの仕様上シーンオブジェクトは設定できません。
    /// </summary>
    [CustomPropertyDrawer(typeof(KeyObjectRefDictionary))]
    public class KeyObjectRefDictionaryPropertyDrawer : PropertyDrawer
    {
        KeyValueDictionaryWithTypeNameEditorUtils<KeyObjectRefDictionary, KeyObjectRefObject, Object, Object> _utils = new KeyValueDictionaryWithTypeNameEditorUtils<KeyObjectRefDictionary, KeyObjectRefObject, Object, Object>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _utils.Draw(property, position, label);
        }
    }
}
