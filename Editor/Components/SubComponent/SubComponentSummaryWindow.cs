using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace Hinode.Editors
{
    public class SubComponentSummaryWindow : EditorWindow
    {
        [MenuItem("Hinode/Tools/SubComponent Summary")]
        public static void Open()
        {
            var window = EditorWindow.CreateWindow<SubComponentSummaryWindow>();
            window.Show();
        }

        RootPopup _rootPopup;
        MethodLabelPopup _methodLabelPopup;
        MonoBehaviour _currentRoot;

        bool FoldOutSubComponents { get; set; } = true;
        bool FoldOutSubComponentsHowTo { get; set; } = false;
        Vector2 ScrollPosSubComponents { get; set; }
        string FilterLabel { get => _methodLabelPopup.SelectedLabel; }
        bool EnableFilterLabel { get => !string.IsNullOrEmpty(FilterLabel); }


        bool MatchLabelFilter(MethodLabelAttribute labels)
        {
            if (!EnableFilterLabel) return true;

            return labels.Contains(FilterLabel);
        }

        void OnEnable()
        {
            titleContent = new GUIContent("SubComponent Summary");
            _rootPopup = new RootPopup();
            _currentRoot = _rootPopup.SelectedMonoBehaviour;
            _methodLabelPopup = new MethodLabelPopup(_currentRoot);
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Reflesh Window"))
            {
                OnEnable();
            }

            FoldOutSubComponentsHowTo = EditorGUILayout.Foldout(FoldOutSubComponentsHowTo, "How To");
            if (FoldOutSubComponentsHowTo)
            {
                EditorGUILayout.TextArea(SUB_COMPONENTS_HOW_TO, GUILayout.MinHeight(150));
            }

            if (_rootPopup.Draw(new GUIContent("Root MonoBehaviour")))
            {
                _currentRoot = _rootPopup.SelectedMonoBehaviour;
                if (_currentRoot != null)
                {
                    _methodLabelPopup = new MethodLabelPopup(_currentRoot);
                }
            }

            if (_currentRoot == null) return;

            if (_methodLabelPopup.Draw(new GUIContent("Method Label Filter")))
            {
            }

            FoldOutSubComponents = EditorGUILayout.Foldout(FoldOutSubComponents, "SubComponents");
            if (FoldOutSubComponents)
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(ScrollPosSubComponents))
                {
                    ScrollPosSubComponents = scroll.scrollPosition;

                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        foreach (var subCom in GetSubComponentsInRoot(_currentRoot.GetType()))
                        {
                            DrawSubComponent(subCom.type, subCom.fieldName);
                        }
                    }
                }
            }
        }

        void DrawSubComponent(System.Type subComType, string fieldName)
        {
            EditorGUILayout.LabelField($"{subComType.Name} : {fieldName}");
            using (var indent = new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("MethodLabels");
                using (var methodLabelIndent = new EditorGUI.IndentLevelScope())
                {
                    var methodsWithLabel = GetMethodLabelEnumerable(subComType)
                        .Where(_t => MatchLabelFilter(_t.label));
                    foreach (var (method, label) in methodsWithLabel)
                    {
                        var labels = label.Labels.Select(_l => $"\"{_l}\"")
                            .Aggregate("", (_s, _c) => _s + _c + " ");
                        EditorGUILayout.LabelField($"{method.Name}() -> {labels}");
                    }
                }
            }
        }

        static IEnumerable<(MethodInfo methodInfo, MethodLabelAttribute label)> GetMethodLabelEnumerable(System.Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(_m => (method: _m, label: _m.GetCustomAttribute<MethodLabelAttribute>()))
                .Where(_t => _t.label != null);
        }

        static IEnumerable<(System.Type type, string fieldName)> GetSubComponentsInRoot(System.Type rootType)
        {
            return new SubComponentEnumerable(rootType);
        }

        class SubComponentEnumerable : IEnumerable<(System.Type type, string filedName)>, IEnumerable
        {
            System.Type _target;
            public SubComponentEnumerable(System.Type target)
            {
                _target = target;
            }

            public IEnumerator<(System.Type type, string filedName)> GetEnumerator()
            {
                if (_target == null) yield break;

                if (_target.EqualGenericTypeDefinition(typeof(ISubComponent<>)))
                {//self Draw
                    yield return (_target, "__Root");
                }

                var subComponents = _target.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(_f => _f.FieldType.EqualGenericTypeDefinition(typeof(ISubComponent<>)));

                foreach (var subCom in subComponents)
                {
                    yield return (subCom.FieldType, subCom.Name);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }


        class RootPopup : PopupEditorGUILayout
        {
            (Scene scene, MonoBehaviour com)[] _optionList;

            public MonoBehaviour SelectedMonoBehaviour
            {
                get => SelectedIndex == 0
                    ? null
                    : OptionList[SelectedIndex].com;
            }

            public (Scene scene, MonoBehaviour com)[] OptionList
            {
                get
                {
                    if(_optionList == null)
                    {
                        _optionList = new (Scene, MonoBehaviour)[] { (default, null) }
                            .Concat(
                                SceneExtensions.GetLoadedSceneEnumerable()
                                .SelectMany(_s => _s.GetGameObjectEnumerable()
                                    .SelectMany(_g => _g.GetComponents<MonoBehaviour>())
                                    .Where(_c => _c.GetType().EqualGenericTypeDefinition(typeof(MonoBehaviourWithSubComponents<>)))
                                    .Select(_com => (scene: _s, com: _com)))
                            )
                            .ToArray();
                    }
                    return _optionList;
                }
            }
            protected override string[] CreateDisplayOptionList()
            {
                return OptionList
                    .Select(_c => {
                        return _c.com != null
                            ? $"{_c.scene.name}:{_c.com.name}:{_c.com.GetType().Name}"
                            : $"(none)";
                    })
                    .ToArray();
            }
        }

        class MethodLabelPopup : PopupEditorGUILayout
        {
            public System.Type RootType { get; private set; }
            public string SelectedLabel
            {
                get => SelectedIndex == 0 ? "" : SelectedElement;
            }

            public MethodLabelPopup(MonoBehaviour root)
            {
                RootType = root != null ? root.GetType() : null;
            }

            protected override string[] CreateDisplayOptionList()
            {
                return new string[] { "(none)" }
                    .Concat(
                        GetSubComponentsInRoot(RootType)
                        .SelectMany(_t => GetMethodLabelEnumerable(_t.type)
                            .SelectMany(_tt => _tt.label.Labels)
                        )
                    )
                    .ToArray();
            }
        }

        const string SUB_COMPONENTS_HOW_TO =
@"How to
ここでは選択したMonoBehaviourが持つSubComponentを確認することができます。
確認できる項目は以下のものになります。
- MethodLabel: SubComponentのメンバ関数に指定されているMethodLabelAttributeの一覧を確認できます。

## Reflesh Windowボタン
このWindowはScene上のObjectが変更された際に自動的に更新されませんので、その際はReflesh Windowボタンを押してください。
";
    }
}
