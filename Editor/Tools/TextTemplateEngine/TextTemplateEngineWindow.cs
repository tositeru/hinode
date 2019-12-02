//#define USE_UI_ELEMENTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

namespace Hinode.Editors.TextTemplateEngines
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

                //var targetElement = this.rootVisualElement.Q<InspectorElement>(TARGET_INSPECTOR_NAME);
                //if(targetElement != null)
                //{
                //    //Debug.Log($"pass name={targetElement.name}, {TargetSO.targetObject}");
                //    targetElement.Bind(TargetSO);
                //    //targetElement.MarkDirtyRepaint();
                //}
                //var referenceElement = this.rootVisualElement.Q<ObjectField>(TARGET_REFERENCE_NAME);
                //if(referenceElement != null)
                //{
                //    //referenceElement.value = Target;
                //}
            }
        }

        SerializedObject _windowSO;
        SerializedObject _targetSO;

        SerializedObject WindowSO
        {
            get
            {
                if (_windowSO == null)
                {
                    _windowSO = new SerializedObject(this);
                }
                return _windowSO;
            }
        }

        /// <summary>
        /// _targetがNULLの時でも編集できるように空のSerializedObjectを生成するようにしている。
        /// </summary>
        SerializedObject TargetSO
        {
            get => _targetSO != null ? _targetSO : (_targetSO = new SerializedObject(CreateInstance<TextTemplateEngine>()));
        }

#if USE_UI_ELEMENTS
        readonly string TARGET_INSPECTOR_NAME = "TargetInspector";
        readonly string TARGET_REFERENCE_NAME = "TargetReference";

        private void OnEnable()
        {
            var root = rootVisualElement;
            var scrollView = new ScrollView(ScrollViewMode.Vertical);

            var objRef = new ObjectField("Source File")
            {
                name = TARGET_REFERENCE_NAME,
                objectType = typeof(TextTemplateEngine),
                value = Target,
                allowSceneObjects = false,
                bindingPath = "_target",
            };
            objRef.RegisterValueChangedCallback(_e =>
            {
                if (_e.newValue == Target) return;
                Target = _e.newValue as TextTemplateEngine;
            });
            scrollView.Add(objRef);

            ////Test
            //var SO = new SerializedObject(this);
            //var test = new PropertyField(SO.FindProperty("_testKeyObjRef"))
            //{
            //    name = "test",
            //    bindingPath = "_testKeyObjRef",
            //};
            //test.Bind(SO);
            //root.Add(test);
            ////Test

            var fileButtons = new VisualElement();
            fileButtons.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            fileButtons.style.alignContent = new StyleEnum<Align>(Align.Center);
            fileButtons.style.alignItems = new StyleEnum<Align>(Align.Center);
            scrollView.Add(fileButtons);
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

            var targetElement = new PropertyField
            {
                name = TARGET_INSPECTOR_NAME,
                bindingPath = "_target",
            };
            //targetElement.Bind(TargetSO);
            scrollView.Add(targetElement);
            root.Add(scrollView);
            root.Bind(new SerializedObject(this));
        }
#else
        Common.CustomEditorParam _param;
        Vector2 _rootScrollPos;
        private void OnGUI()
        {
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_rootScrollPos))
            {
                _rootScrollPos = scrollScope.scrollPosition;

                var newTarget = EditorGUILayout.ObjectField(_target, typeof(TextTemplateEngine), false) as TextTemplateEngine;
                if(newTarget != Target)
                {
                    Target = newTarget;
                    _param = new Common.CustomEditorParam(TargetSO);
                }
                using (var buttonScope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("File");
                    if (GUILayout.Button("Save To File"))
                    {
                        SaveToFile();
                    }
                }

                if(_param == null)
                {
                    _param = new Common.CustomEditorParam(TargetSO);
                }

                _param.onClickedGenerateButton = () =>
                {
                    _param.generatedText = this.Target.Generate();
                };

                Common.OnInspectorGUI(_param);
                TargetSO.ApplyModifiedProperties();
            }
        }
#endif

        void SaveToFile()
        {
            var filepath = EditorUtility.SaveFilePanelInProject("Save Text Template", "textTemplate", "asset", "Save Text Template File(.asset)");
            if (filepath.Length <= 0) return;

            Debug.Log($"save to {filepath}");
            TargetSO.ApplyModifiedProperties();
            var assetPath = AssetDatabase.GetAssetPath(TargetSO.targetObject);
            if (assetPath.Length <= 0)
            {
                AssetDatabase.CreateAsset(TargetSO.targetObject, filepath);
                Target = TargetSO.targetObject as TextTemplateEngine;
            }
            else
            {
                if (AssetDatabase.CopyAsset(assetPath, filepath))
                {
                    var newAsset = AssetDatabase.LoadAssetAtPath<TextTemplateEngine>(filepath);
                    Target = newAsset;
                }
            }
            EditorUtility.DisplayDialog("Save Text Template", $"Save to {filepath}", "OK");
        }
    }
}
