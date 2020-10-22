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

        bool FoldoutLabelFilters { get; set; } = true;
        bool FoldOutSubComponents { get; set; } = true;
        bool FoldOutSubComponentsHowTo { get; set; } = false;

        Vector2 ScrollPosSubComponents { get; set; }
        string FilterLabel { get => _methodLabelPopup.SelectedLabel; }
        bool EnableFilterLabel { get => !string.IsNullOrEmpty(FilterLabel); }

        GameObject CheckPrefab { get; set; } = null;


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

            using (var _h1 = new EditorGUILayout.HorizontalScope())
            {
                if (_rootPopup.Draw(new GUIContent("Root MonoBehaviour")))
                {
                    _currentRoot = _rootPopup.SelectedMonoBehaviour;
                    if (_currentRoot != null)
                    {
                        _methodLabelPopup = new MethodLabelPopup(_currentRoot);
                    }
                }
                EditorGUILayout.ObjectField(_currentRoot, typeof(MonoBehaviour), true, GUILayout.MaxWidth(150));
            }

            if (_currentRoot == null) return;

            using (var scroll = new EditorGUILayout.ScrollViewScope(ScrollPosSubComponents))
            {
                ScrollPosSubComponents = scroll.scrollPosition;

                var labelFilterInfo = _currentRoot.GetType().GetProperty("ControllerLabelFilters");
                var labelFilters = labelFilterInfo.GetValue(_currentRoot) as ControllerLabelFilter[];

                FoldoutLabelFilters = EditorGUILayout.Foldout(FoldoutLabelFilters, $"LabelObjects in {(CheckPrefab != null ? "Prefab" : "Scene")}");
                if(FoldoutLabelFilters)
                {
                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        CheckPrefab = EditorGUILayout.ObjectField(new GUIContent("Check Prefab Root"), CheckPrefab, typeof(GameObject), false)
                            as GameObject;

                        IEnumerable<LabelObject> labelObjs;
                        if(CheckPrefab == null)
                        {
                            var selfScene = SceneExtensions.GetLoadedSceneEnumerable()
                                .First(_s => _s.GetRootGameObjects().Any(_o => _o == _currentRoot.gameObject));
                            labelObjs = selfScene.GetGameObjectEnumerable()
                                .Select(_o => _o.GetComponent<LabelObject>())
                                .Where(_l => _l != null);
                        }
                        else
                        {
                            labelObjs = CheckPrefab.GetComponentsInChildren<LabelObject>();
                        }

                        foreach (var label in labelObjs)
                        {
                            using (var h = new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                            {
                                EditorGUILayout.ObjectField(label, label.GetType(), true);

                                var txt = label.Labels.Concat(label.InitialLabels).Aggregate("", (_s, _c) => _s + _c + " ");
                                EditorGUILayout.LabelField($"{txt}");

                                var matchFilter = labelFilters.FirstOrDefault(_f => _f.DoMatch(label, out var _));
                                //EditorGUILayout.Toggle("DoMatchFilter?", matchFilter != null);

                                var controllerMethods = _currentRoot.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .Select(_m => (method: _m, label: _m.GetCustomAttribute<MethodLabelAttribute>()))
                                    .Where(_t => _t.label != null)
                                    .Where(_t => matchFilter.MethodLabels.Count == _t.label.Labels.Count
                                        && matchFilter.MethodLabels.All(_l => _t.label.Contains(_l)));

                                EditorGUILayout.Popup(new GUIContent("Bind Methods"), 0, controllerMethods.Select(_t => _currentRoot.GetType().Name + "#" +  _t.method.Name).ToArray());
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                FoldOutSubComponents = EditorGUILayout.Foldout(FoldOutSubComponents, "SubComponents");
                if (FoldOutSubComponents)
                {
                    using (var indent = new EditorGUI.IndentLevelScope())
                    {
                        _methodLabelPopup.Draw(new GUIContent("Method Label Filter"));
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
ここでは選択したMonoBehaviourWithSubComponent<T>が持つSubComponentを確認することができます。
確認できる項目は以下のものになります。
- 関連するLabelObjects:
    選択中のMonoBehaviourWithSubComponent<T>が所属するSceneにあるLabelObject Componentを確認できます。
    or Check Prefab Root　を指定した時はそのPrefabにあるLabelObject Componentが確認できます。

    確認できる項目は以下のものです。
    - Initial Labels
    - バインドされる可能性がある選択中のMonoBehaviourWithSubComponent<T>のメソッド

    メソッドのバインドの自動化は実装されていないため、Script上から設定してください。
    その際は、MonoBehaviourWithSubComponent<T>#CreateControllerLabelFilters()とControllerLabelFilter classを利用してください。

- MethodLabel: SubComponentのメンバ関数に指定されているMethodLabelAttributeの一覧を確認できます。

## Reflesh Windowボタン
このWindowはScene上のObjectが変更された際に自動的に更新されませんので、その際はReflesh Windowボタンを押してください。
";
    }
}
