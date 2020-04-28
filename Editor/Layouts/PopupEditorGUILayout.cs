using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    abstract class PopupEditorGUILayout
    {
        protected abstract string[] CreateDisplayOptionList();

        string[] _displayOptionList;
        public int SelectedIndex { get; set; }
        public string SelectedElement { get => DisplayOptionList[SelectedIndex]; }

        public string[] DisplayOptionList
        {
            get
            {
                if (_displayOptionList == null)
                {
                    _displayOptionList = CreateDisplayOptionList();
                }
                return _displayOptionList;
            }
        }

        public bool Draw(SerializedProperty prop)
        {
            Assert.IsTrue(prop.propertyType == SerializedPropertyType.String);
            var label = new GUIContent(prop.displayName);
            if (DisplayOptionList.Length == 0)
            {
                prop.stringValue = "";
                EditorGUILayout.LabelField(label, new GUIContent("Not options..."));
            }
            else
            {
                var SelectedIndex = System.Array.IndexOf(DisplayOptionList, prop.stringValue, 0, DisplayOptionList.Length);
                if (SelectedIndex == -1) SelectedIndex = 0;

                var newSelectedIndex = EditorGUILayout.Popup(label, SelectedIndex, DisplayOptionList);
                if (newSelectedIndex != SelectedIndex)
                {
                    SelectedIndex = newSelectedIndex;
                    prop.stringValue = DisplayOptionList[SelectedIndex];
                    return true;
                }
            }
            return false;
        }

    }

}
