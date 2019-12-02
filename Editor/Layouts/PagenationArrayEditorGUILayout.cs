using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public class PagenationArrayEditorGUILayout
    {
        public delegate GUIContent GetLabelDelegate(int index, SerializedProperty prop);
        public delegate void DrawChildElementDelegate(int index, SerializedProperty child, GUIContent childLabel);

        int _page;
        bool _foldout;
        GetLabelDelegate _getLavelDelegate;
        DrawChildElementDelegate _drawChildElementDelegate;

        readonly int _maxCountPerPage;

        public int MaxCountPerPage { get => _maxCountPerPage; }

        public PagenationArrayEditorGUILayout(int maxCountPerPage, GetLabelDelegate labelGetter = null, DrawChildElementDelegate drawChildElementDelefate = null)
        {
            _maxCountPerPage = maxCountPerPage;
            _getLavelDelegate = labelGetter;
            _drawChildElementDelegate = drawChildElementDelefate;
        }

        GUIContent GetLabel(int index, SerializedProperty prop)
        {
            return _getLavelDelegate?.Invoke(index, prop) ?? new GUIContent($"Element {index}");
        }

        public void OnInspectorGUI(SerializedProperty property, GUIContent label)
        {
            if (!property.isArray)
            {
                EditorGUILayout.LabelField("This property is not array...");
                return;
            }

            _foldout = EditorGUILayout.Foldout(_foldout, label);
            if (!_foldout) return;

            bool doChanged = false;
            using (var indent = new EditorGUI.IndentLevelScope())
            {
                int newArraySize = -1;
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    newArraySize = Mathf.Max(0, EditorGUILayout.IntField("Size", property.arraySize));
                    if (scope.changed)
                    {
                        doChanged |= true;
                    }
                }
                var endIndex = Mathf.Min(property.arraySize, (_page + 1) * MaxCountPerPage);
                for (var i = _page * MaxCountPerPage; i < endIndex; ++i)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    var elementLabel = GetLabel(i, element);
                    if(_drawChildElementDelegate != null)
                    {
                        _drawChildElementDelegate.Invoke(i, element, elementLabel);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(element, elementLabel, true);
                    }
                    //doChanged |= EditorGUILayout.PropertyField(element, elementLabel, true);
                }

                if (doChanged)
                {
                    property.arraySize = newArraySize;
                }

                //Show Pagination
                if (property.arraySize >= MaxCountPerPage)
                {
                    var maxPage = (property.arraySize / MaxCountPerPage);
                    maxPage += (property.arraySize % MaxCountPerPage) == 0 ? 0 : 1;
                    _page = EditorGUILayout.IntSlider($"Pagination {_page + 1}/{maxPage}", _page + 1, 1, maxPage);
                    _page = Mathf.Clamp(_page - 1, 0, maxPage);
                }
            }
        }
    }
}
