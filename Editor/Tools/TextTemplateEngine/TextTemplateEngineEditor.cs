using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Hinode.Editors.TextTemplateEngines
{
    [CustomEditor(typeof(TextTemplateEngine))]
    public class TextTemplateEngineEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var propDict = Common.CreatePropDict(serializedObject);
            var root = Common.CreateElement(propDict);
            var generatedButton = root.Q<Button>(Common.GeneratedButtonName);
            var generatedText = root.Q<TextField>(Common.GeneratedTextName);
            if(generatedButton != null)
            {
                generatedButton.clickable.clicked += () =>
                {
                    var t = serializedObject.targetObject as TextTemplateEngine;
                    generatedText.value = t.Generate();
                };
            }
            return root;
        }
    }

    [CustomPropertyDrawer(typeof(TextTemplateEngine))]
    public class TextTemplateEnginePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference
                || property.objectReferenceValue == null
                || !property.objectReferenceValue.GetType().Equals(typeof(TextTemplateEngine)))
            {
                var r = new VisualElement();
                r.Add(new Label("This PropertyDrawer must be use 'TextTemplateEngine'..."));
                return r;
            }

            var propDict = Common.CreatePropDict(property);
            var root = Common.CreateElement(propDict);
            var generatedButton = root.Q<Button>(Common.GeneratedButtonName);
            var generatedText = root.Q<TextField>(Common.GeneratedTextName);
            if (generatedButton != null)
            {
                generatedButton.clickable.clicked += () =>
                {
                    var t = property.objectReferenceValue as TextTemplateEngine;
                    generatedText.value = t.Generate();
                };
            }

            return root;
        }
    }

    public static class Common
    {
        public static readonly string TemplateTextName = "TemplateText";
        public static readonly string KeywordsName = "Keywords";
        public static readonly string IgnorePairsName = "Ignore Pairs";
        public static readonly string EmbbedDictionaryName = "Embbed Dictionary";
        public static readonly string NewlineName = "Insert Newline Type";
        public static readonly string GeneratedButtonName = "GeneratedButton";
        public static readonly string CopyToClipboardButtonName = "CopyToClipboardButton";
        public static readonly string GeneratedTextName = "GeneratedText";

        static readonly string HOW_TO_LABEL_CONTENT =
@"このツールはテキストテンプレートエンジンになります。
「$〇〇$」と'$'で囲まれたキーワードをテキスト内に埋め込むことで、キーワードに対応したパラメータの値が展開されます。
キーワードは'Keywords'で入力することができます。
一つのキーワードには複数の値を指定することができ、もしそのようなキーワードが存在する時は全てのキーワードの値の組み合わせが出力されます。

この際、組み合わせたくないものがある場合は'Ignore Pairs'に指定してください。
指定の仕方は'Ignore Pairs'の'Element X'に無視したい値の組み合わせを入力することでできます。
    ex) Keyword1: Apple Orange
        Keyword2: Grape Banana がある時に Keyword1=Orange, Keyword2=Grapeの組み合わせを無視したい時は
        Ignored Pairに
            Keyword1=Orange,
            Keyword2=Grape
        を指定してください

また、他のテキストテンプレートのものを本文中に埋め込むことも可能です。
その際は「%〇〇%」と'%'で埋め込みたいテキストテンプレートのキーを囲み、'Embbed Templates'に埋め込んだキーを入力してください

使い方)
1. テンプレートとなるテキストを入力する
1-1. キーワードを埋め込む際は'$'で括ってください
2. キーワードに対応した値を入力する
3. キーワードの内、無視したいペアのものを指定する
4. Generateボタンを押すことで、テキストが出力されます。
option 1. 'Copy To Clipboard'を押すことでクリップボードに出力されたテキストをコピーできます。
option 2. 'Newline'で出力されるテキストの改行コードを指定できます。
option 3. (EditorWindowのみ)'Save To File'ボタンを押すことで、現在のパラメータをUnityアセットとして保存することが可能です。
";
        public static VisualElement CreateElement(SerializedPropertyDictionary<PropType> propDict)
        {
            var container = new VisualElement();

            var scrollView = new ScrollView(ScrollViewMode.Vertical);

            scrollView.Add(propDict.GetPropField(Common.PropType.TemplateText, TemplateTextName, "_templateText"));
            scrollView.Add(propDict.GetPropField(Common.PropType.Keywords, KeywordsName, "_keywords"));
            scrollView.Add(propDict.GetPropField(Common.PropType.IgnorePairs, IgnorePairsName, "_ignorePairs"));
            scrollView.Add(propDict.GetPropField(Common.PropType.EmbbedTemplates, EmbbedDictionaryName, "_embbedTemplates"));
            scrollView.Add(propDict.GetPropField(Common.PropType.Newline, NewlineName, "_newline"));

            var buttons = new VisualElement();
            buttons.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            buttons.style.alignContent = new StyleEnum<Align>(Align.Center);
            buttons.style.alignItems = new StyleEnum<Align>(Align.Center);
            scrollView.Add(buttons);

            var generatedButton = new Button
            {
                name = GeneratedButtonName,
                text = "Generate",
            };
            generatedButton.style.flexGrow = new StyleFloat(1);
            buttons.Add(generatedButton);

            var copytoClipboardButton = new Button
            {
                name = CopyToClipboardButtonName,
                text = "Copy To Clipboard",
            };
            copytoClipboardButton.style.flexGrow = new StyleFloat(1);
            buttons.Add(copytoClipboardButton);

            var generatedLabel = new Label("Generated Text");
            scrollView.Add(generatedLabel);
            var generatedText = new TextField(99999, true, false, '*')
            {
                name = GeneratedTextName,
            };
            scrollView.Add(generatedText);

            var howToLabelFoldout = new Foldout {
                text = "How To Usage",
            };
            var howtoScrollView = new ScrollView(ScrollViewMode.Horizontal);
            var howToLabel = new Label(HOW_TO_LABEL_CONTENT);
            howtoScrollView.Add(howToLabel);
            howToLabelFoldout.Add(howtoScrollView);
            scrollView.Add(howToLabelFoldout);
            container.Add(scrollView);

            copytoClipboardButton.clickable.clicked += () =>
            {
                GUIUtility.systemCopyBuffer = generatedText.value;
                EditorUtility.DisplayDialog("Copy Generated Text", "Copy Generated Text to Clipboard!", "OK");
            };

            return container;
        }

        public enum PropType
        {
            TemplateText,
            Keywords,
            IgnorePairs,
            EmbbedTemplates,
            Newline,
        }
        static readonly (PropType, string)[] props = {
            (PropType.TemplateText, "_templateText"),
            (PropType.Keywords, "_keywords"),
            (PropType.IgnorePairs, "_ignorePairs"),
            (PropType.EmbbedTemplates, "_embbedTemplates"),
            (PropType.Newline, "_newline"),
        };
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedObject SO)
        {
            return new SerializedPropertyDictionary<PropType>(SO, props);
        }
        public static SerializedPropertyDictionary<PropType> CreatePropDict(SerializedProperty parentProp)
        {
            return new SerializedPropertyDictionary<PropType>(parentProp, props);
        }
    }
}

