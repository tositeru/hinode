using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    static class KeyValueDictionaryEditorCommon
    {
        /// <summary>
        /// Editorから現在の描画領域の横幅を取得できなかったので作成した関数
        /// </summary>
        /// <returns></returns>
        public static Rect GetInitialPos()
        {
            var font = GUI.skin.font;
            //var fontScale = (font.fontSize != 0 ? font.fontSize : 1);
            var rowHeight = font.lineHeight;
            rowHeight += GUI.skin.label.padding.vertical + GUI.skin.label.margin.vertical;
            EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true), GUILayout.Height(rowHeight));
            return GUILayoutUtility.GetLastRect();
        }
    }

    [CustomEditor(typeof(KeyBoolDictionary))]
    public class KeyBoolDictionaryEditor : Editor
    {
        KeyValueDictionaryEditorUtils<KeyBoolDictionary, KeyBoolObject, bool> _utils = new KeyValueDictionaryEditorUtils<KeyBoolDictionary, KeyBoolObject, bool>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyBoolValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyIntDictionary))]
    public class KeyIntDictionaryEditor : Editor
    {
        KeyValueDictionaryEditorUtils<KeyIntDictionary, KeyIntObject, int> _utils = new KeyValueDictionaryEditorUtils<KeyIntDictionary, KeyIntObject, int>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyIntValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyFloatDictionary))]
    public class KeyFloatDictionaryEditor : Editor
    {
        KeyValueDictionaryEditorUtils<KeyFloatDictionary, KeyFloatObject, float> _utils = new KeyValueDictionaryEditorUtils<KeyFloatDictionary, KeyFloatObject, float>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyFloatValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyDoubleDictionary))]
    public class KeyDoubleDictionaryEditor : Editor
    {
        KeyValueDictionaryEditorUtils<KeyDoubleDictionary, KeyDoubleObject, double> _utils = new KeyValueDictionaryEditorUtils<KeyDoubleDictionary, KeyDoubleObject, double>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyDoubleValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyStringDictionary))]
    public class KeyStringDictionaryEditor : Editor
    {
        KeyValueDictionaryEditorUtils<KeyStringDictionary, KeyStringObject, string> _utils = new KeyValueDictionaryEditorUtils<KeyStringDictionary, KeyStringObject, string>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyStringValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyEnumDictionary))]
    public class KeyEnumDictionaryEditor : Editor
    {
        KeyValueDictionaryWithTypeNameEditorUtils<KeyEnumDictionary, KeyEnumObject, int, System.Enum> _utils = new KeyValueDictionaryWithTypeNameEditorUtils<KeyEnumDictionary, KeyEnumObject, int, System.Enum>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyEnumValue Dictionary"));
        }
    }

    [CustomEditor(typeof(KeyObjectRefDictionary))]
    public class KeyObjectRefDictionaryEditor : Editor
    {
        KeyValueDictionaryWithTypeNameEditorUtils<KeyObjectRefDictionary, KeyObjectRefObject, Object, Object> _utils = new KeyValueDictionaryWithTypeNameEditorUtils<KeyObjectRefDictionary, KeyObjectRefObject, Object, Object>();

        public override void OnInspectorGUI()
        {
            var initialPos = KeyValueDictionaryEditorCommon.GetInitialPos();
            _utils.Draw(serializedObject, initialPos, new GUIContent("KeyObjectReferenceValue Dictionary"));
        }
    }

}
