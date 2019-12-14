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

