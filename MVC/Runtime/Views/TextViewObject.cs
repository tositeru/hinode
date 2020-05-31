using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hinode.MVC
{
    /// <summary>
    /// TODO TextViewObjectのFontの設定
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(Text))]
    [AvailableModelViewParamBinder(typeof(TextViewObject.FixedParamBinder))]
    [DisallowMultipleComponent()]
    public class TextViewObject : RectTransformViewObject
        , RectTransformViewObject.IOptionalViewObject
    {
        public static new TextViewObject Create(string name = "Text")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<TextViewObject>();
        }

        public Text Text { get => gameObject.GetOrAddComponent<Text>(); }

        void OnGUI()
        {
            if(Text.font == null)
            {//
                Text.font = GUI.skin.font;
            }
        }

        public new class FixedParamBinder : RectTransformViewObject.FixedParamBinder
            , RectTransformViewObject.IOptionalViewObjectParamBinder
        {
            public bool Contains(Params paramType)
                => Contains(paramType.ToString());

            public FixedParamBinder Set(Params param, object value)
                => Set(param.ToString(), value) as FixedParamBinder;
            public FixedParamBinder Set<T>(Params param, T value)
                => Set(param.ToString(), value) as FixedParamBinder;

            public T Get<T>(Params param)
                => (T)Get(param.ToString());

            public FixedParamBinder Delete(Params param)
                => Delete(param.ToString()) as FixedParamBinder;

            protected override void UpdateImpl(Model model, IViewObject viewObj)
            {
                var text = viewObj as TextViewObject;
                UpdateParams(text);
            }

            #region RectTransform.IOptionalViewObjectParamBinder
            public RectTransformViewObject.IOptionalViewObject AppendTo(GameObject target)
            {
                return target.AddComponent<TextViewObject>();
            }
            #endregion

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/TextViewObject/DefineParamsTemplate.asset
public enum Params
{
AlignByGeometry,
Alignment,
Color,
FontSize,
FontStyle,
HorizontalOverflow,
VerticalOverflow,
LineSpacing,
Text,
SupportRichText,
}

public bool AlignByGeometry { get => Get<bool>(Params.AlignByGeometry); set => Set(Params.AlignByGeometry, value); }
public TextAnchor Alignment { get => Get<TextAnchor>(Params.Alignment); set => Set(Params.Alignment, value); }
public Color Color { get => Get<Color>(Params.Color); set => Set(Params.Color, value); }
public int FontSize { get => Get<int>(Params.FontSize); set => Set(Params.FontSize, value); }
public FontStyle FontStyle { get => Get<FontStyle>(Params.FontStyle); set => Set(Params.FontStyle, value); }
public HorizontalWrapMode HorizontalOverflow { get => Get<HorizontalWrapMode>(Params.HorizontalOverflow); set => Set(Params.HorizontalOverflow, value); }
public VerticalWrapMode VerticalOverflow { get => Get<VerticalWrapMode>(Params.VerticalOverflow); set => Set(Params.VerticalOverflow, value); }
public float LineSpacing { get => Get<float>(Params.LineSpacing); set => Set(Params.LineSpacing, value); }
public string Text { get => Get<string>(Params.Text); set => Set(Params.Text, value); }
public bool SupportRichText { get => Get<bool>(Params.SupportRichText); set => Set(Params.SupportRichText, value); }

void UpdateParams(TextViewObject text)
{
if (Contains(Params.AlignByGeometry)) text.Text.alignByGeometry = AlignByGeometry;
if (Contains(Params.Alignment)) text.Text.alignment = Alignment;
if (Contains(Params.Color)) text.Text.color = Color;
if (Contains(Params.FontSize)) text.Text.fontSize = FontSize;
if (Contains(Params.FontStyle)) text.Text.fontStyle = FontStyle;
if (Contains(Params.HorizontalOverflow)) text.Text.horizontalOverflow = HorizontalOverflow;
if (Contains(Params.VerticalOverflow)) text.Text.verticalOverflow = VerticalOverflow;
if (Contains(Params.LineSpacing)) text.Text.lineSpacing = LineSpacing;
if (Contains(Params.Text)) text.Text.text = Text;
if (Contains(Params.SupportRichText)) text.Text.supportRichText = SupportRichText;
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/TextViewObject/DefineParamsTemplate.asset
        }
    }
}
