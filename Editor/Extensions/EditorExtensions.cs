using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hinode.Editors
{
    public static class EditorExtensions
    {
        /// <summary>
        /// 次に描画する位置を取得する
        /// </summary>
        /// <param name="_"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static Rect GetNextRect(this Editor _, GUIContent label)
        {
            var style = GUI.skin.label;
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            rect.height = style.CalcHeight(label, rect.width);
            return EditorGUI.IndentedRect(rect);
        }
    }
}
