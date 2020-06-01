using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    public abstract class PopupEditorGUILayout
    {
        protected abstract string[] CreateDisplayOptionList();

        string[] _displayOptionList;

        public int SelectedIndex { get; set; }
        public string SelectedElement
        {
            get => 0 <= SelectedIndex && SelectedIndex < DisplayOptionList.Length
                ? DisplayOptionList[SelectedIndex]
                : "";
            set
            {
                SelectedIndex = System.Array.IndexOf(DisplayOptionList, value);
                SelectedIndex = Mathf.Min(DisplayOptionList.Length-1, SelectedIndex);
            }
        }

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
            => Draw(prop, new GUIContent(prop.displayName));
        public bool Draw(SerializedProperty prop, GUIContent label)
        {
            Assert.IsTrue(prop.propertyType == SerializedPropertyType.String);
            return Draw(prop.stringValue, label);
        }

        public bool Draw(string element, GUIContent label)
        {
            if (DisplayOptionList.Length == 0)
            {
                EditorGUILayout.LabelField(label, new GUIContent("Not options..."));
                return element != "";
            }
            else
            {
                var SelectedIndex = System.Array.IndexOf(DisplayOptionList, element, 0, DisplayOptionList.Length);
                if (SelectedIndex == -1) SelectedIndex = 0;

                var newSelectedIndex = EditorGUILayout.Popup(label, SelectedIndex, DisplayOptionList);
                if (newSelectedIndex != SelectedIndex)
                {
                    SelectedIndex = newSelectedIndex;
                    return true;
                }
            }
            return false;
        }

        public bool Draw(GUIContent label)
        {
            if (DisplayOptionList.Length == 0)
            {
                EditorGUILayout.LabelField(label, new GUIContent("Not options..."));
                var isChanged = (SelectedIndex != -1);
                SelectedIndex = -1;
                return isChanged;
            }
            else
            {
                if (SelectedIndex == -1) SelectedIndex = 0;

                var newSelectedIndex = EditorGUILayout.Popup(label, SelectedIndex, DisplayOptionList);
                if (newSelectedIndex != SelectedIndex)
                {
                    SelectedIndex = newSelectedIndex;
                    return true;
                }
            }
            return false;
        }

    }

}
