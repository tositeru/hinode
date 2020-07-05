//#define USE_UI_ELEMENTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using UnityEngine.Assertions;
using System;

namespace Hinode.Editors.TextTemplateEngines
{
    [CustomEditor(typeof(TextTemplateEngine))]
    public class TextTemplateEngineEditor : Editor
    {
#if USE_UI_ELEMENTS
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
#else

        Common.CustomEditorParam _param;
        private void OnEnable()
        {
            _param = new Common.CustomEditorParam(serializedObject);
            _param.onClickedGenerateButton = () =>
            {
                var t = serializedObject.targetObject as TextTemplateEngine;
                _param.generatedText = t.Generate();
            };
        }
        public override void OnInspectorGUI()
        {
            Common.OnInspectorGUI(_param);
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }

    [CustomPropertyDrawer(typeof(TextTemplateEngine))]
    public class TextTemplateEnginePropertyDrawer : PropertyDrawer
    {
#if USE_UI_ELEMENTS
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
#else
#endif
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

Keywords)

「$〇〇$」と'$'で囲まれたキーワードをテキスト内に埋め込むことで、キーワードに対応したパラメータの値が展開されます。
キーワードは'Keywords'で入力することができます。
一つのキーワードには複数の値を指定することができ、もしそのようなキーワードが存在する時は全てのキーワードの値の組み合わせが出力されます。

IgnorePairs)
この際、組み合わせたくないものがある場合は'Ignore Pairs'に指定してください。
指定の仕方は'Ignore Pairs'の'Element X'に無視したい値の組み合わせを入力することでできます。
    ex) Keyword1: Apple Orange
        Keyword2: Grape Banana がある時に Keyword1=Orange, Keyword2=Grapeの組み合わせを無視したい時は
        Ignored Pairに
            Keyword1=Orange,
            Keyword2=Grape
        を指定してください

Embbed Templates)
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

パラメータ例
1)
テンプレート: This $Item$ is $Condition$.
キーワード: Item -> Pen, Grass
          Condition -> Good, Nice
無視リスト: 0: Item=Pen, Condition=Nice
埋め込み: なし

結果:
This Pen is Good.
This Grass is Good.
This Grass is Nice.

2)
テンプレート: This $Item$ is $Condition$. %OtherTemplate%
キーワード: Item -> Pen, Grass
          Condition -> Good, Nice
無視リスト: 0: Item=Grass
埋め込み: OtherTemplate -> Go! Go! Go!

結果:
This Pen is Good. Go! Go! Go!
This Pen is Nice. Go! Go! Go!

