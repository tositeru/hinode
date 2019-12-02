using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Hinode.Editors
{
    /// <summary>
    /// Pagination付きの配列を描画するPropertyDrawer
    /// Unityの仕様上配列のフィールドにこのAttributeに指定してもうまく動作しません。
    /// </summary>
    public class PaginationArrayGUIAttributePropertyDrawer : PropertyDrawer
    {
        int _page;
        bool _foldout;
        System.Func<int, SerializedProperty, GUIContent> _getLabelPred;
        readonly int _maxCountPerPage;

        public int MaxCountPerPage { get => _maxCountPerPage; }

        public PaginationArrayGUIAttributePropertyDrawer(int maxCountPerPage, System.Func<int, SerializedProperty, GUIContent> labelGetter = null)
        {
            _maxCountPerPage = maxCountPerPage;
            _getLabelPred = labelGetter;
            if(_getLabelPred == null)
            {
                _getLabelPred = (i, prop) => new GUIContent($"Index {i}");
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!property.isArray)
            {
                EditorGUI.LabelField(position, "This property is not array...");
                return;
            }

            var pos = new GUILayoutPosition(position, position.height);
            _foldout = EditorGUI.Foldout(pos.Pos, _foldout, label);
            if (!_foldout) return;

            bool doChanged = false;
            using (var indent = new EditorGUI.IndentLevelScope())
            {
                var elementsPos = pos.Indent(true);
                elementsPos.IncrementRow();
                int newArraySize = -1;
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    newArraySize = Mathf.Max(0, EditorGUI.IntField(elementsPos.Pos, "Size", property.arraySize));
                    elementsPos.IncrementRow();
                    if (scope.changed)
                    {
                        doChanged |= true;
                    }
                }
                var endIndex = Mathf.Min(property.arraySize, (_page + 1) * MaxCountPerPage);
                for (var i = _page * MaxCountPerPage; i < endIndex; ++i)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    var elementLabel = _getLabelPred(i, element);
                    doChanged |= EditorGUI.PropertyField(elementsPos.Pos, element, elementLabel, true);
                    elementsPos.IncrementRow(element, true);
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
                    _page = EditorGUI.IntSlider(elementsPos.Pos, $"Pagination {_page + 1}/{maxPage}", _page + 1, 1, maxPage);
                    _page = Mathf.Clamp(_page - 1, 0, maxPage);
                }
            }
        }
    }
}
