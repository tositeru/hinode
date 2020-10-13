using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Hinode.Editors
{
    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(ScenePathAttribute))]
    public class ScenePathAttributePropertyDrawer : PropertyDrawer
    {
        string[] _scenePathList = null;

        public string[] ScenePathList
        {
            get
            {
                if(_scenePathList == null)
                {
                    _scenePathList = 
                        EditorBuildSettings.scenes.Select(_s => _s.path)
                        .ToArray();
                }
                return _scenePathList;
            }
        }

        int FindIndex(string path)
        {
            var index = System.Array.FindIndex(ScenePathList, _p => _p == path);
            return index;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var divideCount = 6;
            var pos = new GUILayoutPosition(position);
            var labelPos = pos.GetSplitPos(divideCount, 0, 2);
            EditorGUI.LabelField(labelPos, label);

            var popUpPos = pos.GetSplitPos(divideCount, 2, 4);
            var selectingSceneIndex = FindIndex(property.stringValue);
            var index = EditorGUI.Popup(popUpPos, selectingSceneIndex, ScenePathList);
            if (selectingSceneIndex != index)
            {
                property.stringValue = index >= 0
                    ? EditorBuildSettings.scenes[index].path
                    : "";
            }
        }
    }
}
