using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Hinode.Editors
{
    /// <summary>
	/// <seealso cref="Array2D{T}"/>
	/// </summary>
    public class Array2DEditor<T>
    {
        static readonly GUIContent SIZE_LABEL = new GUIContent("Size");
        static readonly GUIContent SHIFT_OFFSET_LABEL = new GUIContent("ShiftOffset");
        static readonly GUIContent PAGE_OFFSET_LABEL = new GUIContent("Show Offset");
        static readonly GUIContent COUNT_PER_PAGE_LABEL = new GUIContent("Count/Page");

        public delegate void OnChangedDelegate(Array2D<T> inst, ValueKind kinds);

        SmartDelegate<OnChangedDelegate> _onChanged = new SmartDelegate<OnChangedDelegate>();
        public NotInvokableDelegate<OnChangedDelegate> OnChanged { get => _onChanged; }

        [System.Flags]
        public enum ValueKind
        {
            Size = 0x1 << 0,
            Data = 0x1 << 1,
            PageParams = 0x1 << 2,
        }

        Array2D<T> Target { get; set; }
        FieldInfo FieldInfo { get; set; }
        public Vector2Int EditingSize { get; set; }
        public Vector2Int EditingShiftOffset { get; set; }
        public Vector2Int PrevShiftOffset { get; set; }

        public ValueKind ChangedValueKinds { get; set; }

        /// <summary>
        /// 画面にDataを表示する最大数
        /// </summary>
        public Vector2Int CountPerPage { get; set; } = Vector2Int.one * 10;

        /// <summary>
        /// 画面に表示するDataの左上の位置のオフセット
        /// </summary>
        public Vector2Int PageOffset { get; set; } = Vector2Int.zero;

        public bool IsReadOnly { get; set; } = false;
        public bool DoChanged { get => ChangedValueKinds != 0; }

        public bool DoChangedSize { get => Target.Width != EditingSize.x || Target.Height != EditingSize.y; }
        public bool DoChangedShiftOffset { get => EditingShiftOffset.x != 0 || EditingShiftOffset.y != 0; }

        public GUIContent RootLabel { get; set; }
        public bool Foldout { get; set; } = true;

        public System.Func<T, (GUIStyle style, GUILayoutOption[] options)> DataLayoutGetter { get; set; } = (_) => (null, null);

        public Array2DEditor()
            : this(new GUIContent("Array2D"))
        { }

        public Array2DEditor(GUIContent label)
        {
            RootLabel = label;
        }

        public bool Draw(Array2D<T> target, FieldInfo fieldInfo = null, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if(Target != target)
            {
                EditingSize = new Vector2Int(target.Width, target.Height);
                EditingShiftOffset = PrevShiftOffset = Vector2Int.zero;
                CountPerPage = Vector2Int.Min(EditingSize, Vector2Int.one * 10);
                PageOffset = Vector2Int.zero;
            }

            Target = target;
            FieldInfo = fieldInfo;
            ChangedValueKinds = 0;

            Foldout = EditorGUILayout.Foldout(Foldout, RootLabel);
            if (!Foldout) return DoChanged;

            using (var indent = new EditorGUI.IndentLevelScope())
            {
                EditingSize = EditorGUILayout.Vector2IntField(SIZE_LABEL, EditingSize);
                EditingShiftOffset = EditorGUILayout.Vector2IntField(SHIFT_OFFSET_LABEL, EditingShiftOffset);

                if (!IsReadOnly && (DoChangedSize || DoChangedShiftOffset) && GUILayout.Button("Update Size"))
                {
                    ChangedValueKinds |= ValueKind.Size;
                    Target.Resize(EditingSize.x, EditingSize.y, EditingShiftOffset.x, EditingShiftOffset.y);
                    PrevShiftOffset = EditingShiftOffset;
                    EditingShiftOffset = Vector2Int.zero;

                    CountPerPage = Vector2Int.Min(EditingSize, Vector2Int.one * 10);

                    _onChanged.SafeDynamicInvoke(Target, ValueKind.Size, () => "Fail in Array2D<T>#Draw(Size)...");
                }

                if (DrawData() && !IsReadOnly)
                {
                    ChangedValueKinds |= ValueKind.Data;
                    _onChanged.SafeDynamicInvoke(Target, ValueKind.Data, () => "Fail in Array2D<T>#Draw(Data)...");
                }
            }

            return DoChanged;
        }

        bool DrawData()
        {
            bool doChanged = false;

            var boardSize = new Vector2Int(Target.Width, Target.Height);
            EditorGUILayout.LabelField("Show UI");
            using (var indent = new EditorGUI.IndentLevelScope())
            {
                var doChangePageParams = false;
                var newCountPerPage = Vector2Int.Max(
                    Vector2Int.zero,
                    Vector2Int.Min(boardSize, EditorGUILayout.Vector2IntField(COUNT_PER_PAGE_LABEL, CountPerPage)));

                doChangePageParams |= newCountPerPage.x != CountPerPage.x || newCountPerPage.y != CountPerPage.y;
                CountPerPage = newCountPerPage;
                var newPageOffset = Vector2Int.Max(
                    Vector2Int.zero,
                    Vector2Int.Min(boardSize - CountPerPage, EditorGUILayout.Vector2IntField(PAGE_OFFSET_LABEL, PageOffset))
                );

                doChangePageParams |= newPageOffset.x != PageOffset.x || newPageOffset.y != PageOffset.y;
                if(doChangePageParams)
                {
                    _onChanged.SafeDynamicInvoke(Target, ValueKind.PageParams, () => "Fail in Array2D<T>#DrawData(PageParams)...");
                    doChanged = true;
                }
            }

            var size = Vector2Int.Min(CountPerPage, boardSize);
            for (var yy = 0; yy < size.y; ++yy)
            {
                var y = yy + PageOffset.y;
                using (var h = new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                {
                    for (var xx = 0; xx < size.x; ++xx)
                    {
                        var x = xx + PageOffset.x;
                        var value = Target[x, y];
                        //call EditorGUILayout.XXXField()
                        var layout = DataLayoutGetter(value);
                        if (EditorGUIFields.DrawField(value, out object newValue, FieldInfo, layout.style, layout.options))
                        {
                            if (!IsReadOnly)
                            {
                                Target[x, y] = (T)newValue;
                                doChanged = true;
                            }
                        }
                    }
                }
            }

            DataLayoutGetter = (_) => (null, null);
            return doChanged;
        }
    }
}
