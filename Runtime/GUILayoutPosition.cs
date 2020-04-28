using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hinode
{
    /// <summary>
    /// EditorGUIで使用する描画位置を計算するクラス
    /// </summary>
    public class GUILayoutPosition
    {
        Rect _pos;
        public Rect Pos { get => _pos; private set => _pos = value; }
        public float RowHeight { get; private set; }
        public GUILayoutPosition(Rect position, float rowHeight=-1)
        {
            Pos = position;
            RowHeight = rowHeight <= 0 ? Pos.height : rowHeight;
        }

        public Rect GetSplitPos(float divideCount, int index, int width=1)
        {
            var p = Pos;
            p.width /= divideCount;
            p.x += p.width * index;
            p.width *= width;
            return p;
        }

        public void IncrementRow(float height=-1)
        {
            var H = height > 0 ? height : RowHeight;
            GUILayout.Space(H);
            _pos.y += H;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 指定したSerializedPropertyの高さ分、y座標を進める
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="includeChildren">propの子要素の内容も高さに含めるか？</param>
        public void IncrementRow(SerializedProperty prop, bool includeChildren)
        {
            IncrementRow(EditorGUI.GetPropertyHeight(prop, includeChildren));
        }
#endif
        /// <summary>
        /// インデントを加える
        /// </summary>
        /// <param name="isAuto">Unity標準のインデントを加える(Editorのみ)</param>
        /// <param name="percent">インデントに加えるオフセット。現在の横幅からの割合(%)</param>
        /// <returns></returns>
        public GUILayoutPosition Indent(bool isAuto, float percent=0.1f)
        {
            if(isAuto)
            {
#if UNITY_EDITOR
                return new GUILayoutPosition(EditorGUI.IndentedRect(_pos), RowHeight);
#else
                percent = 0.1f;
#endif
            }

            var offset = _pos.width * percent;
            var pos = _pos;
            pos.x += offset;
            pos.width -= offset;
            return new GUILayoutPosition(pos, RowHeight);
        }
    }
}