";

        public enum PropType
        {
            TemplateText,
            ReplacementKeywords,
            IsOnlyEmbbed,
            DoShareKeywords,
            Keywords,
            IgnorePairs,
            EmbbedTemplates,
            IsSingleKeywordPairMode,
            SingleKeywordPairList,
            Newline,
        }
        static readonly (PropType, string)[] props = {
            (PropType.TemplateText, "_templateText"),
            (PropType.ReplacementKeywords, "_replacementKeywords"),
            (PropType.IsOnlyEmbbed, "_isOnlyEmbbed"),
            (PropType.IsSingleKeywordPairMode, "_isSingleKeywordPairMode"),
            (PropType.DoShareKeywords, "_doShareKaywords"),
            (PropType.Keywords, "_keywords"),
            (PropType.IgnorePairs, "_ignorePairs"),
            (PropType.SingleKeywordPairList, "_singleKeywordPairList"),
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

#if USE_UI_ELEMENTS
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
#else
        public class CustomEditorParam
        {
            public SerializedPropertyDictionary<PropType> propDict;
            public Vector2 rootScrollPos;
            public string generatedText;
            public bool usageTextFoldout = true;
            public Vector2 usageScrollPos;

            public System.Action onClickedGenerateButton = () => { };

            public CustomEditorParam(SerializedObject SO)
            {
                propDict = CreatePropDict(SO);
            }
            public CustomEditorParam(SerializedProperty prop)
            {
                propDict = CreatePropDict(prop);
            }
        }

        public static void OnInspectorGUI(CustomEditorParam param)
        {
            //using (var scrollScope = new EditorGUILayout.ScrollViewScope(param.rootScrollPos))
            {
                //param.rootScrollPos = scrollScope.scrollPosition;

                EditorGUILayout.PropertyField(param.propDict[PropType.TemplateText], true);
                EditorGUILayout.ObjectField(param.propDict[PropType.ReplacementKeywords]);
                EditorGUILayout.PropertyField(param.propDict[PropType.IsOnlyEmbbed], true);
                EditorGUILayout.PropertyField(param.propDict[PropType.DoShareKeywords], true);

                EditorGUILayout.PropertyField(param.propDict[PropType.Keywords], true);
                if(param.propDict[PropType.ReplacementKeywords].objectReferenceValue == null)
                {
                    EditorGUILayout.PropertyField(param.propDict[PropType.IsSingleKeywordPairMode], true);
                    if(param.propDict[PropType.IsSingleKeywordPairMode].boolValue)
                    {
                        EditorGUILayout.PropertyField(param.propDict[PropType.SingleKeywordPairList], true);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(param.propDict[PropType.IgnorePairs], true);
                    }
                }
                else
                {
                    var replaceKeywords = param.propDict[PropType.ReplacementKeywords].objectReferenceValue as TextTemplateEngine;
                }

                EditorGUILayout.PropertyField(param.propDict[PropType.EmbbedTemplates], true);
                EditorGUILayout.PropertyField(param.propDict[PropType.Newline], true);

                using (var buttonScope = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Generate"))
                    {
                        param.onClickedGenerateButton();
                    }
                    if (GUILayout.Button("Copy To Clipboard"))
                    {
                        GUIUtility.systemCopyBuffer = param.generatedText;
                        EditorUtility.DisplayDialog("Copy Generated Text", "Copy Generated Text to Clipboard!", "OK");
                    }
                }

                EditorGUILayout.LabelField("Generated Text");
                var style = new GUIStyle(GUI.skin.textArea);
                param.generatedText = EditorGUILayout.TextArea(param.generatedText, style);

                param.usageTextFoldout = EditorGUILayout.Foldout(param.usageTextFoldout, "How To Usage");
                if (param.usageTextFoldout)
                {
                    var usageStyle = new GUIStyle(GUI.skin.label);
                    var height = GUI.skin.label.lineHeight * HOW_TO_LABEL_CONTENT.Where(_c => _c == '\n').Count();
                    EditorGUILayout.LabelField(HOW_TO_LABEL_CONTENT, usageStyle, GUILayout.ExpandHeight(true), GUILayout.Height(height));
                }
            }
        }

#endif
    }

    /// <summary>
    /// PopupFieldのリストを再生成できなさそうなので、文字列を直接入力するようにしている
    /// </summary>
    [CustomPropertyDrawer(typeof(TextTemplateEngine.IgnorePair.Pair))]
    class IgnorePairElementPropertyDrawer : PropertyDrawer
    {
        TextTemplateEngine GetTargetTextTemplateEngine(SerializedProperty prop)
        {
            var textTemplateEngine = prop.GetPropertyPathEnumerable()
                .Select(p => p.instance as TextTemplateEngine)
                .FirstOrDefault(i => i != null);
            if (textTemplateEngine == null)
            {
                textTemplateEngine = prop.serializedObject.targetObject as TextTemplateEngine;
            }
            return textTemplateEngine;
        }

#if USE_UI_ELEMENTS
        //Popup版は一時的に無効化している
#if false
        static readonly string VALUE_POPUP_NAME = "IgnorePairValuePopup";
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement()
            {
                name = "",
            };
            root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            var textTemplateEngine = GetTargetTextTemplateEngine(property);
            Assert.IsNotNull(textTemplateEngine);

            var keywordList = new List<string> { "" };
            keywordList.AddRange(textTemplateEngine.Keywords.Select(k => k.key));
            var keywordPopup = new PopupField<string>(keywordList, 0)
            {
                name = "IgonrePairKeywordPopup",
                bindingPath = $"{property.propertyPath}.keyword",
            };
            keywordPopup.style.flexGrow = new StyleFloat(0.5f);
            keywordPopup.RegisterValueChangedCallback(e => {
                var pairValuePopup = keywordPopup.parent.Children()
                    .First(c => c.name == VALUE_POPUP_NAME) as PopupField<string>;
                string log = "";
                foreach(var child in pairValuePopup.Children())
                {
                    log += $"{child.name}:type={child.GetType().Name} {child.childCount} {child.ElementAt(0).GetType().Name} ";
                }
                Debug.Log($"debug -- value count={pairValuePopup.childCount} log={log}");
                //e.newValue;
            });
            root.Add(keywordPopup);

            var valueList = new List<string> { "", "hoge", "foo", "bar" };
            var valuePopup = new PopupField<string>(valueList, 0)
            {
                name = VALUE_POPUP_NAME,
                bindingPath = $"{property.propertyPath}.value",
            };
            valuePopup.style.flexGrow = new StyleFloat(0.5f);
            root.Add(valuePopup);
            return root;
        }
