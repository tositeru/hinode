using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hinode
{
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

        public void IncrementRow()
        {
            GUILayout.Space(RowHeight);
            _pos.y += RowHeight;
        }

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
