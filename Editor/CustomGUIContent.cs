using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Editors
{
    /// <summary>
    /// 追加のパラメータを渡せるようにしたGUIContent
    /// </summary>
    public class CustomGUIContent : GUIContent
    {
        public object Parameter { get; set; }

        public CustomGUIContent() { }
        public CustomGUIContent(GUIContent src) : base(src) { }
        public CustomGUIContent(string text) : base(text) { }
        public CustomGUIContent(Texture image) : base(image) { }
        public CustomGUIContent(string text, string tooltip) : base(text, tooltip) { }
        public CustomGUIContent(string text, Texture image) : base(text, image) { }
        public CustomGUIContent(Texture image, string tooltip) : base(image, tooltip) { }
        public CustomGUIContent(string text, Texture image, string tooltip) : base(text, image, tooltip) { }
    }
}
