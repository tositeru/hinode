using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

namespace Hinode.Editors
{
    public class TextTemplateEngineWindow : EditorWindow
    {
        [SerializeField] TextTemplateEngine _target;
        TextTemplateEngine Target
        {
            get => _target;
            set
            {
                _target = value;
                _targetSO = new SerializedObject(_target != null
                    ? _target
                    : CreateInstance<TextTemplateEngine>());

                var targetElement = this.rootVisualElement.Q<InspectorElement>(TARGET_INSPECTOR_NAME);
                if(targetElement != null)
                {
                    targetElement.Bind(TargetSO);
                }
                var referenceElement = this.rootVisualElement.Q<ObjectField>(TARGET_REFERENCE_NAME);
                if(referenceElement != null)
                {
                    referenceElement.value = Target;
                }
            }
        }

        SerializedObject _targetSO;
        /// <summary>
        /// _targetがNULLの時でも編集できるように空のSerializedObjectを生成するようにしている。
        /// </summary>
        SerializedObject TargetSO
        {
            get => _targetSO != null ? _targetSO : (_targetSO = new SerializedObject(CreateInstance<TextTemplateEngine>()));
        }

        [MenuItem("Hinode/Tools/Text Template Engine")]
        public static void Open()
        {
            var window = CreateWindow<TextTemplateEngineWindow>("Text Template Engine");
            window.Show();
        }

        readonly string TARGET_INSPECTOR_NAME = "TargetInspector";
        readonly string TARGET_REFERENCE_NAME = "TargetReference";

        private void OnEnable()
        {
            var root = rootVisualElement;
            var objRef = new ObjectField("Source File")
            {
                name = TARGET_REFERENCE_NAME,
                objectType = typeof(TextTemplateEngine),
                value = Target,
                allowSceneObjects = false,
            };
            objRef.RegisterValueChangedCallback(_e =>
            {
                if (_e.newValue == Target) return;
                Target = _e.newValue as TextTemplateEngine;
            });
            root.Add(objRef);

            //Test
            var SO = new SerializedObject(this);
            var test = new PropertyField(SO.FindProperty("_testKeyObjRef"))
            {
                name = "test",
                bindingPath = "_testKeyObjRef",
            };
            test.Bind(SO);
            root.Add(test);
            //Test

            var fileButtons = new VisualElement();
            fileButtons.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            fileButtons.style.alignContent = new StyleEnum<Align>(Align.Center);
            fileButtons.style.alignItems = new StyleEnum<Align>(Align.Center);
            root.Add(fileButtons);
            var label = new Label("File");
            label.style.flexGrow = new StyleFloat(0.5f);
            fileButtons.Add(label);
            var saveButton = new Button()
            {
                name = "saveToFileButton",
                text = "Save To File",
            };
            saveButton.style.flexGrow = new StyleFloat(1);
            saveButton.clickable.clicked += () =>
            {
                var filepath = EditorUtility.SaveFilePanelInProject("Save Text Template", "textTemplate", "asset", "Save Text Template File(.asset)");
                if (filepath.Length <= 0) return;

                Debug.Log($"save to {filepath}");
                TargetSO.ApplyModifiedProperties();
                var assetPath = AssetDatabase.GetAssetPath(TargetSO.targetObject);
                if(assetPath.Length <= 0)
                {
                    AssetDatabase.CreateAsset(TargetSO.targetObject, filepath);
                    Target = TargetSO.targetObject as TextTemplateEngine;
                }
                else
                {
                    if(AssetDatabase.CopyAsset(assetPath, filepath))
                    {
                        var newAsset = AssetDatabase.LoadAssetAtPath<TextTemplateEngine>(filepath);
                        Target = newAsset;
                    }
                }
                EditorUtility.DisplayDialog("Save Text Template", $"Save to {filepath}", "OK");
            };
            fileButtons.Add(saveButton);

            var targetElement = new InspectorElement
            {
                name = TARGET_INSPECTOR_NAME
            };
            targetElement.Bind(TargetSO);
            root.Add(targetElement);
        }
    }
}

