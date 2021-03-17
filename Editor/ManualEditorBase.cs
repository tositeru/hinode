using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ManualEditorBase
    {
        public class OwnerEditor
        {
            public Editor Editor { get; }
            public EditorWindow Window { get; }

            public OwnerEditor(Editor editor)
            {
                Editor = editor;
            }
            public OwnerEditor(EditorWindow window)
            {
                Window = window;
            }

            public void RepaintOwner()
            {
                if(Editor != null)
                {
                    Editor.Repaint();
                }
                if(Window != null)
                {
                    Window.Repaint();
                }
            }

            public static OwnerEditor Create(Editor editor)
                => new OwnerEditor(editor);
            public static OwnerEditor Create(EditorWindow window)
                => new OwnerEditor(window);
        }

        public class LayoutParam
        {
            public GUIStyle Style { get; }
            public GUILayoutOption[] Options { get; }

            public LayoutParam(GUIStyle style, params GUILayoutOption[] options)
            {
                Style = style;
                Options = options;
            }
        }

        public const string LayoutKey_Root = "@Root";

        public OwnerEditor Owner { get; }
        public GUIContent RootLabel { get; }
        public bool RootFoldout { get; set; } = true;

        public Dictionary<string, LayoutParam> UsedLayoutDictionary { get; } = new Dictionary<string, LayoutParam>();
        public LayoutParam RootLayout
        {
            get => UsedLayoutDictionary.ContainsKey(LayoutKey_Root)
                ? UsedLayoutDictionary[LayoutKey_Root]
                : new LayoutParam(GUI.skin.box);
        }

        public ManualEditorBase(OwnerEditor owner, GUIContent rootLabel)
        {
            Assert.IsNotNull(owner);

            Owner = owner;
            RootLabel = rootLabel;
        }
    }

    public static partial class ManualEditorBaseExtensions
    {
        public static ManualEditorBase.OwnerEditor CreateOwnerEditor(this Editor editor)
        {
            return ManualEditorBase.OwnerEditor.Create(editor);
        }

        public static ManualEditorBase.OwnerEditor CreateOwnerEditor(this EditorWindow window)
        {
            return ManualEditorBase.OwnerEditor.Create(window);
        }
    }
}