#else
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement()
            {
                name = "",
            };
            root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            var keywordField = new TextField()
            {
                bindingPath = $"{property.propertyPath}.keyword"
            };
            keywordField.style.flexGrow = new StyleFloat(0.5f);
            root.Add(keywordField);

            var valueField = new TextField()
            {
                bindingPath = $"{property.propertyPath}.value"
            };
            valueField.style.flexGrow = new StyleFloat(0.5f);
            root.Add(valueField);
            return root;
        }
#endif
#else

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var textTemplateEngine = GetTargetTextTemplateEngine(property);
            var keywordProp = property.FindPropertyRelative("keyword");
            var valueProp = property.FindPropertyRelative("value");

            int selectedKeywordIndex = textTemplateEngine.Keywords
                .Zip(Enumerable.Range(0, textTemplateEngine.Keywords.Count()), (_k, _i) => (_k, _i))
                .Where(_t => _t._k.key == keywordProp.stringValue)
                .Select(_t => _t._i).FirstOrDefault();

            int selectedValueIndex = 0;
            if(textTemplateEngine.Keywords.Any())
            {
                selectedValueIndex = textTemplateEngine.Keywords
                    .ElementAt(selectedKeywordIndex)
                    .values.FindIndex(_i => _i == valueProp.stringValue);
            }

            var pos = new GUILayoutPosition(position);
            pos.Indent(false, 0.3f);
            var posWidth = 6;
            var popupWidth = 2;
            bool isUpdated = false;
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                EditorGUI.LabelField(pos.GetSplitPos(posWidth, 0, 1), "Key");

                var keywordList = textTemplateEngine.Keywords.Select(_k => _k.key);
                if (!keywordList.Any()) keywordList = Enumerable.Repeat("", 1);

                selectedKeywordIndex = Mathf.Clamp(selectedKeywordIndex, 0, keywordList.Count()-1);
                var newSelectedKeywordIndex = EditorGUI.Popup(pos.GetSplitPos(posWidth, 1, popupWidth), selectedKeywordIndex, keywordList.ToArray());

                if(newSelectedKeywordIndex != selectedKeywordIndex)
                {
                    isUpdated = true;
                    selectedKeywordIndex = newSelectedKeywordIndex;
                    selectedValueIndex = 0;
                }

                EditorGUI.LabelField(pos.GetSplitPos(posWidth, 3, 1), "Val");
                var valueList = (selectedKeywordIndex < textTemplateEngine.Keywords.Count())
                    ? textTemplateEngine.Keywords.ElementAt(selectedKeywordIndex).values.AsEnumerable()
                    : Enumerable.Repeat("", 1);
                var newSelectedValueIndex = EditorGUI.Popup(pos.GetSplitPos(posWidth, 4, popupWidth), selectedValueIndex, valueList.ToArray());
                if (newSelectedValueIndex != selectedValueIndex)
                {
                    isUpdated = true;
                    selectedValueIndex = newSelectedValueIndex;
                }
            }

            if (isUpdated)
            {
                var keyword = textTemplateEngine.Keywords.ElementAt(selectedKeywordIndex);
                property.FindPropertyRelative("keyword").stringValue = keyword.key;
                property.FindPropertyRelative("value").stringValue = keyword.values[selectedValueIndex];
            }
        }
#endif
    }
}

